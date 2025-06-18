using System;
using System.Collections.Generic;
//using System.Linq; //Important for duplicate check.
//using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace ThreeDeePongProto.Shared.Managers
{
    public class RebindManager : PersistentSingleton<RebindManager>
    {
        //public static RebindManager Instance { get; private set; }    //Replaced by/outsourced to PersistentSingleton<RebindManager>.
        [SerializeField] private PlayerInputManager m_playerInputManager;
        [Header("Global Asset Reference")]
        [Tooltip("Drag in the Main-InputActionAsset for system-wide Bindings.")]
        [SerializeField] private InputActionAsset m_globalActionAsset;

        private InputActionRebindingExtensions.RebindingOperation _rebindingOperation;

        private readonly HashSet<int> _playerIndicesWithLoadedBindings = new();         //Memory to prevent multiple loading processes.

        private readonly Dictionary<string, InputAction> _keyboardBindingRegistry = new();
        private readonly Dictionary<string, InputAction> _gamepadBindingRegistry = new();
        private readonly Dictionary<string, InputAction> _mouseBindingRegistry = new();

        private const string SYSTEM_BINDINGS_KEY = "KeyBindings_System";
        private string GetPlayerBindingsKey(int playerIndex) => $"KeyBindings_Player{playerIndex}";

        private const string m_keyboardLayoutName = "Keyboard", m_gamepadLayoutName = "Gamepad", m_mouseLayoutName = "Mouse";

        private const string m_rebindExcludeMouse = "<Mouse>";                          //Control-Paths REQUIRE <>. LayoutNames DO NOT!
        private const string m_keyboardEscape = "<Keyboard>/escape";                    //Control-Paths REQUIRE <>. LayoutNames DO NOT!
        private const string m_gamepadSystemBtn = "<DualSenseGamepadHID>/systemButton"; //Control-Paths REQUIRE <>. LayoutNames DO NOT!
        private const string m_gamepadMicroBtn = "<DualSenseGamepadHID>/micButton";     //Control-Paths REQUIRE <>. LayoutNames DO NOT!

        public event Action OnRebindComplete;
        public event Action OnRebindCanceled;

        protected override void Awake()
        {
            base.Awake();

            LoadGlobalBindings();
        }

        private void OnEnable()
        {
            if (m_playerInputManager != null)
                m_playerInputManager.onPlayerJoined += HandlePlayerJoined;

            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            if (m_playerInputManager != null)
                m_playerInputManager.onPlayerJoined -= HandlePlayerJoined;

            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        #region Load_playerBindings_on_demand
        /// <summary>
        /// This method gets called automatic, when a new player joins.
        /// </summary>
        /// <param name="playerInput">PlayerInput-Instance of the new player.</param>
        private void HandlePlayerJoined(PlayerInput playerInput)
        {
            if (_playerIndicesWithLoadedBindings.Contains(playerInput.playerIndex))
                return;

            Debug.Log($"Load and register bindings of the joined player {playerInput.playerIndex}.");

            //1. Load playerBindings and register them when a player joins.
            LoadPlayerBindings(playerInput);

            //2. Directly add those bindings to the binding-Registries.
            foreach (var action in playerInput.actions)
                RegisterBindingsForAction(action);

            //3. "Remember" that those players are already processed.
            _playerIndicesWithLoadedBindings.Add(playerInput.playerIndex);
        }

        /// <summary>
        /// Method gets called, once a scene is fully loaded.
        /// </summary>
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            InitializeAndPolluteRegistries();
        }
        #endregion

        /// <summary>
        /// Initialize all bindings und fill dictionaries as central binding-registries für gamepad and keyboardMouse.
        /// </summary>
        public void InitializeAndPolluteRegistries()
        {
            //1. Clear Start-Point.
            _keyboardBindingRegistry.Clear();
            _gamepadBindingRegistry.Clear();
            _mouseBindingRegistry.Clear();

            //2. Register global Bindings.
            foreach (var action in m_globalActionAsset)
                RegisterBindingsForAction(action);

            //3. Find all PlayerInput components in the scene.

            //PlayerInput[] preExistingPlayers = FindObjectsOfType<PlayerInput>();
            //if (preExistingPlayers.Length > 0)
            //    Debug.Log($"Found {preExistingPlayers.Length} already existing Player in the Scene. Loading their Bindings...");

            //If preExistingPlayers is needed elsewhere: foreach (var playerInput in preExistingPlayers)
            foreach (var playerInput in FindObjectsOfType<PlayerInput>())      //If preExistingPlayers is not needed elsewhere.
            {
                LoadPlayerBindings(playerInput);

                foreach (var action in playerInput.actions)
                    RegisterBindingsForAction(action);
            }

            Debug.Log($"Central bindingRegistries initialized.");
        }

        #region Player-Specific_Rebinding
        /// <summary>
        /// Starts the interactive Rebinding-Process for a specific Player-Action.
        /// </summary>
        /// <param name="_playerInput">The PlayerInput component of each Player for rebinding.</param>
        /// <param name="_actionToRebind">The action to rebind.</param>
        /// <param name="_bindingIndex">BindingIndex to rebind.</param>
        /// <param name="_excludeMouse">Bool to exclude the mouse or not.</param>
        public void StartPlayerRebinding(PlayerInput _playerInput, InputAction _actionToRebind, int _bindingIndex, bool _excludeMouse)
        {
            if (_playerInput == null || _actionToRebind == null)
                return;

            //Deactivate the ActionMap of the Player to prevent conflicts.
            _playerInput.SwitchCurrentActionMap($"{ActiveInputActionMap.UserInterface}");

            //Closure: Define HERE what is to do. And hand it over to PerformRebinding, to execute it THERE, if required.
            PerformRebinding(_actionToRebind, _bindingIndex, /*_statusText, */_excludeMouse,
            //OnCompleteCallback: Submit what shall happen on a successful rebind operation.
            () =>
            {
                SavePlayerBindings(_playerInput);
                _playerInput.SwitchCurrentActionMap($"{ActiveInputActionMap.PlayerActions}");
            },
            //OnCancelCallback: Submit what shall happen on a canceled rebind operation.
            () =>
            {
                _playerInput.SwitchCurrentActionMap($"{ActiveInputActionMap.PlayerActions}");
            }
        );
        }

        /// <summary>
        /// Saves the overrides for the specific player.
        /// </summary>
        /// <param name="_playerInput"></param>
        internal void SavePlayerBindings(PlayerInput _playerInput)
        {
            var overrides = _playerInput.actions.SaveBindingOverridesAsJson();
            PlayerPrefs.SetString(GetPlayerBindingsKey(_playerInput.playerIndex), overrides);
#if UNITY_EDITOR
            Debug.Log($"Saving Bindings for Player {_playerInput.playerIndex}.");
#endif
        }

        /// <summary>
        /// Loads the overrides for the specific player.
        /// </summary>
        public void LoadPlayerBindings(PlayerInput _playerInput)
        {
            string overridesJson = PlayerPrefs.GetString(GetPlayerBindingsKey(_playerInput.playerIndex));
            if (!string.IsNullOrEmpty(overridesJson))
            {
                _playerInput.actions.LoadBindingOverridesFromJson(overridesJson);
#if UNITY_EDITOR
                Debug.Log($"Loading Bindings for Player {_playerInput.playerIndex}.");
#endif
            }
        }

        /// <summary>
        /// Reset all Bindings für the specific player.
        /// </summary>
        public void ResetAllBindings(PlayerInput playerInput)
        {
            playerInput.actions.RemoveAllBindingOverrides();
            PlayerPrefs.DeleteKey($"KeyBindings_Player{playerInput.playerIndex}");
#if UNITY_EDITOR
            Debug.Log($"All Bindings for Player {playerInput.playerIndex} are resetted.");
#endif
            ResetStatusText();  //Or a single event Action, if required. Actually 'OnRebindCanceled'.
        }
        #endregion

        #region Global_Rebinding
        /// <summary>
        /// Starts Rebinding for a global Action.
        /// </summary>
        public void StartGlobalRebinding(InputAction _actionToRebind, int _bindingIndex, /*TextMeshProUGUI _statusText, */bool _excludeMouse)
        {
            if (m_globalActionAsset == null)
            {
                Debug.LogError("Global InputActionAsset is not assigned in the RebindManager!");
                return;
            }

            //Manage ActionMaps manually.
            m_globalActionAsset.FindActionMap($"{ActiveInputActionMap.PlayerActions}").Disable();
            m_globalActionAsset.FindActionMap($"{ActiveInputActionMap.UserInterface}").Enable();

            //Closure: Define HERE what is to do. And hand it over to PerformRebinding, to execute it THERE, if required.
            PerformRebinding(_actionToRebind, _bindingIndex, /*_statusText, */_excludeMouse,
            () =>
            {
                SaveGlobalBindings();
                m_globalActionAsset.FindActionMap($"{ActiveInputActionMap.PlayerActions}").Enable();
            },
            () =>
            {
                m_globalActionAsset.FindActionMap($"{ActiveInputActionMap.PlayerActions}").Enable();
            });
        }

        internal InputAction GetGlobalAction(string _actionName)
        {
            return m_globalActionAsset.FindAction(_actionName);
        }

        /// <summary>
        /// Save global overrides.
        /// </summary>
        internal void SaveGlobalBindings()
        {
            if (m_globalActionAsset == null)
                return;

            var overrides = m_globalActionAsset.SaveBindingOverridesAsJson();
            PlayerPrefs.SetString(SYSTEM_BINDINGS_KEY, overrides);
            Debug.Log("Saving global bindings.");
        }

        /// <summary>
        /// Load global overrides on game start.
        /// </summary>
        private void LoadGlobalBindings()
        {
            if (m_globalActionAsset == null)
                return;

            string overridesJson = PlayerPrefs.GetString(SYSTEM_BINDINGS_KEY);
            if (!string.IsNullOrEmpty(overridesJson))
            {
                m_globalActionAsset.LoadBindingOverridesFromJson(overridesJson);
                Debug.Log("Loading global bindings.");
            }
        }
        #endregion

        #region Shared_Logic
        /// <summary>
        /// Central method to execute the interactive Rebinding-Process.
        /// </summary>
        private void PerformRebinding(InputAction _actionToRebind, int _bindingIndex, /*TextMeshProUGUI statusText, */bool _excludeMouse, Action _onCompleteCodeBlock, Action _onCancelCodeBlock)
        {
            //statusText.text = "Please press a Button...";

            var originalBinding = _actionToRebind.bindings[_bindingIndex];      //Save original bindings for a later use.

            _rebindingOperation = _actionToRebind.PerformInteractiveRebinding(_bindingIndex)
                .WithControlsExcluding(m_keyboardEscape)                        //Never rebind Escape.
                .WithCancelingThrough(m_keyboardEscape)                         //Use Escape to cancel rebinding.
                .WithControlsExcluding(m_gamepadSystemBtn)                      //Never rebind gamepad's SystemButton.
                .WithControlsExcluding(m_gamepadMicroBtn)                       //Never rebind gamepad's microphone button.
                                                                                //.WithControlsExcluding("<Gamepad>/start")                       //TODO: RShould gamepad's startButton be rebound?
                .OnMatchWaitForAnother(0.1f);

            if (_excludeMouse)
                _rebindingOperation.WithControlsExcluding(m_rebindExcludeMouse);

            //Call, while a button is pressed. But before the ReBinding is finished.
            _rebindingOperation.OnApplyBinding((operation, newPath) =>
            {
                if (CheckForDuplicateBinding(_actionToRebind, newPath))
                {
                    //Cancel the operation, if a duplicate is found.
                    //statusText.text = "Button already assigned!";
                    _actionToRebind.RemoveBindingOverride(_bindingIndex);   //Remove temporary Changes.
                    //.Dispose() replace .Cancel(), to turn from a fix cancelling behavior to custom-controlled UI behavior. 
                    operation.Dispose();
                    Invoke(nameof(ResetStatusText), 1.5f);                  //Cancel-Delay so the player can read the Information.
                }
            })
            //Call on successful Rebinding, if no duplicate was found.
            .OnComplete(operation =>
            {
                //1. Unregister old bindings in dictionaries.
                UnregisterBinding(originalBinding);
                //2. Register new bindings in dictionaries.
                RegisterBinding(_actionToRebind.bindings[_bindingIndex], _actionToRebind);

                //statusText.text = string.Empty;
                operation.Dispose();
                _onCompleteCodeBlock?.Invoke(); //On Success Save Player/System-Bindings & switch back to PlayerActionMap.
                OnRebindComplete?.Invoke();
            })
            .OnCancel(operation =>
            {
                //statusText.text = string.Empty;
                operation.Dispose();
                _onCancelCodeBlock?.Invoke(); //On Cancel just switch back to PlayerActionMap.
                OnRebindCanceled?.Invoke();
            });

            _rebindingOperation.Start();
        }

        private void ResetStatusText()
        {
            OnRebindCanceled?.Invoke(); //Update UI.
        }

        /// <summary>
        /// New global DuplicateBinding-Check using the new deviceBinding-registries.
        /// </summary>
        /// <param name="actionToRebind">Current Action to rebind.</param>
        /// <param name="newPath">Button for the new command.</param>
        /// <returns>True, if the button is already occupied by another Action.</returns>
        private bool CheckForDuplicateBinding(InputAction actionToRebind, string newPath)
        {
            var isKeyboard = InputSystem.IsFirstLayoutBasedOnSecond(newPath, m_keyboardLayoutName);   //No <> on Layout-Names!
            var isGamepad = InputSystem.IsFirstLayoutBasedOnSecond(newPath, m_gamepadLayoutName);     //No <> on Layout-Names!
            var isMouse = InputSystem.IsFirstLayoutBasedOnSecond(newPath, m_mouseLayoutName);         //No <> on Layout-Names!

            InputAction conflictingAction = null;

            if (isKeyboard)
            {
                _keyboardBindingRegistry.TryGetValue(newPath, out conflictingAction);
            }
            else if (isGamepad)
            {
                _gamepadBindingRegistry.TryGetValue(newPath, out conflictingAction);
            }
            else if (isMouse)
            {
                _mouseBindingRegistry.TryGetValue(newPath, out conflictingAction);
            }

            //Conflict-case, if another Action already uses the rebinding path we try to set.
            if (conflictingAction != null && conflictingAction != actionToRebind)
            {
                Debug.LogWarning($"Duplicate found! '{newPath}' is already in use by Action '{conflictingAction.name}'.");
                return true;
            }

            return false;
        }

        #region Helper-Methods_for_dictionary_population.
        private void RegisterBindingsForAction(InputAction action)
        {
            foreach (var binding in action.bindings)
            {
                //Ignore bindings that aren't mapped to a specific key. (like composite wrappers)
                if (string.IsNullOrEmpty(binding.effectivePath))
                    continue;

                //Pass the parent 'action' object along with the individual 'binding'.
                RegisterBinding(binding, action);
            }
        }

        private void RegisterBinding(InputBinding binding, InputAction inputAction)
        {
            var path = binding.effectivePath;

            if (string.IsNullOrEmpty(path) || inputAction == null)
                return;

            //Get the device of the binding. And assign the 'parentAction' object, which is the correct type for our dictionary.
            if (IsKeyboardBinding(binding))
            {
                _keyboardBindingRegistry[path] = inputAction;
            }
            else if (IsGamepadBinding(binding))
            {
                _gamepadBindingRegistry[path] = inputAction;
            }
            else if (IsMouseBinding(binding))
            {
                _mouseBindingRegistry[path] = inputAction;
            }
        }

        private void UnregisterBinding(InputBinding binding)
        {
            var path = binding.effectivePath;
            var actionName = binding.action;

            if (string.IsNullOrEmpty(path) || string.IsNullOrEmpty(actionName))
                return;

            //May write: if(registeredAction.name == binding.action) { _keyboardBindingRegistry.Remove(path); } instead of && in 1st if{}.
            if (IsKeyboardBinding(binding))
            {
                //Remove the dictionary entry only if it is equal to the Action.
                if (_keyboardBindingRegistry.TryGetValue(path, out var registeredAction) && registeredAction.name == binding.action)
                    _keyboardBindingRegistry.Remove(path);
            }
            else if (IsGamepadBinding(binding))
            {
                if (_gamepadBindingRegistry.TryGetValue(path, out var registeredAction) && registeredAction.name == binding.action)
                    _gamepadBindingRegistry.Remove(path);
            }
            else if (IsMouseBinding(binding))
            {
                if (_mouseBindingRegistry.TryGetValue(path, out var registeredAction) && registeredAction.name == binding.action)
                    _mouseBindingRegistry.Remove(path);
            }
        }

        //Helper-methods to determine the deviceType.
        private bool IsKeyboardBinding(InputBinding binding)
        {
            return binding.groups.Contains("Keyboard", StringComparison.InvariantCultureIgnoreCase);
        }

        private bool IsGamepadBinding(InputBinding binding)
        {
            return binding.groups.Contains("Gamepad", StringComparison.InvariantCultureIgnoreCase);
        }

        private bool IsMouseBinding(InputBinding binding)
        {
            return binding.effectivePath != null && binding.effectivePath.StartsWith("<Mouse>", StringComparison.InvariantCultureIgnoreCase);
        }
        #endregion
        #endregion

        /// <summary>
        /// Method to reset Lists of already processed players, whenever player disappear or are no longer used.
        /// </summary>
        public void ResetProcessedPlayersList()
        {
            _playerIndicesWithLoadedBindings.Clear();
            Debug.Log("Resetting already processed playerLists.");
        }
    }
}