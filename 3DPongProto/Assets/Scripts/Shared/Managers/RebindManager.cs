using System;
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
        [SerializeField] private bool m_useEncryption = false;

        public event Action OnRebindComplete;
        public event Action OnRebindCanceled;

        private string m_targetActionMap;

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

        #region Serialization        
        private IPersistentData<OverrideSaveData> m_saveSystem; //The single, unified save system for all originalBinding overrides.
        private OverrideSaveData m_overrideData;                //The live data object holding all overrides.

        private InputActionRebindingExtensions.RebindingOperation m_rebindingOperation; //Current rebind _operation.

        private static readonly string m_keyBindingOverrideFolderPath = "/SaveData/KeyReBinds/";
        private static readonly string m_fileName = "bindingOverrides.json";
        #endregion

        protected override void Awake()
        {
            base.Awake();

            //Configure the save system once.
            m_saveSystem = new SerializingData<OverrideSaveData>(m_fileName, m_keyBindingOverrideFolderPath, m_useEncryption);
            LoadAllBindings();                  //Load all overrides at gameStart.
        }

        private void OnEnable()
        {
            m_targetActionMap = UserInputManager.m_CentralActionsInstance.PlayerActions.ToString();
        }

        private void OnDisable()
        {
            m_rebindingOperation?.Dispose();    //Clean up the rebind _operation, when the game closes and/or the gameObject is disabled.
        }

        /// <summary>
        /// Initiates the rebinding process for a specific _action originalBinding.
        /// </summary>
        public void StartRebinding(string _actionName, int _bindingIndex, TextMeshProUGUI _buttonText, bool _excludeMouse, int _playerIndex)
        {
            InputAction action = m_mainActionAsset.FindAction(_actionName);
            if (action == null || action.bindings.Count <= _bindingIndex)
            {
                Debug.LogError($"Cannot find action '{_actionName}' or binding index '{_bindingIndex}' is out of range.");
                return;
            }

            m_rebindingOperation?.Cancel(); //Cancel a previous rebind _operation, if one exists.
            action.actionMap.Disable();     //Disable the _action map to prevent input during rebinding.

            //Update UI to give user feedback.
            _buttonText.text = "Press a button";

            //1. Store the _operation in a variable.
            m_rebindingOperation = action.PerformInteractiveRebinding(_bindingIndex);

            //2. Conditionally exclude the mouse.
            if (_excludeMouse)
                m_rebindingOperation.WithControlsExcluding(m_rebindExcludeMouse);   //Optionally exclude mouse.

            //3. Chain the rest of the calls onto the variable.
            m_rebindingOperation
                .WithCancelingThrough(m_keyboardEscape)                         //Use Escape to cancel rebinding.
                .WithCancelingThrough(m_gamepadSelect)                          //Use Select button to cancel rebinding.
                .WithControlsExcluding(m_keyboardAnyKey)                        //Any Key is NOT an option!
                .WithControlsExcluding(m_gamepadAnyKey)                         //WildCard to create a "gamepad-Any Key".
                .WithControlsExcluding(m_keyboardEscape)                        //Never rebind Escape.
                .WithControlsExcluding(m_gamepadSysBtn)                         //PlayStation/Xbox Home Button
                .WithControlsExcluding(m_gamepadSysBtnDS)                       //Never rebind gamepad's SystemButton.
                .WithControlsExcluding(m_gamepadMicroBtn)                       //Never rebind gamepad's microphone button.
                .WithControlsExcluding(m_gamepadStart)                          //TODO: Should gamepad's startButton be rebound?
                .OnComplete(operation => ProcessRebindCompletion(operation, action, _bindingIndex, _playerIndex, _buttonText))
                .OnCancel(operation => ProcessRebindCancellation(operation, _buttonText))
                .Start();
        }

        #region Old_Player-Specific_Rebinding
        ///// <summary>
        ///// Starts the interactive Rebinding-Process for a specific Player-Action.
        ///// </summary>
        ///// <param name="_playerInput">The PlayerInput component of each Player for rebinding.</param>
        ///// <param name="_actionToRebind">The _action to rebind.</param>
        ///// <param name="_bindingIndex">BindingIndex to rebind.</param>
        ///// <param name="_excludeMouse">Bool to exclude the mouse or not.</param>
        //public void StartPlayerRebinding(PlayerInput _playerInput, string _actionName, int _bindingIndex, TextMeshProUGUI _textComponent, bool _excludeMouse)
        //{
        //    InputAction _action = _playerInput.actions.FindAction(_actionName, throwIfNotFound: true);
        //    PerformRebinding(_action, _bindingIndex, _playerInput, _textComponent, _excludeMouse);
        //}
        #endregion

        #region Old_Global_Rebinding
        ///// <summary>
        ///// Starts Rebinding for a global Action.
        ///// </summary>
        //public void StartGlobalRebinding(string _actionName, int _bindingIndex, TextMeshProUGUI _textComponent, bool _excludeMouse)
        //{
        //    InputAction actionToRebind = m_mainActionAsset.FindAction(_actionName, throwIfNotFound: true);
        //    PerformRebinding(actionToRebind, _bindingIndex, null, _textComponent, _excludeMouse);
        //}

        //internal InputAction GetGlobalAction(string _actionName)
        //{
        //    return m_mainActionAsset.FindAction(_actionName);
        //}
        #endregion

        #region Shared_Logic
        /// <summary>
        /// Called when the interactive rebinding process is successfully completed.
        /// </summary>
        private void ProcessRebindCompletion(InputActionRebindingExtensions.RebindingOperation _operation, InputAction _action, int _bindingIndex, int _playerIndex, TextMeshProUGUI _buttonText)
        {
            string newPath = _operation.selectedControl.path;
            InputBinding originalBinding = _action.bindings[_bindingIndex]; //Get the specific originalBinding.
            string originalPath = originalBinding.effectivePath; // Capture the original path BEFORE the change

            //Clean up the _operation memory.
            _operation.Dispose();

            //Check if the new originalBinding is a duplicate of an existing one.
            InputBinding? duplicateBinding = FindDuplicateBinding(originalBinding, newPath);

            if (duplicateBinding.HasValue)
            {
                //A duplicate was found. Let's see what kind.
                InputAction duplicateAction = m_mainActionAsset.FindAction(duplicateBinding.Value.action);

                //Is the duplicate on the SAME _action? If so, we perform a SWAP.
                if (duplicateAction.id == _action.id)
                {
                    Debug.Log($"Duplicate is on the same action '{_action.name}'. Performing a swap.");

                    //Apply the new binding to the original slot (e.g., "left" gets "D").
                    _action.ApplyBindingOverride(_bindingIndex, newPath);
                    UpdateOverrideInList(_action.name, _bindingIndex, _playerIndex, newPath);

                    //Find the index of the duplicate binding within the _action.
                    int duplicateBindingIndex = _action.bindings.IndexOf(b => b.id == duplicateBinding.Value.id);
                    if (duplicateBindingIndex != -1)
                    {
                        //Apply the original binding to the duplicate's slot (e.g., "right" gets "A").
                        _action.ApplyBindingOverride(duplicateBindingIndex, originalPath);
                        UpdateOverrideInList(_action.name, duplicateBindingIndex, _playerIndex, originalPath);
                    }
                }
                else
                {
                    //It's on a DIFFERENT _action. This is a real conflict. Abort.
                    Debug.LogWarning($"Binding '{newPath}' is already used by a different action: '{duplicateAction.name}'. Reverting.");
                    ProcessRebindCancellation(_operation, _buttonText); // Treat as a cancellation
                    return; // Exit without saving
                }
            }
            else
            {
                //No duplicate found. Perform a simple rebind.
                _action.ApplyBindingOverride(_bindingIndex, newPath);
                UpdateOverrideInList(_action.name, _bindingIndex, _playerIndex, newPath);
            }

            SaveBindingOverrides();         //Save all changes to the file.
            
            _action.actionMap.Enable();     //Re-enable the _action map.
            
            OnRebindComplete?.Invoke();     //Notify UI to update it's display.
        }

        /// <summary>
        /// Helper method to create or update an override entry in our central data list.
        /// </summary>
        private void UpdateOverrideInList(string _actionName, int _bindingIndex, int _playerIndex, string _newPath)
        {
            var existingOverride = m_overrideData.bindingOverrides.FirstOrDefault(x =>
                x.playerIndex == _playerIndex &&
                x.actionName == _actionName &&
                x.bindingIndex == _bindingIndex);

            if (existingOverride != null)
            {
                existingOverride.overridePath = _newPath;
            }
            else
            {
                m_overrideData.bindingOverrides.Add(new KeyBindingOverride
                {
                    actionName = _actionName,
                    bindingIndex = _bindingIndex,
                    playerIndex = _playerIndex,
                    overridePath = _newPath
                });
            }
        }

        /// <summary>
        /// Called when the interactive rebinding process is canceled by the user.
        /// </summary>
        private void ProcessRebindCancellation(InputActionRebindingExtensions.RebindingOperation _operation, TextMeshProUGUI _buttonText)
        {
            _operation.Dispose();

            //Re-enable the _action map.
            m_mainActionAsset.FindActionMap(m_targetActionMap)?.Enable();

            //Notify UI to revert its text.
            OnRebindCanceled?.Invoke();
        }

        /// <summary>
        /// Applies all stored overrides to a newly joined player.
        /// </summary>
        public void ApplyOverridesToPlayer(PlayerInput playerInput)
        {
            //Apply all global overrides first.
            foreach (var ov in m_overrideData.bindingOverrides.Where(x => x.playerIndex == -1))
            {
                var action = playerInput.actions.FindAction(ov.actionName);
                action?.ApplyBindingOverride(ov.bindingIndex, ov.overridePath);
            }

            //Then, apply the specific overrides for this player, which will take precedence.
            foreach (var ov in m_overrideData.bindingOverrides.Where(x => x.playerIndex == playerInput.playerIndex))
            {
                var action = playerInput.actions.FindAction(ov.actionName);
                action?.ApplyBindingOverride(ov.bindingIndex, ov.overridePath);
            }
        }

        #region Old PerformRebinding
        ///// <summary>
        ///// Central method to execute the interactive Rebinding-Process.
        ///// </summary>
        //private void PerformRebinding(InputAction _actionToRebind, int _bindingIndex, PlayerInput _playerInput, TextMeshProUGUI _textComponent, bool _excludeMouse)
        //{
        //    _textComponent.text = "Press a Button";

        //    m_rebindingOperation?.Cancel();
        //    _actionToRebind.actionMap.Disable();

        //    m_rebindingOperation = _actionToRebind.PerformInteractiveRebinding(_bindingIndex)
        //        .WithCancelingThrough(m_keyboardEscape)                         //Use Escape to cancel rebinding.
        //        .WithCancelingThrough(m_gamepadSelect)                          //Use Select button to cancel rebinding.
        //        .WithControlsExcluding(m_keyboardAnyKey)                        //Any Key is NOT an option!
        //        .WithControlsExcluding(m_gamepadAnyKey)                         //WildCard to create a "gamepad-Any Key".
        //        .WithControlsExcluding(m_keyboardEscape)                        //Never rebind Escape.
        //        .WithControlsExcluding(m_gamepadSysBtn)                         //PlayStation/Xbox Home Button
        //        .WithControlsExcluding(m_gamepadSysBtnDS)                       //Never rebind gamepad's SystemButton.
        //        .WithControlsExcluding(m_gamepadMicroBtn)                       //Never rebind gamepad's microphone button.
        //        .WithControlsExcluding(m_gamepadStart)                          //TODO: Should gamepad's startButton be rebound?
        //        .OnMatchWaitForAnother(0.1f);

        //    if (_excludeMouse)
        //        m_rebindingOperation.WithControlsExcluding(m_rebindExcludeMouse);

        //    //Call, while a button is pressed. But before the ReBinding is finished.
        //    m_rebindingOperation.OnComplete(_operation =>
        //    {
        //        string _newPath = _operation._action.bindings[_bindingIndex].effectivePath;
        //        if (_newPath != null && (_newPath.EndsWith("/anyKey") || _newPath.EndsWith("/<Button>")))
        //        {
        //            //"anyKey" is handled equal to a cancelation.
        //            _operation._action.RemoveBindingOverride(_bindingIndex); //Remove any change.
        //            _operation.Dispose();
        //            _actionToRebind.actionMap.Enable();
        //            OnRebindCanceled?.Invoke(); //Notify the UI to update itself, after the process is aborted.
        //            Debug.LogWarning("This special Command is excluded from Rebinding to prevent errors.");
        //            return; //Exit./Don't execute the following code.
        //        }

        //        //Check if another _action is using the originalBinding already.
        //        if (CheckForDuplicateBinding(_operation._action, _bindingIndex, _playerInput))
        //        {
        //            _operation._action.RemoveBindingOverride(_bindingIndex);
        //            //OnRebindError?.Invoke("This button is already set to another command!");
        //        }
        //        else
        //            SaveAllBindings();

        //        _operation.Dispose();
        //        _actionToRebind.actionMap.Enable();
        //        OnRebindComplete?.Invoke(); //Notify the UI to update itself on completion.
        //    });

        //    m_rebindingOperation.OnCancel(_operation =>
        //    {
        //        _operation.Dispose();
        //        _actionToRebind.actionMap.Enable();
        //        OnRebindCanceled?.Invoke(); //Notify the UI to update itself, after the process is aborted.
        //    });

        //    m_rebindingOperation.Start();
        //} 
        #endregion

        #region Old DuplicateCheck
        ///// <summary>
        ///// Checks if a given binding path is already used by another action in the target map.
        ///// It intelligently ignores duplicates within the same composite action.
        ///// </summary>
        ///// <param name="_actionToRebind">The action that is currently being rebound.</param>
        ///// <param name="_bindingToRebind">The specific binding slot (part of the action) that is being changed.</param>
        ///// <param name="newPath">The new binding path to check for duplicates.</param>
        ///// <returns>True if a duplicate is found on a DIFFERENT action.</returns>
        //private bool CheckForDuplicateBinding(InputAction _actionToRebind, InputBinding _bindingToRebind, string _newPath)
        //{
        //    #region Old_Code
        //    //InputBinding newBinding = _actionToRebind.bindings[_bindingIndex];
        //    //if (string.IsNullOrEmpty(newBinding.effectivePath))
        //    //    return false;

        //    //if (_playerForContext != null)
        //    //{
        //    //    // 1. Prüfe gegen globale Aktionen
        //    //    foreach (var _action in m_mainActionAsset.actionMaps.SelectMany(map => map.actions))
        //    //    {
        //    //        if (_action.actionMap.id != _actionToRebind.actionMap.id)
        //    //            continue; // Nur gleiche Action Maps vergleichen
        //    //        if (_action.bindings.Any(originalBinding => originalBinding.effectivePath == newBinding.effectivePath))
        //    //        {
        //    //            Debug.LogWarning($"Duplicate found! Path '{newBinding.effectivePath}' is already used by global _action '{_action.name}'.");
        //    //            return true;
        //    //        }
        //    //    }

        //    //    // 2. Prüfe gegen ANDERE Spieler
        //    //    foreach (var player in UserInputManager.Instance.GetActivePlayers())
        //    //    {
        //    //        if (player._playerIndex == _playerForContext._playerIndex)
        //    //            continue; // Ignoriere den Spieler selbst

        //    //        foreach (var _action in player.actions)
        //    //        {
        //    //            if (_action.actionMap.id != _actionToRebind.actionMap.id)
        //    //                continue;
        //    //            if (_action.bindings.Any(originalBinding => originalBinding.effectivePath == newBinding.effectivePath))
        //    //            {
        //    //                Debug.LogWarning($"Duplicate found! Path '{newBinding.effectivePath}' is already used by Player {player._playerIndex} for _action '{_action.name}'.");
        //    //                return true;
        //    //            }
        //    //        }
        //    //    }
        //    //}
        //    //// --- KONTEXT: WIR REBINDEN EINE GLOBALE AKTION ---
        //    //else
        //    //{
        //    //    // 1. Prüfe gegen alle Spieler-Aktionen
        //    //    foreach (var player in UserInputManager.Instance.GetActivePlayers())
        //    //    {
        //    //        foreach (var _action in player.actions)
        //    //        {
        //    //            if (_action.actionMap.id != _actionToRebind.actionMap.id)
        //    //                continue;
        //    //            if (_action.bindings.Any(originalBinding => originalBinding.effectivePath == newBinding.effectivePath))
        //    //            {
        //    //                Debug.LogWarning($"Duplicate found! Path '{newBinding.effectivePath}' is already used by Player {player._playerIndex} for _action '{_action.name}'.");
        //    //                return true;
        //    //            }
        //    //        }
        //    //    }
        //    //}

        //    //// WICHTIG: Prüfe zuletzt auf Duplikate innerhalb der EIGENEN Action-Liste (z.B. MoveLeft vs. MoveRight)
        //    //foreach (var _action in _actionToRebind.actionMap.actions)
        //    //{
        //    //    for (int i = 0; i < _action.bindings.Count; i++)
        //    //    {
        //    //        // Ignoriere die exakte Bindung, die wir gerade ändern
        //    //        if (_action.id == _actionToRebind.id && i == _bindingIndex)
        //    //            continue;

        //    //        if (_action.bindings[i].effectivePath == newBinding.effectivePath)
        //    //        {
        //    //            Debug.LogWarning($"Duplicate found! Path '{newBinding.effectivePath}' is already used by _action '{_action.name}' in the same list.");
        //    //            return true;
        //    //        }
        //    //    }
        //    //}

        //    //return false; // Kein Duplikat gefunden
        //    #endregion

        //    InputActionMap gameplayMap = m_mainActionAsset.FindActionMap(m_targetActionMap);

        //    foreach (var action in gameplayMap.actions)
        //    {
        //        foreach (var binding in action.bindings)
        //        {
        //            //Skip the exact originalBinding we are currently modifying, to allow rebinding to the same key it already has.
        //            if (binding.id == _bindingToRebind.id)
        //                continue;

        //            //Check for a path match.
        //            if (binding.effectivePath == _newPath)
        //            {
        //                //A duplicate is found, if it's on a different _action.
        //                if (action.id != _actionToRebind.id)
        //                {
        //                    Debug.Log($"Duplicate binding found. Path '{_newPath}' is already used by action '{action.name}'.");
        //                    return true;
        //                }

        //                //Allow duplicate is on the SAME _action. (Example: in composites.)
        //            }
        //        }
        //    }
        //    return false; //No conflicts found on other actions.
        //}
        #endregion

        /// <summary>
        /// Finds a originalBinding that uses a specific path, excluding the originalBinding that is currently being rebound.
        /// </summary>
        /// <param name="_bindingToExclude">The specific originalBinding slot that is being changed and should be ignored in the search.</param>
        /// <param name="_pathToCheck">The new originalBinding path to check for duplicates.</param>
        /// <returns>The conflicting InputBinding if a duplicate is found; otherwise, null.</returns>
        private InputBinding? FindDuplicateBinding(InputBinding _bindingToExclude, string _pathToCheck)
        {
            InputActionMap gameplayMap = m_mainActionAsset.FindActionMap(m_targetActionMap);

            foreach (var action in gameplayMap.actions)
            {
                foreach (var binding in action.bindings)
                {
                    //Skip the exact originalBinding we are currently modifying.
                    if (binding.id == _bindingToExclude.id)
                        continue;

                    //Check if the effective path matches the one we're trying to set.
                    if (binding.effectivePath == _pathToCheck)
                        return binding; //Found a originalBinding on some _action using the same path.
                }
            }
            return null; //No duplicate found.
        }

        #region Helper-Methods_for_dictionary_population.
        /// <summary>
        /// Gets the appropriate display string for a originalBinding, considering any overrides.
        /// </summary>
        public string GetBindingDisplayString(string _actionName, int _bindingIndex, int _playerIndex)
        {
            //Find a specific player override first.
            var playerOverride = m_overrideData.bindingOverrides.FirstOrDefault(x =>
                x.playerIndex == _playerIndex &&
                x.actionName == _actionName &&
                x.bindingIndex == _bindingIndex);

            if (playerOverride != null)
                return InputControlPath.ToHumanReadableString(playerOverride.overridePath);

            //If no player override is found, find a global override.
            var globalOverride = m_overrideData.bindingOverrides.FirstOrDefault(x =>
                x.playerIndex == -1 &&
                x.actionName == _actionName &&
                x.bindingIndex == _bindingIndex);

            if (globalOverride != null)
                return InputControlPath.ToHumanReadableString(globalOverride.overridePath);

            //If no override is found, return the default originalBinding from the asset.
            InputAction action = m_mainActionAsset.FindAction(_actionName);
            return action.GetBindingDisplayString(_bindingIndex);
        }
        #endregion

        private void SaveBindingOverrides()
        {
            //Simply save the entire data object. The system handles the rest.
            m_saveSystem.Save(m_overrideData);
        }

        public void LoadAllBindings()
        {
            //Load the data. This is guaranteed to return a valid object.
            m_overrideData = m_saveSystem.Load();

            //After loading, apply all overrides to the base asset.
            foreach (var overrideData in m_overrideData.bindingOverrides)
            {
                var action = m_mainActionAsset.FindAction(overrideData.actionName);
                action?.ApplyBindingOverride(overrideData.bindingIndex, overrideData.overridePath);
            }
        }

        /// <summary>
        /// Resets a specific originalBinding to its default value from the InputActionAsset.
        /// </summary>
        public void ResetSpecificBinding(string actionName, int bindingIndex, int playerIndex)
        {
            var overrideToRemove = m_overrideData.bindingOverrides.FirstOrDefault(x =>
                x.playerIndex == playerIndex &&
                x.actionName == actionName &&
                x.bindingIndex == bindingIndex);

            if (overrideToRemove != null)
            {
                //Remove override from the custom live list.
                m_overrideData.bindingOverrides.Remove(overrideToRemove);

                //Remove the actual override from the _action.
                InputAction action = m_mainActionAsset.FindAction(actionName);
                action?.RemoveBindingOverride(bindingIndex);

                //Save the changes.
                SaveBindingOverrides();

                //Notify UI to update.
                OnRebindComplete?.Invoke();
            }
        }

        /// <summary>
        /// Resets all bindings for all players back to their defaults.
        /// </summary>
        public void ResetAllBindings()
        {
            m_mainActionAsset.RemoveAllBindingOverrides();
            m_overrideData.bindingOverrides.Clear();
            SaveBindingOverrides();
            OnRebindComplete?.Invoke();
        }
        #endregion
    }
}