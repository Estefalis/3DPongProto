// using System;
// using System.Collections;
// using UnityEngine;
// using UnityEngine.UI;
// using UnityEngine.InputSystem;
// using ThreeDeePongProto.Shared.Managers;
// using ThreeDeePongProto.Shared.InputActions;
// using UnityEngine.EventSystems;
// using UnityEngine.SceneManagement;
// using TMPro;

// namespace ThreeDeePongProto.Offline.UI.Menu   
// {
//     public class MenuInput : MonoBehaviour
//     {
//         private PlayerInputActions m_inputActions;
//         private InputActionMap m_uiActionMap;
//         private PlayerInput m_playerInput;

//         [SerializeField] internal MenuManager m_menuManager;
//         private MenuNavigation m_navigation;

//         [SerializeField] private Button m_resumeButton;
//         [SerializeField] private Button m_quitButton;
//         [SerializeField] private Button m_hiddenFinishButton;
        
//         private TMP_InputField m_activeInputField = null;
//         private string m_currentInputFieldContent = "";

//         private MatchSettingsData m_matchData;

//         internal static event Action AEndInfiniteMatch;         //LocalMatchManager ends an InfiniteMatch.

// //         private void Awake()
// //         {
// //             if(m_menuManager == null)
// //                 m_menuManager = GetComponent<MenuManager>();

// //             m_navigation = m_menuManager.m_menuNavigation;

// //             m_matchData = SettingsManager.Instance.CurrentSettings.Match;
// //             var centralInputAction = UserInputManager.Instance.GetCentralActions();
// //             if (centralInputAction == null)
// //             {
// //                 Debug.LogError("CentralActionsInstance in UserInputManager is null!", this);
// //                 enabled = false;
// //                 return;
// //             }

// //             m_inputActions = centralInputAction;
// //             m_uiActionMap = m_inputActions.UserInterface;

// //             //If MenuManager's firstElement is active (MainMenu), toggle UserInterface Map. Else (GameScene) PlayerActions.
// //             if (m_navigation.m_firstTransformElement.gameObject.activeInHierarchy)
// //                 UserInputManager.Instance.ToggleActionMaps(EInputActionMaps.UserInterface.ToString());
// //             else
// //                 UserInputManager.Instance.ToggleActionMaps(EInputActionMaps.PlayerActions.ToString());
// //         }

// //         private void OnEnable()
// //         {
// //             m_inputActions.UserInterface.Submit.performed += OnSubmitInput;
// //             if (!m_navigation.m_navigationKey[0].gameObject.activeInHierarchy)
// //             {
// //                 //Navigation and Submit work in Scenes without PlayerInput components. This if-test prevents doubled performed actions.
// //                 m_inputActions.UserInterface.Navigate.performed += OnNavigationInput;
// //             }

// //             UserInputManager.AChangeActiveActionMap += OnChangeActiveActionMap;
// //         }

// //         private void OnDisable()    //Copy content into 'OnDestroy()', if needed.
// //         {
// //             //TODO: Make clear, if un-subscriptions also need a if-condition like subscription above.
// //             m_inputActions.UserInterface.Navigate.performed -= OnNavigationInput;
// //             m_inputActions.UserInterface.Submit.performed -= OnSubmitInput;

// //             UserInputManager.AChangeActiveActionMap -= OnChangeActiveActionMap;
// //         }

// //         private void Start()
// //         {
// //             m_playerInput = FindObjectOfType<PlayerInput>();

// //             if (m_hiddenFinishButton != null)
// //             {
// //                 bool _infiniteMatch = m_matchData.EGameMode == EGameMode.Infinite;
// //                 m_hiddenFinishButton.gameObject.SetActive(_infiniteMatch);
// //             }
// //         }

// //         private void Update()
// //         {
// //             if (!Application.isFocused)
// //                 return;
                
// //             HandleUIInteractions();
// //         }

// //         #region Subscriptions not related to the InputSystem
// //         /// <summary>
// //         /// Method to react on ActionMap changes triggered via static method 'ToggleActionMaps'.
// //         /// </summary>
// //         /// <param name="_actionMap"></param>
// //         private void OnChangeActiveActionMap(string _actionMap)
// //         {
// //             if (!Application.isFocused)
// //                 return;

// //             var sceneIndex = SceneManager.GetActiveScene().buildIndex;
// //             if (sceneIndex == (int)ESceneNames.StartMenu)   //Or in other menuOnly Scenes.
// //                 return;

// //             if (_actionMap == EInputActionMaps.UserInterface.ToString())
// //             {
// //                 OnOpenMenu();
// //             }
// //             else if (_actionMap == EInputActionMaps.PlayerActions.ToString())
// //             {
// //                 OnCloseMenu();
// //             }
// //         }

// //         private void OnResumeTheGame()
// //         {
// //             //Toggle ActionMap-switch to PlayerActions.
// //             UserInputManager.Instance.ToggleActionMaps(EInputActionMaps.PlayerActions.ToString());
// //         }
// //         #endregion

