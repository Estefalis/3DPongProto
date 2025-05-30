using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    internal enum EButtonTransition
    {
        None,
        Alpha,
        Color
    }

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
        [Header("Button Transition")]
        [SerializeField] private EButtonTransition m_eButtonTransition = EButtonTransition.Color;
        [SerializeField, Range(0.1f, 0.9f)] private float m_reducedAlphaValue = 0.5f;
        [SerializeField, Range(0.5f, 1f)] private float m_maxAlphaValue = 1f;
        [Space]
        [SerializeField] private Button[] m_categoryButtons;
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
        private string m_currentInputFieldContent = "";

        #region Actions_and_Functions
        internal static event Action AResumeTheGame;          //LocalMatchManager to resume the Game.
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

            if (UserInputManager.m_CentralActionsInstance == null)
            {
                Debug.LogError("CentralActionsInstance in UserInputManager is null!", this);
                enabled = false;
                return;
            }

            m_inputActions = UserInputManager.m_CentralActionsInstance;
            m_uiActionMap = m_inputActions.UserInterface;

            //If MenuManager's firstElement is active (MainMenu), toggle UserInterface Map. Else (GameScene) PlayerActions.
            if (m_firstElement.gameObject.activeInHierarchy)
                UserInputManager.ToggleActionMaps(EInputActionMaps.UserInterface.ToString());
            else
                UserInputManager.ToggleActionMaps(EInputActionMaps.PlayerActions.ToString());

            m_lastSelectedGameObject = null;
            m_targetNavigationElement.Clear();

            SetFirstStackElement(m_firstElement);
            SetUIElements();    //Also sets the lastSelectedElement.
        }

        private void OnEnable()
        {
            if (!m_navigationKey[0].gameObject.activeInHierarchy)
            {
                //Navigation and Submit work in Scenes without PlayerInput components. This if-test prevents doubled performed actions.
                m_inputActions.UserInterface.Navigate.performed += OnNavigationInput;
                m_inputActions.UserInterface.Submit.performed += OnSubmitInput;
            }

            UserInputManager.AChangeActiveActionMap += OnChangeActiveActionMap;
            AResumeTheGame += OnResumeTheGame;
        }

        private void OnDisable()    //Copy content into 'OnDestroy()', if needed.
        {
            //TODO: Make clear, if un-subscriptions also need a if-condition like subscription above.
            m_inputActions.UserInterface.Navigate.performed -= OnNavigationInput;
            m_inputActions.UserInterface.Submit.performed -= OnSubmitInput;

            UserInputManager.AChangeActiveActionMap -= OnChangeActiveActionMap;
            AResumeTheGame -= OnResumeTheGame;
        }

        private void Start()
        {
            m_playerInput = FindObjectOfType<PlayerInput>();

            if (m_hiddenFinishButton != null)
                InVisibleButton(m_matchUIStates.InfiniteMatch);     //m_matchUIStates get load in LoadSettingsValues > Awake().
        }

        private void Update()
        {
            if (!Application.isFocused)  //TODO: Test, if Navigation still works, if game is not focused.
                return;

            UpdateLastSelectedObject();
            HandleButtonPresses();
        }

        #region Prepare Navigation-Stack and (de-)activate Menu-Transforms to navigate.
        /// <summary>
        /// 'm_activeElement' Stack requires a set element to start with, to prevent a null error.
        /// </summary>
        /// <param name="_firstElement"></param>
        private void SetFirstStackElement(Transform _firstElement)
        {
            m_activeElement.Push(_firstElement);
        }

        private void SetUIElements()
        {
            for (int navMenu = 0; navMenu < m_navigationKey.Length; navMenu++)
                m_targetNavigationElement.Add(m_navigationKey[navMenu], m_navigationValue[navMenu]);

            SetNavigationGameObject(m_firstElement);
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

        #region Subscriptions not related to the InputSystem
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

        private void OnResumeTheGame()
        {
            //Toggle ActionMap-switch to PlayerActions.
            UserInputManager.ToggleActionMaps(EInputActionMaps.PlayerActions.ToString());
        }
        #endregion

        #region Custom-Methods
        private void OnOpenMenu()
        {
            if (!m_firstElement.gameObject.activeInHierarchy)
            {
                m_firstElement.gameObject.SetActive(true);
                SetNavigationGameObject(m_firstElement);
            }
        }

        private void OnCloseMenu()
        {
            if (m_firstElement.gameObject.activeInHierarchy)
            {
                m_firstElement.gameObject.SetActive(false);
                SetNavigationGameObject(m_firstElement);
            }
        }

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
                            //1st check for TMP Input Field before replacing the latest selected FallBack-GameObject.
                            if (m_lastSelectedGameObject != null)
                                InputFieldCheck(m_lastSelectedGameObject);
                            //Moment, when the previous saved GO is made equal to the selected Object from the eventSystem.
                            m_lastSelectedGameObject = m_eventSystem.currentSelectedGameObject;
                            //2nd has to be in the same frame, or it blinks!
                            ButtonTransition(m_lastSelectedGameObject);
                            break;
                        }
                        case false:
                        {
                            //2nd check for TMP Input Field after replacing the latest selected FallBack-GameObject.
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
                            //Debug.Log($"switch current IF: {m_lastSelectedInputField.name}.");
#endif
                            m_lastSelectedInputField.image.color = m_lastSelectedInputField.colors.selectedColor;
                        }

                        switch (m_fieldIsInEditMode)
                        {
                            case false:
                            {
                                ExitEditMode();
                                break;
                            }
                            case true:
                            {
                                EnterEditMode();
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
                            //Debug.Log($"Left last {inputField.name} IF. Saved old IF Content: {m_currentInputFieldContent}");
#endif
                            m_fieldIsInEditMode = false;
                            inputField.image.color = inputField.colors.normalColor;
                            if (inputField.interactable)
                                inputField.interactable = false;
                            if (!inputField.enabled)
                                inputField.enabled = true;
                            if (inputField.isFocused)
                                inputField.DeactivateInputField();
                            if (m_currentInputFieldContent != string.Empty)
                                m_currentInputFieldContent = string.Empty;  //m_currentInputFieldContent reset each time we leave an IF.
                            m_lastSelectedInputField = null;
                        }
                        break;
                    }
                }
            }
        }

        private void ExitEditMode()
        {
            if (m_lastSelectedInputField == null)
                return;

            if (m_lastSelectedInputField.interactable)
            {
                m_lastSelectedInputField.interactable = false;
                m_lastSelectedInputField.DeactivateInputField();
            }
        }

        private void EnterEditMode()
        {
            if (m_lastSelectedInputField == null)
                return;

            if (!m_lastSelectedInputField.interactable)
            {
                m_currentInputFieldContent = m_lastSelectedInputField.text;   //Save the text for a possible Cancel-Action.
                m_lastSelectedInputField.interactable = true;
                m_lastSelectedInputField.ActivateInputField();
            }
        }

        private void HandleButtonPresses()
        {
            if (!Application.isFocused)
                return;

            #region Submit-Actions
            if (m_inputActions.UserInterface.Submit.WasPressedThisFrame())  //Limit check to once per frame!
            {
                if (m_lastSelectedInputField != null)   //Only set 'm_fieldIsInEditMode' to true, if being in an InputField.
                {
                    switch (m_fieldIsInEditMode)
                    {
                        case false: //InputField is currently not in Edit-Mode, switch into Edit-Mode.
                        {
                            m_fieldIsInEditMode = true;
                            break;
                        }
                        case true:  //InputField is currently in Edit-Mode, switch out of Edit-Mode.
                        {
                            m_fieldIsInEditMode = false;
                            break;
                        }
                    }
                }
            }
            #endregion

            #region Cancel-Actions
            if (m_inputActions.UserInterface.Cancel.WasPressedThisFrame())  //Limit check to once per frame!
            {
                switch (m_fieldIsInEditMode)
                {
                    case false: //If InputField is currently not in Edit-Mode, close to previous Transform-Parent in Stack.
                    {
                        if (!m_firstElement.gameObject.activeInHierarchy)   //Prevents closing the very first menu Transform-Parent.
                            CloseToPreviousElement();
                        else if (m_firstElement.gameObject.activeInHierarchy)
                        {
                            var sceneIndex = SceneManager.GetActiveScene().buildIndex;
                            if (sceneIndex != (int)ESceneNames.StartMenu)
                            {
                                AResumeTheGame?.Invoke();   //Cancel InputAction triggers gameResume procedure.
                            }
                        }
                        break;
                    }
                    case true:  //If InputField is currently in Edit-Mode, switch out of Edit-Mode and reset to previous IF content.
                    {
                        if (m_lastSelectedInputField != null)               //Prevents NullReferenceException when no IF is active/set.
                        {
                            if (m_currentInputFieldContent != string.Empty)
                                m_lastSelectedInputField.text = m_currentInputFieldContent;    //Saved content of the last InputField.
                            //NOTE: m_currentInputFieldContent will be reset on leaving InputFields.

                            m_fieldIsInEditMode = false;
                        }

                        ExitEditMode();
                        break;
                    }
                }
            }
            #endregion
        }

        private void ButtonTransition(GameObject _incomingGameObject)
        {
            if (!_incomingGameObject.TryGetComponent<Button>(out var button))   //Exclude none buttons.
                return;

            if (m_categoryButtons.Contains(button))                             //Only act, if button is in array.
            {
                CategorySwitch(button);
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

        public void ClickActivation(GameObject _gameObject)
        {
            StartCoroutine(SwitchInputFields(_gameObject));
        }

        private IEnumerator SwitchInputFields(GameObject _gameObject)
        {
            m_fieldIsInEditMode = false;
            m_eventSystem.SetSelectedGameObject(_gameObject);
            yield return null;
            m_fieldIsInEditMode = true;
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

        public void EndInfiniteMatch()
        {
            if (m_matchValues.TotalPointsTPOne > 0 || m_matchValues.TotalPointsTPTwo > 0)
            {
                AEndInfiniteMatch?.Invoke();
                m_navigationKey[0].gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Each Button pressed sets the visibly activated/deactivated Button and enables/disables the corresponding Settings-SubPage.
        /// </summary>
        /// <param name="_sender"></param>
        public void CategorySwitch(Button _sender)
        {
            for (int cb = 0; cb < m_categoryButtons.Length; cb++)
            {
                if (_sender == m_categoryButtons[cb])
                {
                    m_subPageTransforms[cb].gameObject.SetActive(true);
                    switch (m_eButtonTransition)
                    {
                        case EButtonTransition.Alpha:
                        {
                            Color maxAlpha = m_categoryButtons[cb].image.color;
                            maxAlpha.a = m_maxAlphaValue;
                            m_categoryButtons[cb].image.color = maxAlpha;
                            break;
                        }
                        case EButtonTransition.Color:
                        {
                            m_categoryButtons[cb].image.color = m_categoryButtons[cb].colors.selectedColor;
                            break;
                        }
                        default:
                            break;
                    }
                }
                else
                {
                    m_subPageTransforms[cb].gameObject.SetActive(false);
                    switch (m_eButtonTransition)
                    {
                        case EButtonTransition.Alpha:
                        {
                            Color reducedAlpha = m_categoryButtons[cb].image.color;
                            reducedAlpha.a = m_reducedAlphaValue;
                            m_categoryButtons[cb].image.color = reducedAlpha;
                            break;
                        }
                        case EButtonTransition.Color:
                        {
                            m_categoryButtons[cb].image.color = m_categoryButtons[cb].colors.normalColor;
                            break;
                        }
                        default:
                            break;
                    }
                }
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

        #region CallbackContext-Subscription_Methods
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