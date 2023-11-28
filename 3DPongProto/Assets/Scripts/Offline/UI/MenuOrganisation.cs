using System;
using System.Collections.Generic;
using ThreeDeePongProto.Offline.Player.Inputs;
using ThreeDeePongProto.Offline.Settings;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ThreeDeePongProto.Offline.UI
{
    public class MenuOrganisation : MonoBehaviour
    {
        private PlayerInputActions m_menuActions;
        //[SerializeField] private string m_loadWinScene;
        //[SerializeField] private string m_loadLoseScene;
        [SerializeField] private EventSystem m_eventSystem;
        private readonly string m_startMenuScene = "StartMenuScene";

        #region Select First Elements by using the EventSystem.
        // The stack of active (Transform-)elements for menu-navigation needs to have a Start-Transform to prevent an error. It gets set active in 'Awake()'.
        [Header("Select First Elements")]
        [SerializeField] private Transform m_firstElement;
        private Stack<Transform> m_activeElement = new();

        //Key-/Value-Pair component-arrays to set the selected GameObject for menu navigation with a dictionary.
        [SerializeField] private Transform[] m_keyTransform;
        [SerializeField] private GameObject[] m_valueGameObject;
        private Dictionary<Transform, GameObject> m_selectedElement = new Dictionary<Transform, GameObject>();
        #endregion

        #region Alpha-Buttons and SubPages.
        //I could not leave this undone. Only the clicked button shall be dominant, the others should visibly stay in the back.
        [Header("Button Alpha-Values")]
        [SerializeField] private Button[] m_alphaButtons;
        [SerializeField, Range(0.1f, 0.9f)] private float m_reducedAlphaValue = 0.5f;
        [SerializeField, Range(0.5f, 1f)] private float m_maxAlphaValue = 1f;

        //Simply just to (de-)activate the corresponding Transforms for each Settings-Category.
        [SerializeField] private Transform[] m_subPageTransforms;
        #endregion

        [SerializeField] private Button m_hiddenFinishButton;
        //MatchManager unpauses the Game. - PlayerController restarts Coroutines and Inputsystem.PlayerActions.
        public static event Action CloseInGameMenu;
        //MatchManager unpauses the Game.
        public static event Action RestartGameLevel;
        //MatchManager unpauses the Game.
        public static event Action OnLoadMainScene;

        #region Scriptable Variables
        [Header("Scriptable Variables")]
        [SerializeField] private VolumeUIStates m_volumeUIStates;
        [SerializeField] private VolumeUIValues m_volumeUIValues;

        [SerializeField] private GraphicUiStates m_graphicUiStates;
        [SerializeField] private MatchUIStates m_matchUIStates;
        [SerializeField] private MatchValues m_matchValues;
        [SerializeField] private BasicFieldValues m_basicFieldValues;
        #endregion

        #region Serialization
        private readonly string m_settingStatesFolderPath = "/SaveData/Settings-States";
        private readonly string m_settingsValuesFolderPath = "/SaveData/Settings-Values";
        private readonly string m_fieldSettingsPath = "/SaveData/FieldSettings";
        private readonly string m_volumeFileName = "/Volume";
        private readonly string m_graphicFileName = "/Graphic";
        private readonly string m_matchFileName = "/Match";
        private readonly string m_fileFormat = ".json";

        public static event Action EndInfiniteMatch;

        private IPersistentData m_persistentData = new SerializingData();
        private bool m_encryptionEnabled = false;
        #endregion

        private void Awake()
        {
            SetUIElements();
            LoadGraphicSettings();
            LoadVolumeSettings();
            LoadMatchSettings();

            if (m_hiddenFinishButton != null)
            {
                InVisibleButton(m_matchUIStates.InfiniteMatch);
            }
        }

        /// <summary>
        /// PlayerController and UIControls need to be moved into 'Start()' and the PlayerInputActions of the UserInputManager into 'Awake()', to prevent Exceptions.
        /// </summary>
        private void Start()
        {
            m_menuActions = UserInputManager.m_playerInputActions;
            m_menuActions?.Enable();
            m_menuActions.PlayerActions.ToggleGameMenu.performed += EnableNavigation;
        }

        private void OnDisable()
        {
            m_menuActions?.Disable();
            m_menuActions.PlayerActions.ToggleGameMenu.performed -= EnableNavigation;
        }

        private void EnableNavigation(InputAction.CallbackContext _callbackContext)
        {
            if (!m_firstElement.gameObject.activeInHierarchy)
            {
                m_firstElement.gameObject.SetActive(true);
                SetSelectedElement(m_firstElement);
            }
        }

        private void SetUIElements()
        {
            SetFirstElement(m_firstElement);

            for (int i = 0; i < m_keyTransform.Length; i++)
                m_selectedElement.Add(m_keyTransform[i], m_valueGameObject[i]);

            SetSelectedElement(m_firstElement);
        }

        private void LoadGraphicSettings()
        {
            GraphicUiSettingsStates uiIndices = m_persistentData.LoadData<GraphicUiSettingsStates>(m_settingStatesFolderPath, m_graphicFileName, m_fileFormat, m_encryptionEnabled);
            m_graphicUiStates.QualityLevelIndex = uiIndices.QualityLevelIndex;
            m_graphicUiStates.SelectedResolutionIndex = uiIndices.SelectedResolutionIndex;
            m_graphicUiStates.FullScreenMode = uiIndices.FullScreenMode;
            m_graphicUiStates.SetCameraMode = uiIndices.SetCameraMode;
        }

        private void LoadVolumeSettings()
        {
            VolumeUISettingsStates uiIndices = m_persistentData.LoadData<VolumeUISettingsStates>(m_settingStatesFolderPath, m_volumeFileName, m_fileFormat, m_encryptionEnabled);
            VolumeUISettingsValues uiValues = m_persistentData.LoadData<VolumeUISettingsValues>(m_settingsValuesFolderPath, m_volumeFileName, m_fileFormat, m_encryptionEnabled);

            m_volumeUIStates.MasterMuteIsOn = uiIndices.MasterMuteIsOn;
            m_volumeUIStates.BGMMuteIsOn = uiIndices.BGMMuteIsOn;
            m_volumeUIStates.SFXMuteIsOn = uiIndices.SFXMuteIsOn;

            m_volumeUIValues.LatestMasterVolume = uiValues.LatestMasterVolume;
            m_volumeUIValues.LatestBGMVolume = uiValues.LatestBGMVolume;
            m_volumeUIValues.LatestSFXVolume = uiValues.LatestSFXVolume;
        }

        private void LoadMatchSettings()
        {
            MatchUISettingsStates uiIndices = m_persistentData.LoadData<MatchUISettingsStates>(m_settingStatesFolderPath, m_matchFileName, m_fileFormat, m_encryptionEnabled);
            m_matchUIStates.InfiniteMatch = uiIndices.InfiniteMatch;
            //In this special case, the DropdownIndex is equal to the set Rounds and MaxPoints, because Index 0 is set as "InfinityValue".
            m_matchUIStates.LastRoundDdIndex = uiIndices.LastRoundDdIndex;
            m_matchUIStates.LastMaxPointDdIndex = uiIndices.LastMaxPointDdIndex;
            m_matchUIStates.MaxRounds = uiIndices.MaxRounds;
            m_matchUIStates.MaxPoints = uiIndices.MaxPoints;

            m_matchUIStates.FixRatio = uiIndices.FixRatio;
            m_matchUIStates.LastFieldWidthDdIndex = uiIndices.LastFieldWidthDdIndex;
            m_matchUIStates.LastFieldLengthDdIndex = uiIndices.LastFieldLengthDdIndex;

            m_matchUIStates.TPOneBacklineDdIndex = uiIndices.TPOneBacklineDdIndex;
            m_matchUIStates.TPTwoBacklineDdIndex = uiIndices.TPTwoBacklineDdIndex;
            m_matchUIStates.TPOneFrontlineDdIndex = uiIndices.TPOneFrontlineDdIndex;
            m_matchUIStates.TPTwoFrontlineDdIndex = uiIndices.TPTwoFrontlineDdIndex;

            BasicFieldSetup basicFieldSetup = m_persistentData.LoadData<BasicFieldSetup>(m_fieldSettingsPath, m_matchFileName, m_fileFormat, m_encryptionEnabled);
            m_basicFieldValues.SetGroundWidth = basicFieldSetup.SetGroundWidth;
            m_basicFieldValues.SetGroundLength = basicFieldSetup.SetGroundLength;
            m_basicFieldValues.FrontlineAdjustment = basicFieldSetup.FrontLineAdjustment;
            m_basicFieldValues.BacklineAdjustment = basicFieldSetup.BackLineAdjustment;
        }

        public void ResumeGame()
        {
            CloseInGameMenu?.Invoke();
            m_firstElement.gameObject.SetActive(false);
            UserInputManager.ToggleActionMaps(UserInputManager.m_playerInputActions.PlayerActions);
        }

        public void RestartLevel()
        {
            RestartGameLevel?.Invoke();
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            UserInputManager.ToggleActionMaps(UserInputManager.m_playerInputActions.PlayerActions);
        }

        public void ReturnToMainScene()
        {
            //Action to reset timescale inside the Matchmanager. And other possible settings on returning to the main menu scene.
            OnLoadMainScene?.Invoke();

            //if (m_startMenuScene == "StartMenuScene" && GameManager.Instance.EGameConnectionModi == EGameModi.LocalPC)
            if (m_startMenuScene == "StartMenuScene" && m_matchValues.EGameConnectionModi == EGameModi.LocalPC)
                SceneManager.LoadScene(m_startMenuScene);
            else
                Debug.Log("Der SzenenName ist nicht mehr aktuell. Bitte aktualisieren! Danke~.");   /*Ggf. SzenenName eingeben lassen.*/
            //TODO: Also disconnect from Network in Online-Seasons.
        }

        public void QuitGameIngame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        public void EndOfInfiniteMatch()
        {
            if (m_matchValues.TotalPointsTPOne > 0 || m_matchValues.TotalPointsTPTwo > 0)
            {
                EndInfiniteMatch?.Invoke();
                m_keyTransform[0].gameObject.SetActive(false);
            }
        }

        #region Methods to (de-)activate Menu-Transforms with a stack and to set the active Element in each UI-Window.
        public void NextElement(Transform _next)
        {
            Transform currentElement = m_activeElement.Peek();
            currentElement.gameObject.SetActive(false);

            m_activeElement.Push(_next);
            _next.gameObject.SetActive(true);

            SetSelectedElement(_next);
        }

        public void CloseToPreviousElement()
        {
            Transform currentElement = m_activeElement.Pop();
            currentElement.gameObject.SetActive(false);

            Transform previousElement = m_activeElement.Peek();
            previousElement.gameObject.SetActive(true);

            SetSelectedElement(previousElement);
        }

        /// <summary>
        /// The Transform gets used as the 'Key' to find the correct 'Value' in the dictionary.
        /// </summary>
        /// <param name="_activeTransform"></param>
        protected void SetSelectedElement(Transform _activeTransform)
        {
            GameObject selectElement = m_selectedElement[_activeTransform];
            m_eventSystem.SetSelectedGameObject(selectElement);
        }

        protected void SetFirstElement(Transform _firstElement)
        {
            m_activeElement.Push(_firstElement);
        }
        #endregion

        /// <summary>
        /// Each Button pressed sets the visibly activated/deactivated Button and enables/disables the corresponding Settings-SubPage.
        /// </summary>
        /// <param name="_sender"></param>
        public void SetButtonAlpha(Button _sender)
        {
            for (int i = 0; i < m_alphaButtons.Length; i++)
            {
                if (_sender == m_alphaButtons[i])
                {
                    m_subPageTransforms[i].gameObject.SetActive(true);
                    Color tempAlpha1 = m_alphaButtons[i].image.color;
                    tempAlpha1.a = m_maxAlphaValue;
                    m_alphaButtons[i].image.color = tempAlpha1;
                }
                else
                {
                    m_subPageTransforms[i].gameObject.SetActive(false);
                    Color tempAlpha05 = m_alphaButtons[i].image.color;
                    tempAlpha05.a = m_reducedAlphaValue;
                    m_alphaButtons[i].image.color = tempAlpha05;
                }
            }
        }

        private void InVisibleButton(bool _infiniteMatch)
        {
            switch (_infiniteMatch)
            {
                case true:
                    m_hiddenFinishButton.gameObject.SetActive(true);
                    break;
                case false:
                    m_hiddenFinishButton.gameObject.SetActive(false);
                    break;
            }
        }
    }
}