using System;
using System.Collections.Generic;
using ThreeDeePongProto.Offline.Settings;
using ThreeDeePongProto.Offline.UI;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace ThreeDeePongProto.Shared.InputActions
{
    public class InputManager : MonoBehaviour
    {
        public static PlayerInputActions m_playerInputActions;  //Reference to the PlayerAction-InputAsset.
#if UNITY_EDITOR
        [SerializeField] private bool m_deleteAllPlayerPrefs = false;
#endif
        [SerializeField] private bool m_loadRebindDicts = true;

        #region Change Action Maps
        //ActionEvent to switch between ActionMaps within the new InputActionAsset.
        public static event Action<InputActionMap> m_changeActiveActionMap;

        private int m_currentSceneIndex;
        private string m_currentSceneName;
        #endregion

        #region KeyRebinding
        public static event Action m_RebindComplete;
        public static event Action m_RebindCanceled;
        public static event Action<InputAction, int> m_rebindStarted;

        private static Dictionary<string, string> m_keyboardRebindDict = new Dictionary<string, string>();
        private static Dictionary<string, string> m_gamepadRebindDict = new Dictionary<string, string>();

        private const string m_cancelWithKeyboardButton = "<Keyboard>/escape";
        private const string m_cancelWithGamepadButton = "<Gamepad>/select";

        private const string m_keyboardMouseScheme = "KeyboardMouse";   //Inputsystem's KeyboardMouse scheme. (groups)
        private const string m_gamePadScheme = "Gamepad";               //Inputsystem's Gamepad scheme. (groups)
        private const string m_keyboardPath = "Keyboard";               //EffectivePath string.
        private const string m_gamepadPath = "Gamepad";                 //EffectivePath string.
        #endregion

        #region KeyBinding Icons
        [SerializeField] public GamepadIcons m_pS;
        [SerializeField] public GamepadIcons m_xbox;
        private static event Func<string, string, Sprite> m_extractButtonImage;
        #endregion

        #region Serialization
        private static int m_playerIndex;
        private static readonly string m_keyBindingOverrideFolderPath = "/SaveData/KeyReBinds";
        private static readonly string m_keyboardMapFileName = "/Keyboard/Player";
        private static readonly string m_gamepadMapFileName = "/Gamepad/Player";
        private static readonly string m_fileFormat = ".json";

        private static readonly IPersistentData m_persistentData = new SerializingData();
        private static readonly bool m_encryptionEnabled = false;
        #endregion

        /// <summary>
        /// PlayerController and UIControls need to be moved into 'Start()' and the PlayerInputActions of the InputManager into 'Awake()', to prevent Exceptions.
        /// </summary>
        private void Awake()
        {
            m_playerIndex = 0;
            m_keyboardRebindDict.Clear();
            m_gamepadRebindDict.Clear();

#if UNITY_EDITOR
            if (m_deleteAllPlayerPrefs)
            {
                PlayerPrefs.DeleteAll();
                Debug.Log("All PlayerPrefs deleted!");
            }
#endif

            //Alternative: m_playerInputActions ??= new PlayerInputActions();
            if (m_playerInputActions == null)
            {
                m_playerInputActions = new PlayerInputActions();
            }
        }

        private void OnEnable()
        {
            if (m_loadRebindDicts)
            {
                m_keyboardRebindDict = m_persistentData.LoadData<Dictionary<string, string>>(m_keyBindingOverrideFolderPath, m_keyboardMapFileName + $"{m_playerIndex}", m_fileFormat, m_encryptionEnabled);
                m_gamepadRebindDict = m_persistentData.LoadData<Dictionary<string, string>>(m_keyBindingOverrideFolderPath, m_gamepadMapFileName + $"{m_playerIndex}", m_fileFormat, m_encryptionEnabled);
            }

            SceneManager.sceneLoaded += OnSceneFinishedLoading;
            m_extractButtonImage += ExtractImage;
            ControlSettings.PlayerViewIndex += PlayerIndex;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneFinishedLoading;
            m_extractButtonImage -= ExtractImage;
            ControlSettings.PlayerViewIndex -= PlayerIndex;
        }

        private static void PlayerIndex(int _playerIndex)
        {
            m_playerIndex = _playerIndex;
#if UNITY_EDITOR
            //            Debug.Log($"PlayerIndex {m_playerIndex}");
#endif
        }

        #region Change Action Maps
        private void OnSceneFinishedLoading(Scene _scene, LoadSceneMode _mode)
        {
            m_currentSceneIndex = _scene.buildIndex; //TODO: Change to SceneIndex after development.
            m_currentSceneName = _scene.name;

            switch (m_currentSceneName)
            {
                case "StartMenuScene":
                    ToggleActionMaps(m_playerInputActions.UI);
                    break;
                case "LocalGameScene":
                {
                    ToggleActionMaps(m_playerInputActions.PlayerActions);
                    break;
                }
                case "LanGameScene":
                {
                    ToggleActionMaps(m_playerInputActions.PlayerActions);
                    break;
                }
                case "NetGameScene":
                {
                    ToggleActionMaps(m_playerInputActions.PlayerActions);
                    break;
                }
                case "WinScene":
                    ToggleActionMaps(m_playerInputActions.UI);
                    break;
                default:
                    ToggleActionMaps(m_playerInputActions.UI);
                    break;
            }
        }

        /// <summary>
        /// Switches ActionMaps, if the active actionMap isn't equal to the submitted one. But does not disable the old actionMaps!
        /// </summary>
        /// <param name="_actionMap"></param>
        public static void ToggleActionMaps(InputActionMap _actionMap)
        {
            //if you try to change to the same ActionMap skip the rest.
            if (_actionMap.enabled)
                return;

            //else disable the current ActionMap to switch to the next.
            m_playerInputActions.Disable();
            m_changeActiveActionMap?.Invoke(_actionMap);
            _actionMap.Enable();
        }
        #endregion

        #region Get and Return Sprites
        public static Sprite GetControllerIcons(string _controlScheme, string _controlPath)
        {
            if (string.IsNullOrEmpty($"{_controlScheme}") || string.IsNullOrEmpty(_controlPath))
                return null;

            Sprite buttonImage = m_extractButtonImage?.Invoke(_controlScheme, _controlPath);
            return buttonImage;
        }

        private Sprite ExtractImage(string _controlScheme, string _controlPath)
        {
            Sprite buttonImage = default;

            switch (_controlScheme)
            {
                case m_keyboardMouseScheme:
                {
                    break;
                }
                case m_gamePadScheme:
                {
                    string wordBetween = GetWordBetweenArgs(_controlPath, "<", ">");
#if UNITY_EDITOR
                    //Debug.Log($"Start-{wordBetween}-End");
#endif
                    switch (wordBetween)
                    {
                        case "Gamepad":
                        {
                            buttonImage = m_pS.GetGamepadSprite(_controlPath);
                            return buttonImage;
                        }
                        case "DualShockGamepad":
                        {
                            buttonImage = m_pS.GetDualShockGamepadSprite(_controlPath);
                            return buttonImage;

                        }
                        case "DualSenseGamepadHID":
                        {
                            buttonImage = m_pS.GetDualSenseGamepadHIDSprite(_controlPath);
                            return buttonImage;
                        }
                    }
                    break;
                }
                //XBox case currently not in use.
                //case EKeyControlScheme.XBoxGamepad:
                //{
                //    buttonImage = m_xbox.GetGamepadSprite(_controlPath);
                //    return buttonImage;
                //}
                default:
                {
                    buttonImage = m_pS.GetGamepadSprite(_controlPath);
                    return buttonImage;
                }
            }

            return null;
        }
        #endregion

        #region Helper Method(s)
        public static string GetWordBetweenArgs(string _source, string _firstArg, string _secondArg)
        {
            if (_source.Contains(_firstArg) && _source.Contains(_secondArg))
            {
                int start = _source.IndexOf(_firstArg, 0) + _firstArg.Length;
                int end = _source.IndexOf(_secondArg, start);
                return _source.Substring(start, end - start);
            }

            return "";
        }
#if UNITY_EDITOR
        public static string ToUpperFirstCharacter(string _source)
        {
            if (string.IsNullOrEmpty(_source))
                return _source;

            char[] letters = _source.ToCharArray();
            letters[0] = char.ToUpper(letters[0]);
            return new string(letters);
        }
#endif
        #endregion

        #region KeyRebinding
        public static void StartRebindProcess(string _actionName, int _bindingIndex, TextMeshProUGUI _statusText, string _controlScheme, bool _excludeMouse, Guid _uniqueGuid)
        {
            //Look up the action name of the inputAction in the generated C#-Script, not the Scriptable Object.
            InputAction inputAction = m_playerInputActions.asset.FindAction(_actionName);

            //Check for null references and valid indices.
            if (inputAction == null || inputAction.bindings.Count <= _bindingIndex)
            {
#if UNITY_EDITOR
                Debug.LogError("InputManager could not find action or binding.");
#endif
                return;
            }

            //isComposite: Corresponds to WASD's Actions Properties. (WASD itself.)
            if (inputAction.bindings[_bindingIndex].isComposite)
            {
                var firstChildIndex = _bindingIndex + 1;    //Examples: First entry after WASD.

                //isPartOfComposite: Corresponds to WASD's W for example. (Childs/Binding Properties of the Composite.)
                if (firstChildIndex < inputAction.bindings.Count && inputAction.bindings[firstChildIndex].isPartOfComposite)
                {
                    //true == _allCompositeParts: true.
                    ExecuteKeyRebind(inputAction, firstChildIndex, _statusText, _controlScheme, _excludeMouse, true, _uniqueGuid);
                }
            }
            else
                //false == _allCompositeParts: false.
                ExecuteKeyRebind(inputAction, _bindingIndex, _statusText, _controlScheme, _excludeMouse, false, _uniqueGuid);
        }

        /// <summary>
        /// Execusion of the Key-Rebindings with the submitted parameters, related to ActionMap and Binding-Indices.
        /// </summary>
        /// <param name="_actionToRebind"></param>
        /// <param name="_bindingIndex"></param>
        /// <param name="_statusText"></param>
        /// <param name="_allCompositeParts"></param>
        /// <param name="_excludeMouse"></param>
        private static void ExecuteKeyRebind(InputAction _actionToRebind, int _bindingIndex, TextMeshProUGUI _statusText, string _controlScheme, bool _excludeMouse, bool _allCompositeParts, Guid _uniqueGuid)
        {
            if (_actionToRebind == null || _bindingIndex < 0)
                return;

            _statusText.text = $"Press a Button"; //old: _statusText.text = $"Press a {_actionToRebind.expectedControlType}";

            _actionToRebind.Disable();  //Required while rebinding!

            //Instance creation for the Rebind-Action-Process.
            InputActionRebindingExtensions.RebindingOperation rebind = _actionToRebind.PerformInteractiveRebinding(_bindingIndex);
#if UNITY_EDITOR
            //Debug.Log($"ExpectedControlType: {_actionToRebind.expectedControlType}");
#endif

            #region Rebind Operation OnComplete (Including Keyboard & Gamepad Override Saves.)
            //assignment of the OnComplete'operation' delegate.
            rebind.OnComplete(operation =>
            {
                _actionToRebind.Enable();
                operation.Dispose();    //Releases memory held by the operation to prevent memory leaks.

                #region .isComposite/.isPartOfComposite switch pre DuplicateBindingCheck
                switch (_allCompositeParts)
                {
                    case true:  //For all Composite for future projects.
                    {
                        if (DuplicateBindingCheck(_actionToRebind, _bindingIndex, _statusText, _controlScheme, _excludeMouse, _allCompositeParts))
                        {
                            //Duplicate case.
                            _actionToRebind.RemoveBindingOverride(_bindingIndex);   //Required, or the new effectivePath gets displayed still.

                            //Gives the Player the option to retry and rebind another buttonKey with '_bindingIndex'. Or to press Escape.
                            if (_bindingIndex < _actionToRebind.bindings.Count && _actionToRebind.bindings[_bindingIndex].isPartOfComposite)
                                ExecuteKeyRebind(_actionToRebind, _bindingIndex, _statusText, _controlScheme, _excludeMouse, _allCompositeParts, _uniqueGuid);
                        }
                        else
                        {
                            //No Duplicate case. Each new buttonPress rebinds the next Composite Part/Button in line with 'nextBindingIndex'.
                            var nextBindingIndex = _bindingIndex + 1;

                            if (nextBindingIndex < _actionToRebind.bindings.Count && _actionToRebind.bindings[nextBindingIndex].isPartOfComposite)
                            {
                                ExecuteKeyRebind(_actionToRebind, nextBindingIndex, _statusText, _controlScheme, _excludeMouse, _allCompositeParts, _uniqueGuid);
                            }
                        }
                        //Composite Rebinding completed from here.
                        break;
                    }
                    case false: //For all Rebinds in this project.
                    {
                        if (DuplicateBindingCheck(_actionToRebind, _bindingIndex, _statusText, _controlScheme, _excludeMouse))
                        {
                            //if DuplicateCheck true. Canceling rebind right away.
                            _actionToRebind.RemoveBindingOverride(_bindingIndex);   //Required, or the new effectivePath gets displayed still.
                            rebind.Cancel();
                            return;
                        }
                        break;
                    }
                }
                #endregion

                m_RebindComplete?.Invoke(); //Subscribers update to new state.

                #region Rebind Save
                switch (_controlScheme)
                {
                    case m_keyboardMouseScheme:
                        SaveKeyboardOverrides(_actionToRebind, _bindingIndex, _uniqueGuid);
                        break;
                    case m_gamePadScheme:
                        SaveGamepadOverrides(_actionToRebind, _bindingIndex, _uniqueGuid);
                        break;
                    case "":
                    {
                        SaveSchemelessComposites(_actionToRebind, _bindingIndex, _uniqueGuid);
                        break;
                    }
                    default:
                        break;
                }
                #endregion
                #endregion
            });

            #region Rebind Operation OnCancel (Including '.WithCancelingThrough' Button-Switch.)
            //assignment of the OnCancel'operation' delegate.
            rebind.OnCancel(operation =>
            {
                _actionToRebind.Enable();
                operation.Dispose();    //Releases memory held by the operation to prevent memory leaks.

                m_RebindCanceled?.Invoke();
            });

            switch (_controlScheme)     //ONLY ONE cancelButton gets recognized in code at a time.
            {
                case m_keyboardMouseScheme:
                    rebind.WithCancelingThrough(m_cancelWithKeyboardButton);
                    break;
                case m_gamePadScheme:
                {
                    rebind.WithCancelingThrough(m_cancelWithGamepadButton);
                    break;
                }
                default:
                    break;
            }
            #endregion

            #region Exclude controls from the rebind process
            rebind.WithControlsExcluding("<Keyboard>/escape");  //Else ESC gets set on keyboard button rebinds on cancellation.
            if (_excludeMouse)  //Ignore the mouse on redinds.
                rebind.WithControlsExcluding("Mouse");
            rebind.WithControlsExcluding("<DualSenseGamepadHID>/systemButton");
            rebind.WithControlsExcluding("<DualSenseGamepadHID>/micButton");
            rebind.WithControlsExcluding("<Gamepad>/start");    //We don't want to completely f... all of our rebinds. Trust me. <(o.O)".
            #endregion

            m_rebindStarted?.Invoke(_actionToRebind, _bindingIndex);

            rebind.Start(); //Real Start of the rebind process.
        }

        private static bool DuplicateBindingCheck(InputAction _actionToRebind, int _bindingIndex, TextMeshProUGUI _statusText, string _controlScheme, bool _excludeMouse, bool _allCompositeParts = false, Guid _uniqueGuid = default)
        {
            InputBinding newBinding = _actionToRebind.bindings[_bindingIndex];

            foreach (InputBinding binding in _actionToRebind.actionMap.bindings)
            {
                #region Compare ActionBindings in the actionMap.
                if (!binding.isComposite)                                                   //Excludes Composites.
                {
#if UNITY_EDITOR
                    #region Flag Debug Logs
                    //if (!binding.isPartOfComposite)
                    //    Debug.Log($"'None' flag {binding.effectivePath} in {binding.action}.");
                    //if (binding.isPartOfComposite)
                    //    Debug.Log($"'isPartOfComposite' flag {binding.effectivePath} in {binding.action}.");
                    #endregion
#endif
                    if (binding.action == newBinding.action && !_allCompositeParts)         //If actions are the same.
                    {
                        if (binding.id == newBinding.id)                                    //Act by binding ID.
                        {
#if UNITY_EDITOR
                            Debug.Log("Same Binding. Skipped Duplicate-Check.");            //Skips itself on same ID. (Can set binding.)
#endif
                            continue;                                                       //And continues.
                        }

                        for (int i = 0; i < binding.action.Length; i++)                     //Old 'if(binding.id != newBinding.id)'.
                        {
                            if (binding.effectivePath == newBinding.effectivePath)
                            {
#if UNITY_EDITOR
                                string bindingName = ToUpperFirstCharacter(binding.name);
                                Debug.Log($"Duplicate binding {newBinding.effectivePath} found in own Composite Part {bindingName}. Canceling rebind.");
#endif
                                return true;                                                //Call out a duplicate, if one if found.
                            }
                        }
                    }

                    if (binding.action != newBinding.action && !_allCompositeParts)         //If actions are different.
                    {
                        for (int j = 0; j < _actionToRebind.actionMap.bindings.Count; j++)
                        {
                            if (binding.effectivePath == newBinding.effectivePath)          //Compare paths on (different) actions & IDs.
                            {
                                switch (binding.isPartOfComposite)
                                {
                                    case true:
                                    {
#if UNITY_EDITOR
                                        string bindingName = ToUpperFirstCharacter(binding.name);
                                        Debug.Log($"Duplicate binding {newBinding.effectivePath} found in {binding.action}, Composite Part {bindingName}. Canceling rebind.");
#endif
                                        return true;                                        //Call out a duplicate, if one if found.
                                    }
                                    case false:
                                    {
#if UNITY_EDITOR
                                        Debug.Log($"Duplicate binding {newBinding.effectivePath} found in {binding.action}. Canceling rebind.");
#endif
                                        return true;                                        //Call out a duplicate, if one if found.
                                    }
                                }
                            }
                        }
                    }
                }
                #endregion
            }

            #region Composite Internal Duplicate Check
            if (_allCompositeParts) //Duplicate Check inside the Composite.
            {
                for (int i = 1; i < _bindingIndex; ++i)
                {
                    if (_actionToRebind.bindings[i].effectivePath == newBinding.effectivePath)
                    {
#if UNITY_EDITOR
                        Debug.Log($"Duplicate binding {newBinding.effectivePath} found. Canceling rebind.");
#endif
                        return true;                                                        //Call out a duplicate, if one if found.
                    }
                }
            }
            #endregion

            return false;                                                                   //Exiting search w/o a discovery.
        }

        /// <summary>
        /// Each binding requires getting called by an own 'Remove-Override-Binding-Extension-Function'.
        /// </summary>
        /// <param name="_actionName"></param>
        /// <param name="_bindingIndex"></param>
        public static void ResetRebinding(string _actionName, int _bindingIndex, string _controlScheme, Guid _uniqueGuid)
        {
            InputAction inputAction = m_playerInputActions.asset.FindAction(_actionName);

            if (inputAction == null && inputAction.bindings.Count <= _bindingIndex)
            {
#if UNITY_EDITOR
                Debug.Log("InputManager: The Action or Binding could not be found!");
#endif
                return;
            }

            if (inputAction.bindings[_bindingIndex].isComposite)
            {
                var firstChildIndex = _bindingIndex + 1;    //Without adding + 1 to start with here, the reset did not work.
                for (int i = firstChildIndex; i < inputAction.bindings.Count && inputAction.bindings[i].isPartOfComposite; i++)
                {
                    inputAction.RemoveBindingOverride(i);
                }
            }
            else
                inputAction.RemoveBindingOverride(_bindingIndex);

            #region Reset Rebind Save
            if (_controlScheme == m_keyboardMouseScheme)
                ResetKeyboardOverrides(inputAction, /*_bindingIndex,*/ _uniqueGuid);    //Kept as example. Guid replaced the Index.

            if (_controlScheme == m_gamePadScheme)
                ResetGamepadOverrides(inputAction, /*_bindingIndex,*/ _uniqueGuid);    //Kept as example. Guid replaced the Index.
            #endregion
        }
        #endregion

        #region Save / Load Binding Override
        /// <summary>
        /// Returns the BindingName to update the TextGameObject of the RebindButtons.
        /// </summary>
        /// <param name="_actionName"></param>
        /// <param name="_bindingIndex"></param>
        /// <returns></returns>
        public static string GetBindingName(string _actionName, int _bindingIndex)
        {
            if (m_playerInputActions == null)
                m_playerInputActions = new PlayerInputActions();

            InputAction inputAction = m_playerInputActions.asset.FindAction(_actionName);
            return inputAction.GetBindingDisplayString(_bindingIndex);
        }

        public static string GetEffectiveBindingPath(string _actionName, int _bindingIndex)
        {
            if (m_playerInputActions == null)
                m_playerInputActions = new PlayerInputActions();

            InputAction inputAction = m_playerInputActions.asset.FindAction(_actionName);
            return inputAction.bindings[_bindingIndex].effectivePath;
        }

        private static void SaveKeyboardOverrides(InputAction _inputAction, int _bindingIndex, Guid _uniqueGuid/* = default*/)
        {
            for (int i = 0; i < _inputAction.bindings.Count; i++)
            {
                #region PlayerPref Example
                //PlayerPrefs.SetString(_inputAction.actionMap + _inputAction.name + i, _inputAction.bindings[i].overridePath);
                #endregion

                #region Unique Guid
                if (_inputAction.bindings[i].overridePath != null && _inputAction.bindings[i].groups == m_keyboardMouseScheme)
                {
                    //string dictKey = $"{_uniqueGuid}";
                    bool dictHasKey = m_keyboardRebindDict.ContainsKey($"{_uniqueGuid}");

                    //Only check and save, if iteration index equals inputAction index. Else more than the target index gets saved.
                    if (i == _bindingIndex)
                    {
                        switch (dictHasKey)
                        {
                            case true:
                            {
                                m_keyboardRebindDict[$"{_uniqueGuid}"] = $"]{_bindingIndex}.{_inputAction.bindings[i].effectivePath}!";
                                break;
                            }
                            case false:
                            {
                                m_keyboardRebindDict.Add($"{_uniqueGuid}", $"]{_bindingIndex}.{_inputAction.bindings[i].effectivePath}!");
                                break;
                            }
                        }
                    }
                }
                else
                {
                    Debug.Log($"Guid from the yet unsupported Composite: {_uniqueGuid} with effectivePath: {_inputAction.bindings[i].effectivePath}");
                }
                #endregion
            }

            m_persistentData.SaveData(m_keyBindingOverrideFolderPath, m_keyboardMapFileName + $"{m_playerIndex}", m_fileFormat, m_keyboardRebindDict, m_encryptionEnabled, true);
        }

        private static void SaveGamepadOverrides(InputAction _inputAction, int _bindingIndex, Guid _uniqueGuid/* = default*/)
        {
            for (int i = 0; i < _inputAction.bindings.Count; i++)
            {
                #region PlayerPref Example
                //PlayerPrefs.SetString(_inputAction.actionMap + _inputAction.name + i, _inputAction.bindings[i].overridePath);
                #endregion

                #region Unique Guid
                if (_inputAction.bindings[i].overridePath != null && _inputAction.bindings[i].groups == m_gamePadScheme)
                {
                    //string dictKey = $"{_uniqueGuid}";
                    bool dictHasKey = m_gamepadRebindDict.ContainsKey($"{_uniqueGuid}");

                    //Only check and save, if iteration index equals inputAction index. Else more than the target index gets saved.
                    if (i == _bindingIndex)
                    {
                        switch (dictHasKey)
                        {
                            case true:
                            {
                                m_gamepadRebindDict[$"{_uniqueGuid}"] = $"]{_bindingIndex}.{_inputAction.bindings[i].effectivePath}!";
                                break;
                            }
                            case false:
                            {
                                m_gamepadRebindDict.Add($"{_uniqueGuid}", $"]{_bindingIndex}.{_inputAction.bindings[i].effectivePath}!");
                                break;
                            }
                        }
                    }
                }
                #endregion
            }

            m_persistentData.SaveData(m_keyBindingOverrideFolderPath, m_gamepadMapFileName + $"{m_playerIndex}", m_fileFormat, m_gamepadRebindDict, m_encryptionEnabled, true);
        }

        private static void SaveSchemelessComposites(InputAction _inputAction, int _bindingIndex, Guid _uniqueGuid/* = default*/)
        {
            var deviceType = GetWordBetweenArgs(_inputAction.bindings[_bindingIndex].effectivePath, "<", ">");
            switch (deviceType)
            {
                case m_keyboardPath:
                {
                    break;
                }
                case m_gamepadPath:
                {
                    break;
                }
                default:
                    break;
            }
        }

        internal static void LoadKeyboardOverrides(string _actionName, Guid _uiGuid)
        {
            if (m_playerInputActions == null)
            {
                //m_playerInputActions = new PlayerInputActions();
#if UNITY_EDITOR
                Debug.Log("Keyboard-InputAction is null");
#endif
            }

            InputAction inputAction = m_playerInputActions.asset.FindAction(_actionName);

            #region PlayerPref Example
            //for (int i = 0; i < inputAction.bindings.Count; i++)
            //{
            //    if (!string.IsNullOrEmpty(PlayerPrefs.GetString(inputAction.actionMap + inputAction.name + i)))
            //    {
            //        inputAction.ApplyBindingOverride(i, PlayerPrefs.GetString(inputAction.actionMap + inputAction.name + i));
            //    }
            //}
            #endregion

            #region Unique Guid
            Guid uniqueGuid;
            string guidKey,/* actionMap,*/ bindingIndex, overridePath;

            foreach (var entry in m_keyboardRebindDict)
            {
                guidKey = entry.Key;
                uniqueGuid = Guid.Parse(guidKey);
                //actionMap = GetWordBetweenArgs(entry.Value, "[", "]");
                bindingIndex = GetWordBetweenArgs(entry.Value, "]", ".");
                int bindingIndexAsInt = int.Parse(bindingIndex);
                overridePath = GetWordBetweenArgs(entry.Value, ".", "!");

                switch (uniqueGuid == _uiGuid)
                {
                    case true:
                    {
                        inputAction.ApplyBindingOverride(bindingIndexAsInt, overridePath);
                        break;
                    }
                    case false:
                    {
                        continue;
                    }
                }
            }
            #endregion
        }

        internal static void LoadGamepadOverrides(string _actionName, Guid _uiGuid)
        {
            if (m_playerInputActions == null)
            {
                m_playerInputActions = new PlayerInputActions();
            }

            InputAction inputAction = m_playerInputActions.asset.FindAction(_actionName);

            #region PlayerPref Example
            //for (int i = 0; i < inputAction.bindings.Count; i++)
            //{
            //    if (!string.IsNullOrEmpty(PlayerPrefs.GetString(inputAction.actionMap + inputAction.name + i)))
            //    {
            //        inputAction.ApplyBindingOverride(i, PlayerPrefs.GetString(inputAction.actionMap + inputAction.name + i));
            //    }
            //}
            #endregion

            #region Unique Guid
            Guid uniqueGuid;
            string guidKey,/* actionMap,*/ bindingIndex, overridePath;

            foreach (var entry in m_gamepadRebindDict)
            {
                guidKey = entry.Key;
                uniqueGuid = Guid.Parse(guidKey);
                //actionMap = GetWordBetweenArgs(entry.Value, "[", "]");
                bindingIndex = GetWordBetweenArgs(entry.Value, "]", ".");
                int bindingIndexAsInt = int.Parse(bindingIndex);
                overridePath = GetWordBetweenArgs(entry.Value, ".", "!");

                switch (uniqueGuid == _uiGuid)
                {
                    case true:
                    {
                        inputAction.ApplyBindingOverride(bindingIndexAsInt, overridePath);
                        break;
                    }
                    case false:
                    {
                        continue;
                    }
                }
            }
            #endregion
        }

        private static void ResetKeyboardOverrides(InputAction _inputAction, /*int _bindingIndex,*/ Guid _uniqueGuid/* = default*/)
        {
            for (int i = 0; i < _inputAction.bindings.Count; i++)
            {
                #region Unique Guid
                //string dictKey = $"{_uniqueGuid}";
                bool dictHasKey = m_keyboardRebindDict.ContainsKey($"{_uniqueGuid}");

                //If the dictionary has the guidKey, save the entry. Else create a new entry with the new informations. 
                switch (dictHasKey)
                {
                    case true:
                    {
                        m_keyboardRebindDict.Remove($"{_uniqueGuid}");
                        break;
                    }
                    case false:
                    {
                        break;
                    }
                }

                m_persistentData.SaveData(m_keyBindingOverrideFolderPath, m_keyboardMapFileName + $"{m_playerIndex}", m_fileFormat, m_keyboardRebindDict, m_encryptionEnabled, true);
                #endregion
            }
        }

        private static void ResetGamepadOverrides(InputAction _inputAction, /*int _bindingIndex,*/ Guid _uniqueGuid/* = default*/)
        {
            for (int i = 0; i < _inputAction.bindings.Count; i++)
            {
                #region Unique Guid
                //string dictKey = $"{_uniqueGuid}";
                bool dictHasKey = m_gamepadRebindDict.ContainsKey($"{_uniqueGuid}");

                //If the dictionary has the guidKey, save the entry. Else create a new entry with the new informations. 
                switch (dictHasKey)
                {
                    case true:
                    {
                        m_gamepadRebindDict.Remove($"{_uniqueGuid}");
                        break;
                    }
                    case false:
                    {
                        break;
                    }
                }

                m_persistentData.SaveData(m_keyBindingOverrideFolderPath, m_gamepadMapFileName + $"{m_playerIndex}", m_fileFormat, m_gamepadRebindDict, m_encryptionEnabled, true);
                #endregion
            }
        }
        #endregion
    }
}