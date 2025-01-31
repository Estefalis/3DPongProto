using System;
using System.Collections.Generic;
using ThreeDeePongProto.Shared.InputActions;
using ThreeDeePongProto.Shared.Managers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ThreeDeePongProto.Offline.UI.Menu
{
    public class MenuManager : MonoBehaviour
    {
        private PlayerInputActions m_playerInputActions;
        [SerializeField] internal EventSystem m_eventSystem;

        #region MenuNavigation
        #region Select First Elements by using the EventSystem.
        [Header("Select First Elements")]
        [SerializeField] private Transform m_firstElement;
        internal Transform FirstElement { get => m_firstElement; }
        private Stack<Transform> m_activeElement = new();
        [Space]

        //Key-/Value-Pair component-arrays to set the selected GameObject for menu navigation with a dictionary.
        [SerializeField] internal Transform[] m_keyTransform;
        [SerializeField] private GameObject[] m_valueGameObject;
        private Dictionary<Transform, GameObject> m_selectedElement = new Dictionary<Transform, GameObject>();
        #endregion

        #region Alpha-Buttons and SubPages.
        //I could not leave this undone. Only the clicked button shall be dominant, the others should visibly stay in the back.
        [Header("Button Alpha-Values")]
        [SerializeField, Range(0.1f, 0.9f)] private float m_reducedAlphaValue = 0.5f;
        [SerializeField, Range(0.5f, 1f)] private float m_maxAlphaValue = 1f;
        [Space]
        [SerializeField] private Button[] m_alphaButtons;
        //Simply just to (de-)activate the corresponding Transforms for each Settings-Category.
        [SerializeField] private Transform[] m_subPageTransforms;
        #endregion

        internal static GameObject LastSelectedGameObject { get => m_lastSelectedGameObject; }
        private static GameObject m_lastMenuSceneObject, m_lastGameSceneObject, m_lastSelectedGameObject;
        #endregion

        #region GameScene-Variables
        [Header("GameScene Variables")]
        [SerializeField] private Button m_hiddenFinishButton;
        private const string m_startMenuScene = "StartMenuScene";

        //LocalMatchManager unpauses the Game. - CharacterMainController restarts Coroutines and Inputsystem.PlayerActions.
        public static event Action ResumeTheGame;
        public static event Action OnLoadMainScene;
        public static event Action EndInfiniteMatch;

        private const string m_userInterfaceName = "UserInterface";
        #endregion

        #region Scriptable_Objects
        [Header("Scriptable Objects")]
        [SerializeField] private MatchUIStates m_matchUIStates;
        [SerializeField] private MatchValues m_matchValues;
        #endregion

        private void Awake()
        {
            if (m_eventSystem == null)
                m_eventSystem = EventSystem.current;

            m_lastSelectedGameObject = null;
            m_selectedElement.Clear();

            SetFirstStackElement(m_firstElement);
            SetUIElements();    //Also sets the lastSelectedElement.

            //m_playerInputActions = RebindManager.m_PlayerInputActions;
            m_playerInputActions = new PlayerInputActions();
            m_playerInputActions.UserInterface.Enable();
        }

        private void OnEnable()
        {
            m_playerInputActions.UserInterface.ToggleGameMenu.performed += CloseMenu;
            RebindManager.m_changeActiveActionMap += OnInputManagerChangedActionMap;
        }

        private void OnDisable()
        {
            m_playerInputActions.UserInterface.ToggleGameMenu.performed -= CloseMenu;
            RebindManager.m_changeActiveActionMap -= OnInputManagerChangedActionMap;

            m_playerInputActions.UserInterface.Disable();
        }

        private void OnDestroy()
        {
            m_playerInputActions.UserInterface.ToggleGameMenu.performed -= CloseMenu;
            RebindManager.m_changeActiveActionMap -= OnInputManagerChangedActionMap;

            m_playerInputActions.Dispose();
        }

        private void Start()
        {
            if (m_hiddenFinishButton != null)
                InVisibleButton(m_matchUIStates.InfiniteMatch); //m_matchUIStates get load in LoadSettingsValues > Awake().
        }

        private void Update()
        {
            UpdateLastSelectedObject();
            //Debug.Log($"RebindManager: {RebindManager.m_PlayerInputActions.UserInterface.enabled} - MenuManager: {m_playerInputActions.UserInterface.enabled}");
        }

        #region Custom-Methods
        private void UpdateLastSelectedObject()
        {
            switch (m_eventSystem.currentSelectedGameObject == null)
            {
                case false:
                {
                    switch (m_lastSelectedGameObject != m_eventSystem.currentSelectedGameObject)
                    {
                        case false:
                            break;
                        case true:
                        {
                            m_lastSelectedGameObject = RebindManager.m_PlayerInputActions.UserInterface.enabled ? m_lastMenuSceneObject = m_eventSystem.currentSelectedGameObject : m_lastGameSceneObject = m_eventSystem.currentSelectedGameObject;
                            break;
                        }
                    }
                    break;
                }
                case true:
                {
                    switch (m_lastSelectedGameObject == null)
                    {
                        case false:
                            m_eventSystem.SetSelectedGameObject(m_lastSelectedGameObject);
                            break;
                        case true:
                            break;
                    }
                    break;
                }
            }
#if UNITY_EDITOR
            //Debug.Log(m_lastSelectedGameObject);
#endif
        }

        private void SetUIElements()
        {
            for (int i = 0; i < m_keyTransform.Length; i++)
                m_selectedElement.Add(m_keyTransform[i], m_valueGameObject[i]);

            SetNavigationGameObject(m_firstElement);
        }

        #region Stack-Methods to (de-)activate Menu-Transforms and set the active UI-Element.
        /// <summary>
        /// 'm_activeElement' Stack requires a set element to start with, to prevent a null error.
        /// </summary>
        /// <param name="_firstElement"></param>
        protected void SetFirstStackElement(Transform _firstElement)
        {
            m_activeElement.Push(_firstElement);
        }

        public void NextElement(Transform _next)
        {
            Transform currentElement = m_activeElement.Peek();
            currentElement.gameObject.SetActive(false);

            m_activeElement.Push(_next);
            _next.gameObject.SetActive(true);

            SetNavigationGameObject(_next);
        }

        public void CloseToPreviousElement()
        {
            Transform currentElement = m_activeElement.Pop();
            currentElement.gameObject.SetActive(false);

            Transform previousElement = m_activeElement.Peek();
            previousElement.gameObject.SetActive(true);

            SetNavigationGameObject(previousElement);
        }

        /// <summary>
        /// Sets the lastSelected Transform and GameObject from the dictionary required for navigation in each new enabled Transform.
        /// </summary>
        /// <param name="_activeTransform"></param>
        internal void SetNavigationGameObject(Transform _activeTransform)
        {
            switch (_activeTransform == null)
            {
                case false:
                {
                    GameObject selectElement = m_selectedElement[_activeTransform];
                    m_lastSelectedGameObject = selectElement;   //Set this before the eventSystem. Else inactive Transforms lastGO stays selected.
                    m_eventSystem.SetSelectedGameObject(selectElement);
                    break;
                }
                case true:
                    return;
            }
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
        #endregion

        #region MenuButton-Methods
        public void ResumeGame()
        {
            RebindManager.ToggleActionMaps(RebindManager.m_PlayerInputActions.PlayerActions);
            ResumeTheGame?.Invoke();
            m_firstElement.gameObject.SetActive(false);
        }

        public void RestartGameScene()
        {
            //LocalMatchManager toggles ActionMap in ReSetMatch()-method on Scene-reload.
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void ReturnToMainScene()
        {
            OnLoadMainScene?.Invoke();
            //Action to reset timescale inside the Matchmanager. And other possible settings on returning to the main menu scene.
            SceneManager.LoadScene(m_startMenuScene);
        }

        public void EndOfInfiniteMatch()
        {
            if (m_matchValues.TotalPointsTPOne > 0 || m_matchValues.TotalPointsTPTwo > 0)
            {
                EndInfiniteMatch?.Invoke();
                m_keyTransform[0].gameObject.SetActive(false);
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

        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
        #endregion

        private void CloseMenu(InputAction.CallbackContext _callbackContext)
        {
            RebindManager.ToggleActionMaps(RebindManager.m_PlayerInputActions.PlayerActions);
        }

        private void OnInputManagerChangedActionMap(InputActionMap _inputActionMap)
        {
            switch (_inputActionMap.name == m_userInterfaceName)
            {
                case true:
                    m_playerInputActions.UserInterface.Enable();
                    break;
                case false:
                    m_playerInputActions.UserInterface.Disable();
                    break;
            }
            Debug.Log("!");
            if (!m_firstElement.gameObject.activeInHierarchy && m_playerInputActions.UserInterface.enabled)
            {
                m_firstElement.gameObject.SetActive(true);
                SetNavigationGameObject(m_firstElement);
            }

            if (m_firstElement.gameObject.activeInHierarchy && !m_playerInputActions.UserInterface.enabled)
            {
                ResumeGame();
            }
        }
    }
}