// //         #region Custom-Methods
// //         private void OnOpenMenu()
// //         {
// //             if (!m_navigation.m_firstTransformElement.gameObject.activeInHierarchy)
// //             {
// //                 m_navigation.m_firstTransformElement.gameObject.SetActive(true);
// //                 m_navigation.SetNavigationGameObject(m_navigation.m_firstTransformElement);
// //             }

// //             UpdatePauseMenuNavigation();
// //         }

// //         private void OnCloseMenu()
// //         {
// //             if (m_navigation.m_firstTransformElement.gameObject.activeInHierarchy)
// //             {
// //                 m_navigation.m_firstTransformElement.gameObject.SetActive(false);
// //                 m_navigation.SetNavigationGameObject(m_navigation.m_firstTransformElement);
// //             }
// //         }
        
// //         private void HandleUIInteractions()
// //         {
// //             if (!Application.isFocused)
// //                 return;

// //             var uiActions = m_inputActions.UserInterface;

// //             //CANCEL-Logic
// //             if (uiActions.Cancel.WasPressedThisFrame())
// //             {
// //                 if (HandleHighPriorityCancelActions())
// //                 {
// //                     return;
// //                 }

// //                 //If nothing has a higher priority, navigate back.
// //                 if (m_navigation.m_activeElement.Count > 1)
// //                 {
// //                     m_navigation.CloseToPreviousElement();
// //                 }
// //                 else if (SceneManager.GetActiveScene().buildIndex != (int)ESceneNames.StartMenu)
// //                 {
// //                     OnResumeTheGame();  //Resume back to the game in GameScene.
// //                 }
// //                 Debug.Log($"{uiActions.Cancel.activeControl.device.name} pressed OnCancel.");
// //             }

// //             //SUBMIT-Logic
// //             if (uiActions.Submit.WasPressedThisFrame())
// //             {
// //                 //Blocks the Submit-Logic, if actions like KeyRebinding or Dropdowns have priority.
// //                 if (RebindManager.Instance != null && RebindManager.Instance.IsRebinding/* || IsAnyDropdownOpen()*/)
// //                 {
// //                     return;
// //                 }
                
// //                 //Else process the InputField-Logic.
// //                 HandleInputFieldSubmit();
// //             }
// //         }

// //         /// <summary>
// //         /// Submit-Action Logic of InputFields.m_navigation
// //         /// </summary>
// //         private void HandleInputFieldSubmit()
// //         {
// //             var currentSelected = m_menuManager.m_eventSystem.currentSelectedGameObject;
            
// //             if (m_activeInputField != null)
// //             {
// //                 Debug.Log($"{currentSelected} in if.");
// //                 //End Edit-Mode while being in an InputField
// //                 m_activeInputField.interactable = false;
// //                 m_activeInputField.DeactivateInputField();
// //                 m_activeInputField = null;
// //             }
// //             else if (currentSelected != null && currentSelected.TryGetComponent<TMP_InputField>(out var selectedField))
// //             {
// //                 Debug.Log($"{currentSelected} in else if.");
// //                 //An InputField is selected and Edit-Mode shall get started
// //                 m_activeInputField = selectedField;
// //                 m_currentInputFieldContent = selectedField.text; // Text f�r "Cancel" merken
// //                 m_activeInputField.interactable = true;
// //                 m_activeInputField.ActivateInputField();
// //             }
// //         }

// //         /// <summary>
// //         /// Checks for a prioritized UI-Interactions.
// //         /// </summary>
// //         /// <returns>True, if an action is currently handled, else false.</returns>
// //         private bool HandleHighPriorityCancelActions()
// //         {
// //             if (RebindManager.Instance != null && (RebindManager.Instance.IsRebinding || RebindManager.Instance.WasJustCancelled))
// //                 return true;        //Block Cancel-Action. Rebind-Cancel-Action has priority before Menu-Back-Navigation.

// //             //Check for opened Dropdowns. Dropdown-References required!
// //             foreach (var dropdown in m_navigation.m_uIDropdowns) //Basicly the whole private bool IsAnyDropdownOpen() method. Plus return false below.
// //             {
// //                 if (dropdown.IsExpanded)
// //                 {
// //                     dropdown.Hide(); //Closes the dropdown.
// //                     return true;     //Block the Cancel-Action, so dropdown gets closed first.
// //                 }
// //             }

// //             //Check for an active InputField in Edit-Mode.
// //             if (m_activeInputField != null)
// //             {
// //                 //Deactivate the inputField to enable navigation.
// //                 m_activeInputField.text = m_currentInputFieldContent; //Optional: Reset saved Text.
// //                 m_activeInputField.interactable = false;
// //                 m_activeInputField.DeactivateInputField();
// //                 m_activeInputField = null;
// //                 return true;    //Block the Cancel-Action, to move out of the InputField first.
// //             }

// //             return false;       //No active action prioritized.
// //         }

// //         public void ClickActivation(GameObject _gameObject)
// //         {
// //             StartCoroutine(SetNewObject(_gameObject));
// //         }

// //         private IEnumerator SetNewObject(GameObject _gameObject)
// //         {
// //             m_menuManager.m_eventSystem.SetSelectedGameObject(_gameObject);
// //             yield return null;
// //             _gameObject.TryGetComponent(out TMP_InputField inputField);
            
