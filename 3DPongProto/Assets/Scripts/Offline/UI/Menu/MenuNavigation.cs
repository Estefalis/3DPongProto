using System;
using System.Collections.Generic;
using ThreeDeePongProto.Shared.InputActions;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ThreeDeePongProto.Offline.UI.Menu
{
    public class MenuNavigation : MonoBehaviour
    {
        private PlayerInputActions m_menuActions;
        [SerializeField] internal MenuOrganisation m_menuOrganisation;

        #region Select First Elements by using the EventSystem.
        // The stack of active (Transform-)elements for menu-navigation needs to have a Start-Transform to prevent an error. It gets set active in 'Awake()'.
        [Header("Select First Elements")]
        [SerializeField] private Transform m_firstElement;
        private Stack<Transform> m_activeElement = new();
        private Transform m_lastSelectedTransform;

        //Key-/Value-Pair component-arrays to set the selected GameObject for menu navigation with a dictionary.
        [SerializeField] private Transform[] m_keyTransform;
        [SerializeField] private GameObject[] m_valueGameObject;
        private Dictionary<Transform, GameObject> m_selectedElement = new Dictionary<Transform, GameObject>();
        #endregion

        private readonly string m_startMenuScene = "StartMenuScene";

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

        #region Scriptables
        [Header("Scriptables")]
        [SerializeField] private MatchUIStates m_matchUIStates;
        [SerializeField] private MatchValues m_matchValues;
        [SerializeField] private PlayerIDData[] m_playerIDData;
        #endregion

        public static event Action EndInfiniteMatch;

        private void Awake()
        {
            SetUIElements();

            if (m_hiddenFinishButton != null)
            {
                InVisibleButton(m_matchUIStates.InfiniteMatch);
            }
        }

        private void OnDisable()
        {
            m_menuActions?.Disable();
            m_menuActions.PlayerActions.ToggleGameMenu.performed -= EnableMenuNavigation;
        }

        /// <summary>
        /// PlayerController and UIControls need to be moved into 'Start()' and the PlayerInputActions of the InputManager into 'Awake()', to prevent Exceptions.
        /// </summary>
        private void Start()
        {
            m_menuActions = InputManager.m_playerInputActions;
            m_menuActions?.Enable();
            m_menuActions.PlayerActions.ToggleGameMenu.performed += EnableMenuNavigation;

            PreSetUpPlayerAmount(m_matchUIStates.EPlayerAmount);
        }

        private void Update()
        {
            if (EventSystem.current.currentSelectedGameObject == null && InputManager.m_playerInputActions.UI.enabled)
            {
                SetSelectedElement(m_lastSelectedTransform);
            }
        }

        private void EnableMenuNavigation(InputAction.CallbackContext _callbackContext)
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
        
        /// <summary>
        /// Required, so MatchSettings right at start can fill the Front-/Backline-Dropdowns.
        /// </summary>
        /// <param name="_ePlayerAmount"></param>
        private void PreSetUpPlayerAmount(EPlayerAmount _ePlayerAmount)
        {
            if (m_matchUIStates.GameRuns)
                return;

            m_matchValues.PlayerData.Clear();
            m_matchValues.PlayerData = new();

            uint playerAmount = (uint)_ePlayerAmount;    //EPlayerAmount.Four => int 4 || EPlayerAmount.Two => int 2
            for (uint i = 0; i < playerAmount; i++)
            {
                m_matchValues.PlayerData.Add(m_playerIDData[(int)i]);
            }
        }

        public void ResumeGame()
        {
            CloseInGameMenu?.Invoke();
            m_firstElement.gameObject.SetActive(false);
            InputManager.ToggleActionMaps(InputManager.m_playerInputActions.PlayerActions);
        }

        public void RestartLevel()
        {
            RestartGameLevel?.Invoke();
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            InputManager.ToggleActionMaps(InputManager.m_playerInputActions.PlayerActions);
        }

        public void ReturnToMainScene()
        {
            //Action to reset timescale inside the Matchmanager. And other possible settings on returning to the main menu scene.
            OnLoadMainScene?.Invoke();
            SceneManager.LoadScene(m_startMenuScene);
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
            m_lastSelectedTransform = _activeTransform; //Update reselects first selected Button once, if none is selected anymore.
            GameObject selectElement = m_selectedElement[_activeTransform];
            m_menuOrganisation.m_eventSystem.SetSelectedGameObject(selectElement);
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