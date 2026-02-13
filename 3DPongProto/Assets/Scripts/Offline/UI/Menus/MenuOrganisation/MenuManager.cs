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
        [SerializeField] internal Transform m_firstTransformElement; //To prevent confusion: DictKey, not dictValue.
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

        [SerializeField] private TMP_Dropdown[] m_uIDropdowns;

        [Header("Pause Menu Navigation")]
        [SerializeField] private Button m_resumeButton;
        [SerializeField] private Button m_quitButton;
        [SerializeField] private Button m_hiddenFinishButton;
        #endregion

        internal static GameObject LastSelectedGameObject { get => m_lastSelectedGameObject; }
        private static GameObject m_lastSelectedGameObject;

        private TMP_InputField m_lastSelectedInputField = null;
        private TMP_InputField m_activeInputField = null;

        #region Actions_and_Functions
        internal static event Action AEndInfiniteMatch;         //LocalMatchManager ends an InfiniteMatch.
        #endregion

        private MatchSettingsData m_matchData;
        private string m_currentInputFieldContent = "";

        private void Awake()
        {
            m_matchData = SettingsManager.Instance.CurrentSettings.Match;

            var centralInputAction = UserInputManager.Instance.GetCentralActions();
            if (centralInputAction == null)
            {
                Debug.LogError("CentralActionsInstance in UserInputManager is null!", this);
                enabled = false;
                return;
            }

            m_inputActions = centralInputAction;
            m_uiActionMap = m_inputActions.UserInterface;

            //If MenuManager's firstElement is active (MainMenu), toggle UserInterface Map. Else (GameScene) PlayerActions.
            if (m_firstTransformElement.gameObject.activeInHierarchy)
                UserInputManager.Instance.ToggleActionMaps(EInputActionMaps.UserInterface.ToString());
            else
                UserInputManager.Instance.ToggleActionMaps(EInputActionMaps.PlayerActions.ToString());

            m_lastSelectedGameObject = null;
            m_targetNavigationElement.Clear();

            SetFirstStackElement(m_firstTransformElement);
            SetUIElements();    //Also sets the lastSelectedElement.
        }

        private void OnEnable()
        {
            m_inputActions.UserInterface.Submit.performed += OnSubmitInput;
            if (!m_navigationKey[0].gameObject.activeInHierarchy)
            {
                //Navigation and Submit work in Scenes without PlayerInput components. This if-test prevents doubled performed actions.
                m_inputActions.UserInterface.Navigate.performed += OnNavigationInput;
            }

            UserInputManager.AChangeActiveActionMap += OnChangeActiveActionMap;
        }

        private void OnDisable()    //Copy content into 'OnDestroy()', if needed.
        {
            //TODO: Make clear, if un-subscriptions also need a if-condition like subscription above.
            m_inputActions.UserInterface.Navigate.performed -= OnNavigationInput;
            m_inputActions.UserInterface.Submit.performed -= OnSubmitInput;

            UserInputManager.AChangeActiveActionMap -= OnChangeActiveActionMap;
        }

        private void Start()
        {
            m_playerInput = FindObjectOfType<PlayerInput>();

            if (m_hiddenFinishButton != null)
            {
                bool _infiniteMatch = m_matchData.EGameMode == EGameMode.Infinite;
                m_hiddenFinishButton.gameObject.SetActive(_infiniteMatch);
            }
        }

        private void Update()
        {
            if (!Application.isFocused)
                return;

            UpdateLastSelectedObject();
            HandleUIInteractions();
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

            SetNavigationGameObject(m_firstTransformElement);
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
                SetFirstStackElement(m_firstTransformElement);

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
                {
                    Debug.LogWarning("FocusTarget is null!");
                    return;
                }
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
            UserInputManager.Instance.ToggleActionMaps(EInputActionMaps.PlayerActions.ToString());
        }
        #endregion

        #region Custom-Methods
        private void OnOpenMenu()
        {
            if (!m_firstTransformElement.gameObject.activeInHierarchy)
            {
                m_firstTransformElement.gameObject.SetActive(true);
                SetNavigationGameObject(m_firstTransformElement);
            }

            UpdatePauseMenuNavigation();
        }

        private void OnCloseMenu()
        {
            if (m_firstTransformElement.gameObject.activeInHierarchy)
            {
                m_firstTransformElement.gameObject.SetActive(false);
                SetNavigationGameObject(m_firstTransformElement);
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
                            //CallbackContext-OnNavigationInput sets new selected GameObject!

                            //2nd has to be in the same frame, or it blinks!
                            if(m_lastSelectedGameObject.TryGetComponent(out Button button))
                                CategoryButtonTransition(button);
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
        }

        private void InputFieldCheck(GameObject _incomingGameObject)
        {
            //If '_incomingGameObject' is a TMP_InputField.
            if (_incomingGameObject.TryGetComponent<TMP_InputField>(out var inputField))
            {
                //true == newest G0 from InputFieldCheck() || false == previous saved GO from UpdateLastSelectedObject().
                switch (inputField.gameObject == m_eventSystem.currentSelectedGameObject)
                {
                    case true:  //GO is the newest, current selected Object AND a TMP_InputField. InputField's eventData activates itself in EditActivation.cs.
                    {
                        if (m_lastSelectedInputField == null)
                        {
                            m_lastSelectedInputField = inputField;  //Save the latest selected TMP_InputField for later changes.
                            m_lastSelectedInputField.image.color = m_lastSelectedInputField.colors.selectedColor;
                        }
                        break;
                    }
                    case false: //GO is the TMP_InputField we just left. Deactivate the old InputField, before setting current IF.
                    {
                        if (m_lastSelectedInputField != null)
                        {
                            if(m_lastSelectedInputField.interactable)
                                {
                                    m_lastSelectedInputField.interactable = false;
                                    m_lastSelectedInputField.DeactivateInputField();
                                }
                                
                            inputField.image.color = inputField.colors.normalColor;
                            m_lastSelectedInputField = null;
                        }
                        break;
                    }
                }
            }
        }

        private void HandleUIInteractions()
        {
            if (!Application.isFocused)
                return;

            var uiActions = m_inputActions.UserInterface;

            //CANCEL-Logic
            if (uiActions.Cancel.WasPressedThisFrame())
            {
                if (HandleHighPriorityCancelActions())
                {
                    return;
                }

                //If nothing has a higher priority, navigate back.
                if (m_activeElement.Count > 1)
                {
                    CloseToPreviousElement();
                }
                else if (SceneManager.GetActiveScene().buildIndex != (int)ESceneNames.StartMenu)
                {
                    OnResumeTheGame();  //Resume back to the game in GameScene.
                }
                Debug.Log($"{uiActions.Cancel.activeControl.device.name} pressed OnCancel.");
            }

            //SUBMIT-Logic
            if (uiActions.Submit.WasPressedThisFrame())
            {
                //Blocks the Submit-Logic, if actions like KeyRebinding or Dropdowns have priority.
                if (RebindManager.Instance != null && RebindManager.Instance.IsRebinding/* || IsAnyDropdownOpen()*/)
                {
                    return;
                }
                
                //Else process the InputField-Logic.
                HandleInputFieldSubmit();
            }
        }

        /// <summary>
        /// Submit-Action Logic of InputFields.
        /// </summary>
        private void HandleInputFieldSubmit()
        {
            var currentSelected = m_eventSystem.currentSelectedGameObject;
            
            if (m_activeInputField != null)
            {
                Debug.Log($"{currentSelected} in if.");
                //End Edit-Mode while being in an InputField
                m_activeInputField.interactable = false;
                m_activeInputField.DeactivateInputField();
                m_activeInputField = null;
            }
            else if (currentSelected != null && currentSelected.TryGetComponent<TMP_InputField>(out var selectedField))
            {
                Debug.Log($"{currentSelected} in else if.");
                //An InputField is selected and Edit-Mode shall get started
                m_activeInputField = selectedField;
                m_currentInputFieldContent = selectedField.text; // Text f�r "Cancel" merken
                m_activeInputField.interactable = true;
                m_activeInputField.ActivateInputField();
            }
        }

        /// <summary>
        /// Checks for a prioritized UI-Interactions.
        /// </summary>
        /// <returns>True, if an action is currently handled, else false.</returns>
        private bool HandleHighPriorityCancelActions()
        {
            if (RebindManager.Instance != null && (RebindManager.Instance.IsRebinding || RebindManager.Instance.WasJustCancelled))
                return true;        //Block Cancel-Action. Rebind-Cancel-Action has priority before Menu-Back-Navigation.

            //Check for opened Dropdowns. Dropdown-References required!
            foreach (var dropdown in m_uIDropdowns) //Basicly the whole private bool IsAnyDropdownOpen() method. Plus return false below.
            {
                if (dropdown.IsExpanded)
                {
                    dropdown.Hide(); //Closes the dropdown.
                    return true;     //Block the Cancel-Action, so dropdown gets closed first.
                }
            }

            //Check for an active InputField in Edit-Mode.
            if (m_activeInputField != null)
            {
                //Deactivate the inputField to enable navigation.
                m_activeInputField.text = m_currentInputFieldContent; //Optional: Reset saved Text.
                m_activeInputField.interactable = false;
                m_activeInputField.DeactivateInputField();
                m_activeInputField = null;
                return true;    //Block the Cancel-Action, to move out of the InputField first.
            }

            return false;       //No active action prioritized.
        }

        private void CategoryButtonTransition(Button _incomingButton)
        {
            if(m_categoryButtons.Contains(_incomingButton))         //Only act, if button is in array.
                CategorySwitch(_incomingButton);
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
            StartCoroutine(SetNewObject(_gameObject));
        }

        private IEnumerator SetNewObject(GameObject _gameObject)
        {
            m_eventSystem.SetSelectedGameObject(_gameObject);
            yield return null;
            _gameObject.TryGetComponent(out TMP_InputField inputField);
            
            if (inputField)
                m_activeInputField = inputField;
        }

        /// <summary>
        /// Adapt the Pause-Menu-Navigation to the current Gamemode.
        /// </summary>
        private void UpdatePauseMenuNavigation()
        {
            //Get the current EGameMode from the SettingsManager.
            bool isInfiniteMode = m_matchData.EGameMode == EGameMode.Infinite;

            //Get the navigation from the relevant Buttons.
            Navigation resumeNav = m_resumeButton.navigation;
            Navigation quitNav = m_quitButton.navigation;
            Navigation endInfiniteNav = m_hiddenFinishButton.navigation;

            //Adapt the Navigation to the current EGamemode.
            if (isInfiniteMode)
            {
                //Infinite-Mode: UI-Navigation loop between Resume and EndInfiniteMatch Buttons.
                resumeNav.selectOnUp = m_hiddenFinishButton;
                //Change Looping to ResumeButton to navigate down to the EndInfiniteMatch button.
                quitNav.selectOnDown = m_hiddenFinishButton;
                //Looping between Resume- and EndInfiniteMatch Buttons on Infinite-Mode.
                endInfiniteNav.selectOnDown = m_resumeButton;
            }
            else
            {
                //Normal Mode: Loop between Resume und Quit Buttons.
                //ResumeButton loops to QuitButton.
                resumeNav.selectOnUp = m_quitButton;
                //And vise versa on QuitButton.
                quitNav.selectOnDown = m_resumeButton;
            }

            //Apply the changed Navigation(s).
            m_resumeButton.navigation = resumeNav;
            m_quitButton.navigation = quitNav;
            m_hiddenFinishButton.navigation = endInfiniteNav;
        }
        #endregion

        #region MenuButton_Methods
        public void ResumeGame()
        {
            OnResumeTheGame();
        }

        public void RestartGameScene()
        {
            var sceneIndex = SceneManager.GetActiveScene().buildIndex;
            UserInputManager.Instance.ToggleActionMaps(EInputActionMaps.PlayerActions.ToString());
            ReLoadScene(sceneIndex);
        }

        public void ReturnToMainMenu()
        {
            ReLoadScene((int)ESceneNames.StartMenu);
        }

        public void EndInfiniteMatch()
        {
            AEndInfiniteMatch?.Invoke();    //Sends request to end infinite Matches. Skips HighScoreBoard while noone gained a point.
        }

        /// <summary>
        /// Method to receive the buildIndex of the scene that shall be reloaded.
        /// </summary>
        /// <param name="_sceneIndex"></param>
        private void ReLoadScene(int _sceneIndex)
        {
            if (_sceneIndex > 0) //Exclude BootScene with DDOL-Managers.
            {
                if (_sceneIndex < SceneManager.sceneCountInBuildSettings - 1)   //-1 marks last SceneIndex.
                    SceneManager.LoadScene(_sceneIndex);
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

            // if (CursorManager.Instance != null && CursorManager.Instance.IsCursorActive)
            // {
            //     Debug.Log("Get current highlighted Object and set it as m_lastSelectedGameObject.");
            //     return; 
            // }
            
            if (_callbackContext.ReadValueAsButton())
                ExecuteEvents.Execute(m_lastSelectedGameObject, new BaseEventData(m_eventSystem), ExecuteEvents.submitHandler);
        }
        #endregion
    }
}