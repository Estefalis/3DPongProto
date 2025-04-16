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
        private PlayerInputActions m_inputActions;
        private InputActionMap m_uiActionMap;

        private PlayerInput m_playerInput;
        //private MenuInputHandler m_menuInputHandler;

        [SerializeField] internal EventSystem m_eventSystem;

        #region MenuNavigation
        #region Select First Elements by using the EventSystem.
        [Header("Select First Elements")]
        [SerializeField] internal Transform m_firstElement;
        private readonly Stack<Transform> m_activeElement = new();
        [Space]

        //Key-/Value-Pair component-arrays to set the selected GameObject for menu navigation with a dictionary.
        [SerializeField] internal Transform[] m_navigationKey;
        [SerializeField] private GameObject[] m_navigationValue;
        [SerializeField] private Button[] m_navigationBackButtons;
        [SerializeField] private Button[] m_settingsBackButtons;
        private readonly Dictionary<Transform, GameObject> m_targetNavigationElement = new();
        private readonly Dictionary<Transform, Button> m_selectableBackButton = new();
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
        internal static event Action<int> AReLoadScene;       //(New)UserInputManager with central SceneManager.LoadScene().
        internal static event Action AEndInfiniteMatch;
        #endregion

        #region Scriptable_Objects
        [Header("Scriptable Objects")]
        [SerializeField] private MatchUIStates m_matchUIStates;
        [SerializeField] private MatchValues m_matchValues;
        #endregion

        private void Awake()
        {
            m_inputActions = new PlayerInputActions();
            m_inputActions.Enable();
            m_uiActionMap = m_inputActions.UserInterface;

            //Because 'OnNavigationInput', 'OnSubmitInput' and 'OnCancelInput' have to be handled differently, this foreach is needed.
            foreach (InputActionMap actionMap in m_inputActions.asset.actionMaps)
            {
                if (m_navigationKey[0].gameObject.activeInHierarchy) //Transform active at Start of StartScene.
                {
                    if (actionMap == m_uiActionMap)
                        actionMap.Enable();
                    else
                        actionMap.Disable();
                }
                else
                    actionMap.Disable();                            //Transform inactive at Start of GameScene.
            }

            //m_menuInputHandler = FindObjectOfType<MenuInputHandler>();

            m_lastSelectedGameObject = null;
            m_targetNavigationElement.Clear();
            m_selectableBackButton.Clear();

            SetFirstStackElement(m_firstElement);
            SetUIElements();    //Also sets the lastSelectedElement.
        }

        private void OnEnable()
        {
            m_inputActions.UserInterface.CloseGameMenu.performed += CloseMenu;

            if (!m_navigationKey[0].gameObject.activeInHierarchy)
            {
                //Navigation and Submit work in Scenes without PlayerInput components. This if-test prevents doubled performed actions.
                m_inputActions.UserInterface.Navigate.performed += OnNavigationInput;
                m_inputActions.UserInterface.Submit.performed += OnSubmitInput;
            }

            m_inputActions.UserInterface.Cancel.performed += OnCancelInput;

            AResumeTheGame += OnResumeTheGame;
            UserInputManager.AChangeActiveActionMap += OnChangeActiveActionMap;
        }

        private void OnDisable()    //Copy content into 'OnDestroy()', if needed.
        {
            m_inputActions.Disable();
            m_inputActions.UserInterface.CloseGameMenu.performed -= CloseMenu;

            //TODO: Make clear, if unsubscriptions also need a if-condition like subscription above.
            m_inputActions.UserInterface.Navigate.performed -= OnNavigationInput;
            m_inputActions.UserInterface.Submit.performed -= OnSubmitInput;

            m_inputActions.UserInterface.Cancel.performed -= OnCancelInput;

            AResumeTheGame -= OnResumeTheGame;
            UserInputManager.AChangeActiveActionMap -= OnChangeActiveActionMap;
        }

        private void Start()
        {
            m_playerInput = FindObjectOfType<PlayerInput>();

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

            if (!m_uiActionMap.enabled)
                m_uiActionMap.Enable();

            //m_menuInputHandler.ActivateMenuInput();
        }

        private void OnCloseMenu()
        {
            SetNavigationGameObject(m_firstElement);

            if (m_inputActions == null)
                return;

            if (m_uiActionMap.enabled)
                m_uiActionMap.Disable();

            #region With MenuInputHandler
            //AResumeTheGame?.Invoke();
            //m_menuInputHandler.DeactivateMenuInput();
            #endregion
        }
        #endregion

        #region Non-Subscription-Custom-Methods
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
            for (int navMenu = 0; navMenu < m_navigationKey.Length; navMenu++)
                m_targetNavigationElement.Add(m_navigationKey[navMenu], m_navigationValue[navMenu]);

            for (int backMenu = 0; backMenu < m_navigationKey.Length; backMenu++)
                m_selectableBackButton.Add(m_navigationKey[backMenu], m_navigationBackButtons[backMenu]);

            SetNavigationGameObject(m_firstElement);
        }

        #region Stack-Methods to (de-)activate Menu-Transforms and set the active UI-Element.
        /// <summary>
        /// 'm_activeElement' Stack requires a set element to start with, to prevent a null error.
        /// </summary>
        /// <param name="_firstElement"></param>
        private void SetFirstStackElement(Transform _firstElement)
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

            if (m_activeElement.Count == 0)
            {
                SetFirstStackElement(m_firstElement);

                if (UserInputManager.SetActionMap == EInputActionMaps.UserInterface.ToString())
                    UserInputManager.ToggleActionMaps(EInputActionMaps.PlayerActions.ToString());
            }

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
                    GameObject selectElement = m_targetNavigationElement[_activeTransform];
                    m_lastSelectedGameObject = selectElement;   //BEFORE active Transform switch, new selectElement replaces the old!
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

        private void NavigateToNextObject(Vector2 _navigationVector)
        {
            Selectable nextSelectable = null;

            if (_navigationVector.y != 0)
            {
                switch (_navigationVector.y < 0)
                {
                    case true:  //-1 Down
                    {
                        if (m_lastSelectedGameObject.TryGetComponent<Selectable>(out var currentSelectable))
                            nextSelectable = currentSelectable.FindSelectableOnDown();
                    }
                    break;
                    case false: //+1 Up
                    {
                        if (m_lastSelectedGameObject.TryGetComponent<Selectable>(out var currentSelectable))
                            nextSelectable = currentSelectable.FindSelectableOnUp();
                    }
                    break;
                }
            }
            else if (_navigationVector.x != 0)
            {
                switch (_navigationVector.x < 0)
                {
                    case true:  //-1 Left
                    {
                        if (m_lastSelectedGameObject.TryGetComponent<Selectable>(out var currentSelectable))
                            nextSelectable = currentSelectable.FindSelectableOnLeft();
                    }
                    break;
                    case false: //+1 Right
                    {
                        if (m_lastSelectedGameObject.TryGetComponent<Selectable>(out var currentSelectable))
                            nextSelectable = currentSelectable.FindSelectableOnRight();
                    }
                    break;
                }
            }

            if (nextSelectable != null && nextSelectable.gameObject.activeInHierarchy)
                m_eventSystem.SetSelectedGameObject(nextSelectable.gameObject);
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
            UserInputManager.ToggleActionMaps(EInputActionMaps.PlayerActions.ToString());
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
                m_navigationKey[0].gameObject.SetActive(false);
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

        #region CallbackContext_Methods
        private void CloseMenu(InputAction.CallbackContext _callbackContext)
        {
            var sceneIndex = SceneManager.GetActiveScene().buildIndex;
            if (sceneIndex == (int)ESceneNames.StartMenu)   //TODO: Add other 'menoOnly' Scenes.
                return;

            if (m_firstElement.gameObject.activeInHierarchy && UserInputManager.SetActionMap == EInputActionMaps.UserInterface.ToString())
                AResumeTheGame?.Invoke();
        }

        private void OnNavigationInput(InputAction.CallbackContext _callbackContext)
        {
            if (m_playerInput == null && !m_uiActionMap.enabled)    //Only pass in GameScenes if PauseMenu is opened and PlayerInputs exist.
                return;

            if (_callbackContext.control.device is Keyboard)        //Because Keyboard works (currently).
                return;

            NavigateToNextObject(_callbackContext.ReadValue<Vector2>());
        }

        private void OnSubmitInput(InputAction.CallbackContext _callbackContext)
        {
            if (m_playerInput == null && !m_uiActionMap.enabled)    //Only pass in GameScenes if PauseMenu is opened and PlayerInputs exist.
                return;

            if (_callbackContext.control.device is Keyboard)        //Because Keyboard works (currently).
                return;

            if (_callbackContext.ReadValueAsButton())
                ExecuteEvents.Execute(m_lastSelectedGameObject, new BaseEventData(m_eventSystem), ExecuteEvents.submitHandler);
        }

        private void OnCancelInput(InputAction.CallbackContext _callbackContext)
        {
            if (/*m_playerInput == null && */!m_uiActionMap.enabled)    //Only pass if PauseMenu is opened.
                return;

            //if (_callbackContext.control.device is Keyboard)        //Because Keyboard works (currently).
            //    return;

            if (_callbackContext.ReadValueAsButton())
            {
                Button backButton = null;
                foreach (Transform transform in m_navigationKey)
                {
                    if (transform.gameObject.activeInHierarchy)
                    {
                        backButton = m_selectableBackButton[transform];
                        if (!backButton.gameObject.activeInHierarchy)       //Other active button as Fallback.
                        {
                            for (int i = 0; i < m_settingsBackButtons.Length; i++)
                            {
                                if (m_settingsBackButtons[i].gameObject.activeInHierarchy)
                                {
                                    backButton = m_settingsBackButtons[i];
                                    break;
                                }
                            }
                        }
                        break;
                    }
                }

                if (!m_navigationValue[0].activeInHierarchy)
                    ExecuteEvents.Execute(backButton.gameObject, new BaseEventData(m_eventSystem), ExecuteEvents.submitHandler);
            }
        }
        #endregion
    }
}