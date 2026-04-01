using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using ThreeDeePongProto.Shared.Managers;
using ThreeDeePongProto.Shared.InputActions;
using UnityEngine.EventSystems;
using TMPro;

namespace ThreeDeePongProto.Offline.UI.Menu   
{
    [RequireComponent(typeof(MenuManager))]
    public class MenuInput : MonoBehaviour
    {
        private PlayerInputActions m_inputActions;
        private InputActionMap m_uiActionMap;
        private PlayerInput m_playerInput;

        [SerializeField] internal MenuManager m_menuManager;
        private MenuNavigation m_menuNavigation;

        [SerializeField] private Button m_resumeButton;
        [SerializeField] private Button m_quitButton;
        [SerializeField] private Button m_hiddenFinishButton;
        
        private TMP_InputField m_activeInputField = null;
        private string m_currentInputFieldContent = "";

        private MatchSettingsData m_matchData;

        private void Awake()
        {
            if(m_menuManager == null)
                m_menuManager = GetComponent<MenuManager>();

            m_menuNavigation = m_menuManager.m_menuNavigation;

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
        }

        private void OnEnable()
        {
            m_inputActions.UserInterface.Submit.performed += OnSubmitInput;
            if (!m_menuNavigation.m_navigationKey[0].gameObject.activeInHierarchy)
            {
                //Navigation and Submit work in Scenes without PlayerInput components. This if-test prevents doubled performed actions.
                m_inputActions.UserInterface.Navigate.performed += OnNavigationInput;
            }
        }

        private void OnDisable()    //Copy content into 'OnDestroy()', if needed.
        {
            m_inputActions.UserInterface.Navigate.performed -= OnNavigationInput;
            m_inputActions.UserInterface.Submit.performed -= OnSubmitInput;
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
                
            HandleUIInteractions();
        }

        #region Custom-Methods
        internal void OnOpenMenu()
        {
            if (!m_menuNavigation.m_firstTransformElement.gameObject.activeInHierarchy)
            {
                m_menuNavigation.m_firstTransformElement.gameObject.SetActive(true);
                m_menuNavigation.SetNavigationGameObject(m_menuNavigation.m_firstTransformElement);
            }

            UpdatePauseMenuNavigation();
        }

        internal void OnCloseMenu()
        {
            if (m_menuNavigation.m_firstTransformElement.gameObject.activeInHierarchy)
            {
                m_menuNavigation.m_firstTransformElement.gameObject.SetActive(false);
                m_menuNavigation.SetNavigationGameObject(m_menuNavigation.m_firstTransformElement);
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
                if (m_menuNavigation.m_activeElement.Count > 1)
                {
                    m_menuNavigation.CloseToPreviousElement();
                }
                else if (m_menuManager.CurrentSceneIndex() != (int)ESceneNames.StartMenu)
                {
                    //Resume back to the game in GameScene.
                    m_menuManager.ResumeTheGame();
                }
                Debug.Log($"Skipping OnCancel. Arrived on highest Menu-Level.");
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
        /// Submit-Action Logic of InputFields.m_navigation
        /// </summary>
        private void HandleInputFieldSubmit()
        {
            var currentSelected = m_menuManager.m_eventSystem.currentSelectedGameObject;
            
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
            foreach (var dropdown in m_menuNavigation.m_uIDropdowns) //Basicly the whole private bool IsAnyDropdownOpen() method. Plus return false below.
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

        public void ClickActivation(GameObject _gameObject)
        {
            StartCoroutine(SetNewObject(_gameObject));
        }

        private IEnumerator SetNewObject(GameObject _gameObject)
        {
            m_menuManager.m_eventSystem.SetSelectedGameObject(_gameObject);
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

        #region CallbackContext-Subscription_Methods
        private void OnNavigationInput(InputAction.CallbackContext _callbackContext)
        {
            if (!Application.isFocused)
                return;
            
            if (m_playerInput == null && !m_uiActionMap.enabled)    //Only pass in GameScenes if PauseMenu is opened and PlayerInputs exist.
                return;
            
            if (_callbackContext.control.device is Keyboard)        //Because Keyboard works (currently).
                return;

            m_menuNavigation.NavigateToNextObject(_callbackContext.ReadValue<Vector2>());
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
                ExecuteEvents.Execute(MenuNavigation.m_lastSelectedGameObject, new BaseEventData(m_menuManager.m_eventSystem), ExecuteEvents.submitHandler);
        }
        #endregion
    }
}