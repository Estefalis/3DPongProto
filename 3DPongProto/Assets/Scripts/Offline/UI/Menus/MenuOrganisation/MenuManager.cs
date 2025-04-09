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
        [Header("InputActions")]
        private PlayerInputActions m_inputActions;

        private InputActionMap m_uiActionMap;
        private InputActionMap m_playerActionMap;

        //private UserInputManager m_userInputManager;
        [SerializeField] internal EventSystem m_eventSystem;

        #region MenuNavigation
        #region Select First Elements by using the EventSystem.
        [Header("Select First Elements")]
        [SerializeField] private Transform m_firstElement;
        private readonly Stack<Transform> m_activeElement = new();
        [Space]

        //Key-/Value-Pair component-arrays to set the selected GameObject for menu navigation with a dictionary.
        [SerializeField] internal Transform[] m_keyTransform;
        [SerializeField] private GameObject[] m_valueGameObject;
        private readonly Dictionary<Transform, GameObject> m_selectedElement = new();
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
        private static GameObject m_lastSelectedGameObject;
        #endregion

        #region GameScene-Variables
        [Header("GameScene Variables")]
        [SerializeField] private Button m_hiddenFinishButton;

        internal static event Action AResumeTheGame;          //LocalMatchManager unpauses the Game.
        internal static event Action<int> AReLoadScene;       //UserInputManager with central SceneManager.LoadScene().
        internal static event Action AEndInfiniteMatch;
        #endregion

        #region Scriptable_Objects
        [Header("Scriptable Objects")]
        [SerializeField] private MatchUIStates m_matchUIStates;
        [SerializeField] private MatchValues m_matchValues;
        #endregion

        private void Awake()
        {
            //m_userInputManager = FindObjectOfType<UserInputManager>();
            m_inputActions = new PlayerInputActions();

            if (m_inputActions == null)
            {
                Debug.LogError("Unable to instantiate PlayerInputActions!", this);
                enabled = false;
                return;
            }

            m_uiActionMap = m_inputActions.UserInterface;
            m_playerActionMap = m_inputActions.PlayerActions;
            m_uiActionMap.Disable();
            m_playerActionMap.Disable();

            m_lastSelectedGameObject = null;
            m_selectedElement.Clear();

            SetFirstStackElement(m_firstElement);
            SetUIElements();    //Also sets the lastSelectedElement.
        }

        private void OnEnable()
        {
            m_inputActions.UserInterface.CloseGameMenu.performed += CloseMenu;
            AResumeTheGame += OnResumeTheGame;
            UserInputManager.AChangeActiveActionMap += OnChangeActiveActionMap;
        }

        private void OnDisable()
        {
            m_uiActionMap.Disable();
            m_inputActions.UserInterface.CloseGameMenu.performed -= CloseMenu;
            AResumeTheGame -= OnResumeTheGame;
            UserInputManager.AChangeActiveActionMap -= OnChangeActiveActionMap;
        }

        private void OnDestroy()
        {
            m_inputActions.Disable();
            m_inputActions.UserInterface.CloseGameMenu.performed -= CloseMenu;
            AResumeTheGame -= OnResumeTheGame;
            UserInputManager.AChangeActiveActionMap -= OnChangeActiveActionMap;
        }

        private void Start()
        {
            if (m_hiddenFinishButton != null)
                InVisibleButton(m_matchUIStates.InfiniteMatch);     //m_matchUIStates get load in LoadSettingsValues > Awake().
        }

        private void Update()
        {
#if UNITY_EDITOR
            //Debug.Log($"UIMap enabled: {m_uiActionMap.enabled} | PlayerMap enabled: {m_playerActionMap.enabled}."); 
#endif
            UpdateLastSelectedObject();
        }

        #region Non_InputAction_Subscriptions
        private void OnResumeTheGame()
        {
            if (m_firstElement.gameObject.activeInHierarchy)
            {
                UserInputManager.ToggleActionMaps(EInputActionMaps.PlayerActions.ToString());
                m_firstElement.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Method to react on ActionMap changes triggered via static method 'ToggleActionMaps'.
        /// </summary>
        /// <param name="_actionMap"></param>
        private void OnChangeActiveActionMap(string _actionMap)
        {
            var sceneIndex = SceneManager.GetActiveScene().buildIndex;
            if (sceneIndex == (int)ESceneNames.StartMenu)   //Or in other menuOnly Scenes.
                return;

            if (_actionMap == EInputActionMaps.UserInterface.ToString())
            {
                OnOpenMenu();
            }
            else if (_actionMap == EInputActionMaps.PlayerActions.ToString())
            {
                OnCloseMenu();
            }
        }

        private void OnOpenMenu()
        {
            if (!m_firstElement.gameObject.activeInHierarchy)
            {
                m_firstElement.gameObject.SetActive(true);
                SetNavigationGameObject(m_firstElement);
            }

            if (m_inputActions == null)
                return;

            m_uiActionMap.Enable();
            m_playerActionMap.Disable();
        }

        private void OnCloseMenu()
        {
            if (m_inputActions == null)
                return;

            m_uiActionMap.Disable();
            m_playerActionMap.Enable();
        }
        #endregion

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
                            m_lastSelectedGameObject = m_eventSystem.currentSelectedGameObject;
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

        #region MenuButton_Methods
        public void ResumeGame()
        {
            AResumeTheGame?.Invoke();
        }

        public void RestartGameScene()
        {
            var sceneIndex = SceneManager.GetActiveScene().buildIndex;
            AReLoadScene?.Invoke(sceneIndex);
        }

        public void ReturnToMainMenu()
        {
            AReLoadScene?.Invoke((int)ESceneNames.StartMenu);   //Possible without '?.Invoke'?
        }

        public void EndOfInfiniteMatch()
        {
            if (m_matchValues.TotalPointsTPOne > 0 || m_matchValues.TotalPointsTPTwo > 0)
            {
                AEndInfiniteMatch?.Invoke();
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
            var sceneIndex = SceneManager.GetActiveScene().buildIndex;
            if (sceneIndex == (int)ESceneNames.StartMenu)   //TODO: Add other 'menoOnly' Scenes.
                return;

            if (m_firstElement.gameObject.activeInHierarchy && UserInputManager.SetActionMap == EInputActionMaps.UserInterface.ToString())
                AResumeTheGame?.Invoke();
        }
    }
}