// //             if (inputField)
// //                 m_activeInputField = inputField;
// //         }

// //         /// <summary>
// //         /// Adapt the Pause-Menu-Navigation to the current Gamemode.
// //         /// </summary>
// //         private void UpdatePauseMenuNavigation()
// //         {
// //             //Get the current EGameMode from the SettingsManager.
// //             bool isInfiniteMode = m_matchData.EGameMode == EGameMode.Infinite;

// //             //Get the navigation from the relevant Buttons.
// //             Navigation resumeNav = m_resumeButton.navigation;
// //             Navigation quitNav = m_quitButton.navigation;
// //             Navigation endInfiniteNav = m_hiddenFinishButton.navigation;

// //             //Adapt the Navigation to the current EGamemode.
// //             if (isInfiniteMode)
// //             {
// //                 //Infinite-Mode: UI-Navigation loop between Resume and EndInfiniteMatch Buttons.
// //                 resumeNav.selectOnUp = m_hiddenFinishButton;
// //                 //Change Looping to ResumeButton to navigate down to the EndInfiniteMatch button.
// //                 quitNav.selectOnDown = m_hiddenFinishButton;
// //                 //Looping between Resume- and EndInfiniteMatch Buttons on Infinite-Mode.
// //                 endInfiniteNav.selectOnDown = m_resumeButton;
// //             }
// //             else
// //             {
// //                 //Normal Mode: Loop between Resume und Quit Buttons.
// //                 //ResumeButton loops to QuitButton.
// //                 resumeNav.selectOnUp = m_quitButton;
// //                 //And vise versa on QuitButton.
// //                 quitNav.selectOnDown = m_resumeButton;
// //             }

// //             //Apply the changed Navigation(s).
// //             m_resumeButton.navigation = resumeNav;
// //             m_quitButton.navigation = quitNav;
// //             m_hiddenFinishButton.navigation = endInfiniteNav;
// //         }

// //         public void ResumeGame()
// //         {
// //             OnResumeTheGame();
// //         }

// //         public void RestartGameScene()
// //         {
// //             var sceneIndex = SceneManager.GetActiveScene().buildIndex;
// //             UserInputManager.Instance.ToggleActionMaps(EInputActionMaps.PlayerActions.ToString());
// //             ReLoadScene(sceneIndex);
// //         }

// //         public void ReturnToMainMenu()
// //         {
// //             ReLoadScene((int)ESceneNames.StartMenu);
// //         }

// //         public void EndInfiniteMatch()
// //         {
// //             AEndInfiniteMatch?.Invoke();    //Sends request to end infinite Matches. Skips HighScoreBoard while noone gained a point.
// //         }

// //         /// <summary>
// //         /// Method to receive the buildIndex of the scene that shall be reloaded.
// //         /// </summary>
// //         /// <param name="_sceneIndex"></param>
// //         private void ReLoadScene(int _sceneIndex)
// //         {
// //             if (_sceneIndex > 0) //Exclude BootScene with DDOL-Managers.
// //             {
// //                 if (_sceneIndex < SceneManager.sceneCountInBuildSettings - 1)   //-1 marks last SceneIndex.
// //                     SceneManager.LoadScene(_sceneIndex);
// //             }
// //         }

// //         public void QuitGame()
// //         {
// // #if UNITY_EDITOR
// //             UnityEditor.EditorApplication.isPlaying = false;
// // #else
// //             Application.Quit();
// // #endif
// //         }
// //         #endregion

// //         #region CallbackContext-Subscription_Methods
// //         private void OnNavigationInput(InputAction.CallbackContext _callbackContext)
// //         {
// //             if (!Application.isFocused)
// //                 return;
            
// //             if (m_playerInput == null && !m_uiActionMap.enabled)    //Only pass in GameScenes if PauseMenu is opened and PlayerInputs exist.
// //                 return;
            
// //             if (_callbackContext.control.device is Keyboard)        //Because Keyboard works (currently).
// //                 return;

// //             m_navigation.NavigateToNextObject(_callbackContext.ReadValue<Vector2>());
// //         }

// //         private void OnSubmitInput(InputAction.CallbackContext _callbackContext)
// //         {
// //             if (!Application.isFocused)
// //                 return;

// //             if (m_playerInput == null && !m_uiActionMap.enabled)    //Only pass in GameScenes if PauseMenu is opened and PlayerInputs exist.
// //                 return;

// //             if (_callbackContext.control.device is Keyboard)        //Because Keyboard works (currently).
// //                 return;

// //             // if (CursorManager.Instance != null && CursorManager.Instance.IsCursorActive)
// //             // {
// //             //     Debug.Log("Get current highlighted Object and set it as m_lastSelectedGameObject.");
// //             //     return; 
// //             // }
            
// //             if (_callbackContext.ReadValueAsButton())
// //                 ExecuteEvents.Execute(MenuManager.LastSelectedGameObject, new BaseEventData(m_menuManager.m_eventSystem), ExecuteEvents.submitHandler);
// //         }
// //         #endregion
//     }
// }