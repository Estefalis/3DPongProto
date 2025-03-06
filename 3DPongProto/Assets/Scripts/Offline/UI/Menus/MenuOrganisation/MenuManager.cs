using System;
using System.Collections.Generic;
using System.Linq;
using ThreeDeePongProto.Shared.Managers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

//'UserInputManager.cs' adds PlayerInput component on runtime!
namespace ThreeDeePongProto.Offline.UI.Menu
{
    public class MenuManager : MonoBehaviour
    {
        private PlayerInput m_menuInput;
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
        private static GameObject m_lastSelectedGameObject;
        #endregion

        #region GameScene-Variables
        [Header("GameScene Variables")]
        [SerializeField] private Button m_hiddenFinishButton;

        public static event Action AResumeTheGame;          //LocalMatchManager unpauses the Game.
        public static event Action<int> AReLoadScene;       //UserInputManager with central SceneManager.LoadScene().
        public static event Action AEndInfiniteMatch;

        private const string m_keyboardMouse = "KeyboardMouse", m_keyboardSchemePID0 = "KeyboardPlayerID0", m_keyboardSchemePID1 = "KeyboardPlayerID1", m_keyboardSchemePID2 = "KeyboardPlayerID2", m_keyboardSchemePID3 = "KeyboardPlayerID3", m_keyboardDevice = "Keyboard";

        private const string m_closeGameMenuString = "CloseGameMenu";
        private const string m_uiActionMap = "UserInterface", m_playerActionMap = "PlayerActions";
        private string m_currentControlScheme;
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

            m_menuInput = GetComponent<PlayerInput>();
            SetMenuInputDefault(m_menuInput);

            m_lastSelectedGameObject = null;
            m_selectedElement.Clear();

            SetFirstStackElement(m_firstElement);
            SetUIElements();    //Also sets the lastSelectedElement.
        }

        private void OnEnable()
        {
            InputActionMap uiActionMap = m_menuInput.actions.FindActionMap(m_uiActionMap, true);
            if (uiActionMap != null)
            {
                uiActionMap.Enable();
                InputAction closeGameMenuAction = uiActionMap.FindAction(m_closeGameMenuString, true);
                if (closeGameMenuAction != null)
                {
                    closeGameMenuAction.performed += CloseMenu;
                }
            }

            AResumeTheGame += OnResumeTheGame;
            UserInputManager.AChangeActiveActionMap += OnChangeActiveActionMap;
            UserInputManager.AOnDeviceInput += OnUserHandledDevice;
        }

        private void OnResumeTheGame()
        {
            if (m_firstElement.gameObject.activeInHierarchy)
            {
                UserInputManager.ToggleActionMaps(m_playerActionMap);
                m_firstElement.gameObject.SetActive(false);
            }
        }

        private void OnDisable()
        {
            AResumeTheGame -= OnResumeTheGame;
            UserInputManager.AChangeActiveActionMap -= OnChangeActiveActionMap;
            UserInputManager.AOnDeviceInput -= OnUserHandledDevice;

            InputActionMap uiActionMap = m_menuInput.actions.FindActionMap(m_uiActionMap, true);
            if (uiActionMap != null)
            {
                InputAction closeGameMenuAction = uiActionMap.FindAction(m_closeGameMenuString, true);
                if (closeGameMenuAction != null)
                {
                    closeGameMenuAction.performed -= CloseMenu;
                }
                uiActionMap.Disable();
            }
        }

        private void OnDestroy()
        {
            AResumeTheGame -= OnResumeTheGame;
            UserInputManager.AChangeActiveActionMap -= OnChangeActiveActionMap;
            UserInputManager.AOnDeviceInput -= OnUserHandledDevice;

            InputActionMap uiActionMap = m_menuInput.actions.FindActionMap(m_uiActionMap, true);
            if (uiActionMap != null)
            {
                InputAction closeGameMenuAction = uiActionMap.FindAction(m_closeGameMenuString, true);
                if (closeGameMenuAction != null)
                {
                    closeGameMenuAction.performed -= CloseMenu;
                }
                uiActionMap.Disable();
            }
        }

        private void Start()
        {
            if (m_hiddenFinishButton != null)
                InVisibleButton(m_matchUIStates.InfiniteMatch); //m_matchUIStates get load in LoadSettingsValues > Awake().
        }

        private void Update()
        {
            UpdateLastSelectedObject();
        }

        #region Non_InputAction_Subscriptions
        private void OnChangeActiveActionMap(string _actionMap)
        {
            //TODO: DON'T switch to playerActionMap in Scenes with Menus only!
            var sceneIndex = SceneManager.GetActiveScene().buildIndex;
            if (sceneIndex == (int)ESceneNames.StartMenu)   //Or in other menuOnly Scenes.
                return;

            switch (_actionMap)
            {
                case m_uiActionMap:
                {
                    if (!m_firstElement.gameObject.activeInHierarchy)
                    {
                        m_firstElement.gameObject.SetActive(true);
                        SetNavigationGameObject(m_firstElement);
                    }
                    Debug.Log($"Assigned Device(s): {string.Join(", ", m_menuInput.devices.Select(d => d.name))} | CurActionMap: {m_menuInput.currentActionMap.name} | CurControlScheme: {m_menuInput.currentControlScheme}.");
                    m_menuInput.SwitchCurrentActionMap(m_uiActionMap);
                    break;
                }
                case m_playerActionMap:
                {
                    m_menuInput.SwitchCurrentActionMap(m_playerActionMap);
                    break;
                }
                default:
                    break;
            }
        }

        private void OnUserHandledDevice(string _controlScheme, InputDevice[] _devices)
        {
            m_menuInput.SwitchCurrentControlScheme(_controlScheme, _devices);
            m_currentControlScheme = _controlScheme;
        }
        #endregion

        #region PlayerInput-Configuration_on_Menu
        private void SetMenuInputDefault(PlayerInput _menuInput)
        {
            if (_menuInput == null)
                m_menuInput = gameObject.AddComponent<PlayerInput>();

            m_menuInput.actions = Resources.Load<InputActionAsset>("InputActions/PlayerInputActions");
            m_menuInput.defaultActionMap = m_uiActionMap;
            //Enable active ControlScheme switch.
            m_menuInput.neverAutoSwitchControlSchemes = false;
            //Set the notificationBehavior of the PlayerInput component.
            m_menuInput.notificationBehavior = PlayerNotifications.InvokeCSharpEvents;

            m_menuInput.SwitchCurrentActionMap(m_uiActionMap);
            m_menuInput.defaultControlScheme = m_keyboardMouse;
            m_currentControlScheme = m_keyboardMouse;
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
                            //TODO: Update to new PlayerInputManager + PlayerInput Combo.
                            m_lastSelectedGameObject = /*RebindManager.m_PlayerInputActions.UserInterface.enabled ? m_lastMenuSceneObject = m_eventSystem.currentSelectedGameObject : m_lastGameSceneObject = */m_eventSystem.currentSelectedGameObject;
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
            if (sceneIndex == (int)ESceneNames.StartMenu)   //Or in other menuOnly Scenes.
                return;

            AResumeTheGame?.Invoke();
        }
    }
}