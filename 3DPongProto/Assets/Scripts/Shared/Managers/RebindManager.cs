using System;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ThreeDeePongProto.Shared.Managers
{
    public class RebindManager : PersistentSingleton<RebindManager>
    {
        [Tooltip(tooltip: "Reference of the Main-InputActionAsset for global Rebindings.")]
        [SerializeField] private InputActionAsset m_mainActionAsset;

        private InputActionRebindingExtensions.RebindingOperation m_rebindingOperation;

        public event Action OnRebindComplete;
        public event Action OnRebindCanceled;
        public event Action<string> OnRebindError; //To prevent errors like "Button is already set to..."

        private const string GLOBAL_BINDINGS_FILENAME = "global_bindings.json";
        private string GetPlayerBindingsPath(int _playerIndex) => Path.Combine(Application.persistentDataPath, $"player_{_playerIndex}_bindings.json");

        #region WithCancelingThrough- & WithControlsExcluding-Strings
        //Control-Paths REQUIRE <>. LayoutNames DO NOT!
        private const string m_rebindExcludeMouse = "<Mouse>";
        private const string m_keyboardEscape = "<Keyboard>/escape";
        private const string m_gamepadSelect = "<Gamepad>/select";
        private const string m_gamepadStart = "<Gamepad>/start";
        private const string m_gamepadSysBtn = "<Gamepad>/systemButton";
        private const string m_gamepadSysBtnDS = "<DualSenseGamepadHID>/systemButton";
        private const string m_gamepadMicroBtn = "<DualSenseGamepadHID>/micButton";
        private const string m_keyboardAnyKey = "<Keyboard>/anyKey";
        private const string m_gamepadAnyKey = "<Gamepad>/<Button>";
        #endregion

        protected override void Awake()
        {
            base.Awake();
            LoadAllBindings();
        }

        #region Player-Specific_Rebinding
        /// <summary>
        /// Starts the interactive Rebinding-Process for a specific Player-Action.
        /// </summary>
        /// <param name="_playerInput">The PlayerInput component of each Player for rebinding.</param>
        /// <param name="_actionToRebind">The action to rebind.</param>
        /// <param name="_bindingIndex">BindingIndex to rebind.</param>
        /// <param name="_excludeMouse">Bool to exclude the mouse or not.</param>
        public void StartPlayerRebinding(PlayerInput _playerInput, string _actionName, int _bindingIndex, TextMeshProUGUI _textComponent, bool _excludeMouse)
        {
            InputAction action = _playerInput.actions.FindAction(_actionName, throwIfNotFound: true);
            PerformRebinding(action, _bindingIndex, _playerInput, _excludeMouse);
        }
        #endregion

        #region Global_Rebinding
        /// <summary>
        /// Starts Rebinding for a global Action.
        /// </summary>
        public void StartGlobalRebinding(string _actionName, int _bindingIndex, TextMeshProUGUI _textComponent, bool _excludeMouse)
        {
            InputAction actionToRebind = m_mainActionAsset.FindAction(_actionName, throwIfNotFound: true);
            PerformRebinding(actionToRebind, _bindingIndex, null, _excludeMouse);
        }

        internal InputAction GetGlobalAction(string _actionName)
        {
            return m_mainActionAsset.FindAction(_actionName);
        }
        #endregion

        #region Shared_Logic
        /// <summary>
        /// Central method to execute the interactive Rebinding-Process.
        /// </summary>
        private void PerformRebinding(InputAction _actionToRebind, int _bindingIndex, PlayerInput _playerInput, bool _excludeMouse)
        {
            //statusText.text = "Press a Button";
            m_rebindingOperation?.Cancel();
            _actionToRebind.actionMap.Disable();

            m_rebindingOperation = _actionToRebind.PerformInteractiveRebinding(_bindingIndex)
                .WithCancelingThrough(m_keyboardEscape)                         //Use Escape to cancel rebinding.
                .WithCancelingThrough(m_gamepadSelect)                          //Use Select button to cancel rebinding.
                .WithControlsExcluding(m_keyboardAnyKey)                        //Any Key is NOT an option!
                .WithControlsExcluding(m_gamepadAnyKey)                         //WildCard to create a "gamepad-Any Key".
                .WithControlsExcluding(m_keyboardEscape)                        //Never rebind Escape.
                .WithControlsExcluding(m_gamepadSysBtn)                         //PlayStation/Xbox Home Button
                .WithControlsExcluding(m_gamepadSysBtnDS)                       //Never rebind gamepad's SystemButton.
                .WithControlsExcluding(m_gamepadMicroBtn)                       //Never rebind gamepad's microphone button.
                .WithControlsExcluding(m_gamepadStart)                          //TODO: Should gamepad's startButton be rebound?
                .OnMatchWaitForAnother(0.1f);

            if (_excludeMouse)
                m_rebindingOperation.WithControlsExcluding(m_rebindExcludeMouse);

            //Call, while a button is pressed. But before the ReBinding is finished.
            m_rebindingOperation.OnComplete(operation =>
            {
                string newPath = operation.action.bindings[_bindingIndex].effectivePath;
                if (newPath != null && (newPath.EndsWith("/anyKey") || newPath.EndsWith("/<Button>")))
                {
                    //"anyKey" is handled equal to a cancelation.
                    operation.action.RemoveBindingOverride(_bindingIndex); //Remove any change.
                    operation.Dispose();
                    _actionToRebind.actionMap.Enable();
                    OnRebindCanceled?.Invoke(); //Notify the UI to update itself, after the process is aborted.
                    Debug.LogWarning("This special Command is excluded from Rebinding to prevent errors.");
                    return; //Exit./Don't execute the following code.
                }

                //Check if another action is using the binding already.
                if (CheckForDuplicate(operation.action, _bindingIndex, _playerInput))
                {
                    operation.action.RemoveBindingOverride(_bindingIndex);
                    OnRebindError?.Invoke("This button is already set to another command!");
                }

                operation.Dispose();
                _actionToRebind.actionMap.Enable();
                SaveAllBindings();
                OnRebindComplete?.Invoke(); //Notify the UI to update itself on completion.
            });

            m_rebindingOperation.OnCancel(operation =>
            {
                operation.Dispose();
                _actionToRebind.actionMap.Enable();
                OnRebindCanceled?.Invoke(); //Notify the UI to update itself, after the process is aborted.
            });

            m_rebindingOperation.Start();
        }

        /// <summary>
        /// Checks for duplicate bindings. A binding is only a duplicate if it shares the same path 
        /// AND is in the same Action Map. This check now uses a more robust self-identification
        /// to prevent flagging the binding that is currently being changed as a duplicate of itself.
        /// </summary>
        private bool CheckForDuplicate(InputAction _actionToRebind, int _bindingIndex, PlayerInput _playerForContext)
        {
            InputBinding newBinding = _actionToRebind.bindings[_bindingIndex];
            if (string.IsNullOrEmpty(newBinding.effectivePath))
                return false;

            //1. Check against all global bindings
            foreach (var action in m_mainActionAsset.actionMaps.SelectMany(map => map.actions))
            {
                if (action.actionMap != _actionToRebind.actionMap)
                    continue;

                for (int i = 0; i < action.bindings.Count; i++)
                {
                    var binding = action.bindings[i];
                    if (string.IsNullOrEmpty(binding.effectivePath))
                        continue;

                    //Robust-Self-Check for the same global action.
                    bool isSelf = _playerForContext == null && action == _actionToRebind && i == _bindingIndex;
                    if (isSelf)
                        continue;

                    if (binding.effectivePath == newBinding.effectivePath)
                    {
                        Debug.LogWarning($"Duplicate found! Path '{newBinding.effectivePath}' is already used by global action '{action.name}' in the same action map.");
                        return true;
                    }
                }
            }

            //2. Check against all other player bindings
            foreach (var player in UserInputManager.Instance.GetActivePlayers())
            {
                foreach (var action in player.actions)
                {
                    if (action.actionMap != _actionToRebind.actionMap)
                        continue;

                    for (int i = 0; i < action.bindings.Count; i++)
                    {
                        var binding = action.bindings[i];
                        if (string.IsNullOrEmpty(binding.effectivePath))
                            continue;

                        //Robust-Self-Check for the same player-based action.
                        bool isSelf = player == _playerForContext && action == _actionToRebind && i == _bindingIndex;
                        if (isSelf)
                            continue;

                        if (binding.effectivePath == newBinding.effectivePath)
                        {
                            Debug.LogWarning($"Duplicate found! Path '{newBinding.effectivePath}' is already used by Player {player.playerIndex} for action '{action.name}' in the same action map.");
                            return true;
                        }
                    }
                }
            }

            return false; //No duplicate found.
        }

        #region Helper-Methods_for_dictionary_population.
        public string GetBindingDisplayString(string _actionName, int _bindingIndex, PlayerInput _playerInput, bool _isGlobal)
        {
            InputAction action = _isGlobal ? GetGlobalAction(_actionName) : _playerInput.actions.FindAction(_actionName);

            if (action == null)
                return "N/A";

            return action.GetBindingDisplayString(_bindingIndex);
        }
        #endregion

        private void SaveAllBindings()
        {
            var globalOverrides = m_mainActionAsset.SaveBindingOverridesAsJson();
            File.WriteAllText(Path.Combine(Application.persistentDataPath, GLOBAL_BINDINGS_FILENAME), globalOverrides);

            foreach (var player in UserInputManager.Instance.GetActivePlayers())
            {
                var playerOverrides = player.actions.SaveBindingOverridesAsJson();
                File.WriteAllText(GetPlayerBindingsPath(player.playerIndex), playerOverrides);
            }
            Debug.Log("All bindings saved.");
        }

        public void LoadAllBindings()
        {
            string globalFilePath = Path.Combine(Application.persistentDataPath, GLOBAL_BINDINGS_FILENAME);
            if (File.Exists(globalFilePath))
            {
                string json = File.ReadAllText(globalFilePath);
                m_mainActionAsset.LoadBindingOverridesFromJson(json);
            }
        }

        public void LoadPlayerBindings(PlayerInput _player)
        {
            string playerFilePath = GetPlayerBindingsPath(_player.playerIndex);
            if (File.Exists(playerFilePath))
            {
                string json = File.ReadAllText(playerFilePath);
                _player.actions.LoadBindingOverridesFromJson(json);
                Debug.Log($"Bindings for Player {_player.playerIndex} loaded.");
            }
        }

        public void ResetSpecificBinding(string _actionName, int _bindingIndex, PlayerInput _playerInput, bool _isGlobal)
        {
            InputAction actionToReset = _isGlobal ? m_mainActionAsset.FindAction(_actionName, throwIfNotFound: true)
            : _playerInput.actions.FindAction(_actionName, throwIfNotFound: true);

            actionToReset.RemoveBindingOverride(_bindingIndex);
            SaveAllBindings();
            OnRebindComplete?.Invoke(); //Notify UI-Update method.
        }

        /// <summary>
        /// Reset all Bindings for the specific player of the active UI-window and all global Actions.
        /// </summary>
        public void ResetAllBindings(/*PlayerInput _playerInput*/)
        {
            //            _playerInput.actions.RemoveAllBindingOverrides();
            //            PlayerPrefs.DeleteKey($"KeyBindings_Player{_playerInput.playerIndex}");
            //#if UNITY_EDITOR
            //            Debug.Log($"All Bindings for Player {_playerInput.playerIndex} are resetted.");
            //#endif
            //            ResetStatusText();  //Or a single event Action, if required. Actually 'OnRebindCanceled'.
        }
        #endregion
    }
}