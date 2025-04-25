using System;
using System.Collections.Generic;
using ThreeDeePongProto.Shared.InputActions;
using ThreeDeePongProto.Shared.Managers;
using TMPro;
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
        private readonly Dictionary<Transform, GameObject> m_targetNavigationElement = new();
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

        [Header("Mouse Cursor")]
        [SerializeField] private CursorLockMode m_cursorLockMode = CursorLockMode.Confined;
        [SerializeField] private bool m_showCursor = true;

        //[SerializeField] private float m_stickDeadZoneMin = 0.1f;
        //[SerializeField] private float m_stickDeadZoneMax = 0.5f;
        internal static GameObject LastSelectedGameObject { get => m_lastSelectedGameObject; }
        private static GameObject m_lastSelectedGameObject;
        #endregion

        [SerializeField] private Button m_hiddenFinishButton;

        private TMP_InputField m_lastSelectedInputField = null;
        private bool m_fieldIsInEditMode = false;

        #region Actions_and_Functions
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
            Cursor.lockState = m_cursorLockMode;
            Cursor.visible = m_showCursor;

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

            m_lastSelectedGameObject = null;
            m_targetNavigationElement.Clear();

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
            if (!Application.isFocused)  //TODO: Test, if Navigation still works, if game is not focussed.
                return;

            UpdateLastSelectedObject();

            HandleButtonPresses();
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
            if (!Application.isFocused)
                return;

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
        }

        private void OnCloseMenu()
        {
            SetNavigationGameObject(m_firstElement);

            if (m_inputActions == null)
                return;

            if (m_uiActionMap.enabled)
                m_uiActionMap.Disable();
        }
        #endregion

        #region Non-Subscription-Custom-Methods
        private void UpdateLastSelectedObject()
        {
            switch (m_eventSystem.currentSelectedGameObject == null)
            {
                case false:
                {
                    //Moment, when the previous saved GO still differs from the 'up to date' selected Object from the eventSystem.
                    switch (m_lastSelectedGameObject != m_eventSystem.currentSelectedGameObject)
                    {
                        case true:
                        {
                            //1st check for TMP_Inputfield before replacing the latest selected FallBack-GameObject.
                            InputFieldCheck(m_lastSelectedGameObject);
                            //Moment, when the previous saved GO is made equal to the selected Object from the eventSystem.
                            m_lastSelectedGameObject = m_eventSystem.currentSelectedGameObject;
                            break;
                        }
                        case false:
                        {
                            //2nd check for TMP_Inputfield after replacing the latest selected FallBack-GameObject.
                            InputFieldCheck(m_lastSelectedGameObject);
                            break;
                        }
                    }
                    break;
                }
                case true:
                {
                    switch (m_lastSelectedGameObject == null)
                    {
                        case true:
                            break;
                        case false:
                            m_eventSystem.SetSelectedGameObject(m_lastSelectedGameObject);
                            break;
                    }
                    break;
                }
            }
#if UNITY_EDITOR
            //Debug.Log(m_lastSelectedGameObject);
#endif
        }

        private void InputFieldCheck(GameObject _incomingGameObject)
        {
            //If '_incomingGameObject' is a TMP_InputField.
            if (_incomingGameObject.TryGetComponent<TMP_InputField>(out var inputField))
            {
                //true == newest G0 from InputFieldCheck() || false == previous saved GO from UpdateLastSelectedObject().
                switch (inputField.gameObject == m_eventSystem.currentSelectedGameObject)
                {
                    case true:  //GO is the newest, current selected Object AND a TMP_InputField.
                    {
                        if (m_lastSelectedInputField == null)
                        {
                            m_lastSelectedInputField = inputField;  //Save the latest selected TMP_InputField for later changes.
#if UNITY_EDITOR
                            Debug.Log($"switch current IF: {m_lastSelectedInputField.name}.");
#endif
                            m_lastSelectedInputField.image.color = m_lastSelectedInputField.colors.selectedColor;
                        }

                        switch (m_fieldIsInEditMode)
                        {
                            case false:
                            {
                                #region Cursor doesn't activate, but Player has still to press Submit to move to next GO.
                                //m_eventSystem.SetSelectedGameObject(m_lastSelectedInputField.gameObject);
                                //m_lastSelectedInputField.enabled = false;
                                #endregion
                                m_lastSelectedInputField.interactable = false;
                                break;
                            }
                            case true:
                            {
                                #region Cursor doesn't activate, but Player has still to press Submit to move to next GO.
                                //m_lastSelectedInputField.enabled = true;
                                #endregion
                                m_lastSelectedInputField.interactable = true;
                                break;
                            }
                        }
                        break;
                    }
                    case false: //GO is the TMP_InputField we just left.
                    {
                        if (m_lastSelectedInputField != null)
                        {
#if UNITY_EDITOR
                            Debug.Log($"Left last {inputField.name} IF.");
#endif
                            m_fieldIsInEditMode = false;
                            inputField.image.color = inputField.colors.normalColor;
                            if (inputField.interactable)
                                inputField.interactable = false;
                            if (!inputField.enabled)
                                inputField.enabled = true;
                            m_lastSelectedInputField = null;
                        }
                        break;
                    }
                }
            }
        }

        private void HandleButtonPresses()
        {
            if (m_inputActions.UserInterface.Submit.WasPressedThisFrame())  //Limit check to once per frame!
            {
                switch (m_fieldIsInEditMode)
                {
                    case false: //InputField is currently not in Edit-Mode. Switch into Edit-Mode.
                    {
                        m_fieldIsInEditMode = true;
                        break;
                    }
                    case true:  //InputField is currently in Edit-Mode. Switch out of Edit-Mode.
                    {
                        m_fieldIsInEditMode = false;
                        break;
                    }
                }
            }

            if (m_inputActions.UserInterface.Cancel.WasPressedThisFrame() && !m_firstElement.gameObject.activeInHierarchy && !m_fieldIsInEditMode)
                CloseToPreviousElement();
        }

        private void SetUIElements()
        {
            for (int navMenu = 0; navMenu < m_navigationKey.Length; navMenu++)
                m_targetNavigationElement.Add(m_navigationKey[navMenu], m_navigationValue[navMenu]);

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
            if (!Application.isFocused)
                return;

            Transform currentElement = m_activeElement.Peek();
            currentElement.gameObject.SetActive(false);

            m_activeElement.Push(_next);
            _next.gameObject.SetActive(true);

            SetNavigationGameObject(_next);
        }

        public void CloseToPreviousElement()
        {
            if (!Application.isFocused)
                return;

            Transform currentElement = m_activeElement.Pop();
            currentElement.gameObject.SetActive(false);

            if (m_activeElement.Count == 0)
                SetFirstStackElement(m_firstElement);

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
            if (!Application.isFocused)
                return;

            var sceneIndex = SceneManager.GetActiveScene().buildIndex;
            if (sceneIndex == (int)ESceneNames.StartMenu)   //TODO: Add other 'menoOnly' Scenes.
                return;

            if (m_firstElement.gameObject.activeInHierarchy)
                AResumeTheGame?.Invoke();
        }

        private void OnNavigationInput(InputAction.CallbackContext _callbackContext)
        {
            if (!Application.isFocused)
                return;

            if (m_playerInput == null && !m_uiActionMap.enabled)    //Only pass in GameScenes if PauseMenu is opened and PlayerInputs exist.
                return;

            if (_callbackContext.control.device is Keyboard)        //Because Keyboard works (currently).
                return;

            NavigateToNextObject(_callbackContext.ReadValue<Vector2>());
        }

        private void OnSubmitInput(InputAction.CallbackContext _callbackContext)
        {
            if (!Application.isFocused)
                return;

            if (m_playerInput == null && !m_uiActionMap.enabled)    //Only pass in GameScenes if PauseMenu is opened and PlayerInputs exist.
                return;

            if (_callbackContext.control.device is Keyboard)        //Because Keyboard works (currently).
                return;

            if (_callbackContext.ReadValueAsButton())
                ExecuteEvents.Execute(m_lastSelectedGameObject, new BaseEventData(m_eventSystem), ExecuteEvents.submitHandler);
        }
        #endregion
    }
}