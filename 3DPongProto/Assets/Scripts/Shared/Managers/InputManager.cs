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
        public static PlayerInputActions m_PlayerInputActions;  //Reference to the PlayerAction-InputAsset.
#if UNITY_EDITOR
        [SerializeField] private bool m_deleteAllPlayerPrefs = false;
#endif
        [SerializeField] private bool m_loadRebindDicts = true;

        #region Change Action Maps
        //ActionEvent to switch between ActionMaps within the InputActionAsset.
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

        private const string m_keyboardMouseScheme = "KeyboardMouse";           //Inputsystem's KeyboardMouse scheme. (groups)
        private const string m_gamePadScheme = "Gamepad";                       //Inputsystem's Gamepad scheme. (groups)

        private const string m_keyboardPath = "Keyboard";                       //EffectivePath string.
        private const string m_gamepadPath = "Gamepad";                         //EffectivePath string.
        private const string m_dualShockGamepadPath = "DualShockGamepad";       //EffectivePath string.
        private const string m_dualSenseGamepadHIDPath = "DualSenseGamepadHID"; //EffectivePath string.
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

            //Alternative: m_PlayerInputActions ??= new PlayerInputActions();
            if (m_PlayerInputActions == null)
            {
                m_PlayerInputActions = new PlayerInputActions();
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
                    ToggleActionMaps(m_PlayerInputActions.UI);
                    break;
                case "LocalGameScene":
                {
                    ToggleActionMaps(m_PlayerInputActions.PlayerActions);
                    break;
                }
                case "LanGameScene":
                {
                    ToggleActionMaps(m_PlayerInputActions.PlayerActions);
                    break;
                }
                case "NetGameScene":
                {
                    ToggleActionMaps(m_PlayerInputActions.PlayerActions);
                    break;
                }
                case "WinScene":
                    ToggleActionMaps(m_PlayerInputActions.UI);
                    break;
                default:
                    ToggleActionMaps(m_PlayerInputActions.UI);
                    break;
            }
        }

        /// <summary>
        /// Switches ActionMaps, if the active actionMap isn't equal to the submitted one. But does not disable the old actionMaps!
        /// </summary>
        /// <param name="_actionMap"></param>
        internal static void ToggleActionMaps(InputActionMap _actionMap)
        {
            //if you try to change to the same ActionMap skip the rest.
            if (_actionMap.enabled)
                return;

            //else disable the current ActionMap to switch to the next.
            m_PlayerInputActions.Disable();
            m_changeActiveActionMap?.Invoke(_actionMap);
            _actionMap.Enable();
        }
        #endregion

        #region Get and Return Sprites
        internal static Sprite GetControllerIcons(string _controlScheme, string _controlPath)
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
                    string devicePath = GetWordBetweenArgs(_controlPath, "<", ">");
#if UNITY_EDITOR
                    //Debug.Log($"Start-{devicePath}-End");
#endif
                    switch (devicePath)
                    {
                        case m_gamepadPath:
                        {
                            buttonImage = m_pS.GetGamepadSprite(_controlPath);
                            return buttonImage;
                        }
                        case m_dualShockGamepadPath:
                        {
                            buttonImage = m_pS.GetDualShockGamepadSprite(_controlPath);
                            return buttonImage;

                        }
                        case m_dualSenseGamepadHIDPath:
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
                    break;
            }

            return null;
        }
        #endregion

        #region Helper Method(s)
        internal static string GetWordBetweenArgs(string _source, string _firstArg, string _secondArg)
        {
            if (_source.Contains(_firstArg) && _source.Contains(_secondArg))
            {
                int start = _source.IndexOf(_firstArg, 0) + _firstArg.Length;
                int end = _source.IndexOf(_secondArg, start);
                return _source.Substring(start, end - start);
            }

            return "";
        }

        internal static string ToUpperFirstCharacter(string _source)
        {
            if (string.IsNullOrEmpty(_source))
                return _source;

            char[] letters = _source.ToCharArray();
            letters[0] = char.ToUpper(letters[0]);
            return new string(letters);
        }
        #endregion

        #region KeyRebinding
        internal static void StartRebindProcess(string _actionName, int _bindingIndex, TextMeshProUGUI _statusText, string _controlScheme, bool _excludeMouse)
        {
            //Look up the action name of the inputAction in the generated C#-Script, not the Scriptable Object.
            InputAction inputAction = m_PlayerInputActions.asset.FindAction(_actionName);

            //Check for null references and valid indices.
            if (inputAction == null || inputAction.bindings.Count <= _bindingIndex)
            {
#if UNITY_EDITOR
                Debug.LogError("InputManager could not find action or binding.");
#endif
                return;
            }

            //isComposite: WASD's Actions Properties. (WASD itself.)
            if (inputAction.bindings[_bindingIndex].isComposite)
            {
                var firstChildIndex = _bindingIndex + 1;    //Examples: First entry after WASD.

                //isPartOfComposite: Corresponds to WASD's W for example. (Child's/Binding Properties of the Composite.)
                if (firstChildIndex < inputAction.bindings.Count && inputAction.bindings[firstChildIndex].isPartOfComposite)
                {
                    //true == _allCompositeParts: true.
                    ExecuteKeyRebind(inputAction, firstChildIndex, _statusText, _controlScheme, _excludeMouse, true);
                }
            }
            else
                //false == _allCompositeParts: false.
                ExecuteKeyRebind(inputAction, _bindingIndex, _statusText, _controlScheme, _excludeMouse, false);
        }

        /// <summary>
        /// Execusion of the Key-Rebindings with the submitted parameters, related to ActionMap and Binding-Indices.
        /// </summary>
        /// <param name="_actionToRebind"></param>
        /// <param name="_bindingIndex"></param>
        /// <param name="_statusText"></param>
        /// <param name="_allCompositeParts"></param>
        /// <param name="_excludeMouse"></param>
        private static void ExecuteKeyRebind(InputAction _actionToRebind, int _bindingIndex, TextMeshProUGUI _statusText, string _controlScheme, bool _excludeMouse, bool _allCompositeParts)
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
                        if (DuplicateBindingCheck(_actionToRebind, _bindingIndex, _statusText, _controlScheme, _excludeMouse))
                        {
                            //Duplicate case.
                            _actionToRebind.RemoveBindingOverride(_bindingIndex);   //Required, or the new effectivePath gets displayed still.

                            //Gives the Player the option to retry and rebind another buttonKey with '_bindingIndex'. Or to press Escape.
                            if (_bindingIndex < _actionToRebind.bindings.Count && _actionToRebind.bindings[_bindingIndex].isPartOfComposite)
                                ExecuteKeyRebind(_actionToRebind, _bindingIndex, _statusText, _controlScheme, _excludeMouse, _allCompositeParts);
                        }
                        else
                        {
                            //No Duplicate case. Each new buttonPress rebinds the next Composite Part/Button in line with 'nextBindingIndex'.
                            var nextBindingIndex = _bindingIndex + 1;

                            if (nextBindingIndex < _actionToRebind.bindings.Count && _actionToRebind.bindings[nextBindingIndex].isPartOfComposite)
                            {
                                ExecuteKeyRebind(_actionToRebind, nextBindingIndex, _statusText, _controlScheme, _excludeMouse, _allCompositeParts);
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
                        SaveKeyboardIndexOverrides(_actionToRebind, _bindingIndex);
                        break;
                    case m_gamePadScheme:
                        SaveGamepadIndexOverrides(_actionToRebind, _bindingIndex);
                        break;
                    case "":    //Empty string. Does not accept 'string.Empty'.
                    {
                        SaveCompositeOverrides(_actionToRebind, _bindingIndex);
                        break;
                    }
                    default:
#if UNITY_EDITOR
                        Debug.Log($"Saving this deviceType is still unsupported, yet. Please ask your purrrsonal Progger. <(~.^)'");
#endif
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

        private static bool DuplicateBindingCheck(InputAction _actionToRebind, int _bindingIndex, TextMeshProUGUI _statusText, string _controlScheme, bool _excludeMouse)
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
                    if (binding.action == newBinding.action)                                //If actions are the same.
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

                    if (binding.action != newBinding.action/* && !_allCompositeParts*/)     //If actions are different.
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

            return false;                                                                   //Exiting search w/o a discovery.
        }

        /// <summary>
        /// Each binding requires getting called by an own 'Remove-Override-Binding-Extension-Function'.
        /// </summary>
        /// <param name="_actionName"></param>
        /// <param name="_bindingIndex"></param>
        internal static void ResetRebinding(string _actionName, int _bindingIndex, string _controlScheme)
        {
            InputAction inputAction = m_PlayerInputActions.asset.FindAction(_actionName);

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
            //Sending Guid as method arguments got replaced by getting each Guid with '_inputAction.bindings[_bindingIndex].id;'.
            switch (_controlScheme)
            {
                case m_keyboardMouseScheme:
                    ResetKeyboardIndexOverrides(inputAction, _bindingIndex);
                    break;
                case m_gamePadScheme:
                    ResetGamepadIndexOverrides(inputAction, _bindingIndex);
                    break;
                case "":    //Empty string. Does not accept 'string.Empty'.
                {
                    ResetCompositeOverrides(inputAction);
                    break;
                }
                default:
#if UNITY_EDITOR
                    Debug.Log($"Resetting this deviceType is still unsupported, yet. Please ask your purrrsonal Progger (again). <(~.^)'");
#endif
                    break;
            }
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
        internal static string GetBindingName(string _actionName, int _bindingIndex)
        {
            if (m_PlayerInputActions == null)
                m_PlayerInputActions = new PlayerInputActions();

            InputAction inputAction = m_PlayerInputActions.asset.FindAction(_actionName);
            return inputAction.GetBindingDisplayString(_bindingIndex);
        }

        /// <summary>
        /// Return the effective bindingPath, so each Gamepad button (or Keyboard button, if desired) sets it's icon properly.
        /// </summary>
        /// <param name="_actionName"></param>
        /// <param name="_bindingIndex"></param>
        /// <returns></returns>
        public static string GetEffectiveBindingPath(string _actionName, int _bindingIndex)
        {
            if (m_PlayerInputActions == null)
                m_PlayerInputActions = new PlayerInputActions();

            InputAction inputAction = m_PlayerInputActions.asset.FindAction(_actionName);
            return inputAction.bindings[_bindingIndex].effectivePath;
        }

        /// <summary>
        /// Saves Keyboard overrides from individual buttons, that can be allocated to the Keyboard(Mouse) control scheme.
        /// </summary>
        /// <param name="_inputAction"></param>
        /// <param name="_bindingIndex"></param>
        private static void SaveKeyboardIndexOverrides(InputAction _inputAction, int _bindingIndex)
        {
            Guid keyIndexGuid = _inputAction.bindings[_bindingIndex].id;
            bool dictHasKey = m_keyboardRebindDict.ContainsKey($"{keyIndexGuid}");

            #region PlayerPref Example
            //for (int i = 0; i < _inputAction.bindings.Count; i++)
            //{
            //PlayerPrefs.SetString(_inputAction.actionMap + _inputAction.name + i, _inputAction.bindings[i].overridePath);
            //}
            #endregion

            #region Save data with unique Guid as Dict Key
            if (_inputAction.bindings[_bindingIndex].groups == m_keyboardMouseScheme && !_inputAction.bindings[_bindingIndex].isComposite)
            {
                switch (dictHasKey)
                {
                    case true:
                    {
                        m_keyboardRebindDict[$"{keyIndexGuid}"] = $"]{_bindingIndex}.{_inputAction.bindings[_bindingIndex].effectivePath}!";
                        break;
                    }
                    case false:
                    {
                        m_keyboardRebindDict.Add($"{keyIndexGuid}", $"]{_bindingIndex}.{_inputAction.bindings[_bindingIndex].effectivePath}!");
                        break;
                    }
                }
            }
            #endregion

            m_persistentData.SaveData(m_keyBindingOverrideFolderPath, m_keyboardMapFileName + $"{m_playerIndex}", m_fileFormat, m_keyboardRebindDict, m_encryptionEnabled, true);
        }

        /// <summary>
        /// Saves Gamepad overrides from individual buttons, that can be allocated to the Gamepad control scheme.
        /// </summary>
        /// <param name="_inputAction"></param>
        /// <param name="_bindingIndex"></param>
        private static void SaveGamepadIndexOverrides(InputAction _inputAction, int _bindingIndex)
        {
            Guid buttonIndexGuid = _inputAction.bindings[_bindingIndex].id;
            bool dictHasKey = m_gamepadRebindDict.ContainsKey($"{buttonIndexGuid}");

            #region PlayerPref Example
            //for (int i = 0; i < _inputAction.bindings.Count; i++)
            //{
            //PlayerPrefs.SetString(_inputAction.actionMap + _inputAction.name + i, _inputAction.bindings[i].overridePath);
            //}
            #endregion

            #region Save data with unique Guid as Dict Key
            if (_inputAction.bindings[_bindingIndex].groups == m_gamePadScheme && !_inputAction.bindings[_bindingIndex].isComposite)
            {
                switch (dictHasKey)
                {
                    case true:
                    {
                        m_gamepadRebindDict[$"{buttonIndexGuid}"] = $"]{_bindingIndex}.{_inputAction.bindings[_bindingIndex].effectivePath}!";
                        break;
                    }
                    case false:
                    {
                        m_gamepadRebindDict.Add($"{buttonIndexGuid}", $"]{_bindingIndex}.{_inputAction.bindings[_bindingIndex].effectivePath}!");
                        break;
                    }
                }
            }
            #endregion

            m_persistentData.SaveData(m_keyBindingOverrideFolderPath, m_gamepadMapFileName + $"{m_playerIndex}", m_fileFormat, m_gamepadRebindDict, m_encryptionEnabled, true);
        }

        /// <summary>
        /// Gets the deviceType from each '(Composite)Index.effectivePath' to save properly.
        /// </summary>
        /// <param name="_inputAction"></param>
        /// <param name="_bindingIndex"></param>
        private static void SaveCompositeOverrides(InputAction _inputAction, int _bindingIndex)
        {
            string deviceType = GetWordBetweenArgs(_inputAction.bindings[_bindingIndex].effectivePath, "<", ">");
            Guid indexGuid = _inputAction.bindings[_bindingIndex].id;

            switch (deviceType)
            {
                case m_keyboardPath:
                {
                    bool dictHasKey = m_keyboardRebindDict.ContainsKey($"{indexGuid}");

                    #region Save Keyboard data with unique Guid as Dict Key
                    switch (dictHasKey)
                    {
                        case true:
                        {
                            m_keyboardRebindDict[$"{indexGuid}"] = $"]{_bindingIndex}.{_inputAction.bindings[_bindingIndex].effectivePath}!";
                            break;
                        }
                        case false:
                        {
                            m_keyboardRebindDict.Add($"{indexGuid}", $"]{_bindingIndex}.{_inputAction.bindings[_bindingIndex].effectivePath}!");
                            break;
                        }
                    }
                    #endregion

                    m_persistentData.SaveData(m_keyBindingOverrideFolderPath, m_keyboardMapFileName + $"{m_playerIndex}", m_fileFormat, m_keyboardRebindDict, m_encryptionEnabled, true);
                    break;
                }
                case m_gamepadPath:
                case m_dualShockGamepadPath:
                case m_dualSenseGamepadHIDPath:
                {
                    bool dictHasKey = m_gamepadRebindDict.ContainsKey($"{indexGuid}");

                    #region Save Gamepad data with unique Guid as Dict Key
                    switch (dictHasKey)
                    {
                        case true:
                        {
                            m_gamepadRebindDict[$"{indexGuid}"] = $"]{_bindingIndex}.{_inputAction.bindings[_bindingIndex].effectivePath}!";
                            break;
                        }
                        case false:
                        {
                            m_gamepadRebindDict.Add($"{indexGuid}", $"]{_bindingIndex}.{_inputAction.bindings[_bindingIndex].effectivePath}!");
                            break;
                        }
                    }
                    #endregion

                    m_persistentData.SaveData(m_keyBindingOverrideFolderPath, m_gamepadMapFileName + $"{m_playerIndex}", m_fileFormat, m_gamepadRebindDict, m_encryptionEnabled, true);
                    break;
                }
                default:
                {
#if UNITY_EDITOR
                    Debug.LogWarning($"Save structure for {deviceType} is not set, yet. Didn't you ask alrrready? <(o.O)'");
#endif
                    break;
                }
            }
        }

        internal static void LoadKeyRebindOverrides(string _actionName, int _bindingIndex)
        {
            if (m_PlayerInputActions == null)
            {
                m_PlayerInputActions = new PlayerInputActions();
            }

            InputAction inputAction = m_PlayerInputActions.asset.FindAction(_actionName);

            #region PlayerPref Example
            //for (int i = 0; i < inputAction.bindings.Count; i++)
            //{
            //    if (!string.IsNullOrEmpty(PlayerPrefs.GetString(inputAction.actionMap + inputAction.name + i)))
            //    {
            //        inputAction.ApplyBindingOverride(i, PlayerPrefs.GetString(inputAction.actionMap + inputAction.name + i));
            //    }
            //}
            #endregion

            Guid uniqueGuid;    //The key in the dictionary.
            int bindingIndex;
            string /* actionMap,*/ overridePath;

            switch (inputAction.bindings[_bindingIndex].groups)
            {
                #region Using unique Guid to reapply the specific Keyboard Override
                case m_keyboardMouseScheme:
                {
                    foreach (var entry in m_keyboardRebindDict)
                    {
                        uniqueGuid = Guid.Parse(entry.Key);
                        //actionMap = GetWordBetweenArgs(entry.Value, "[", "]");
                        bindingIndex = int.Parse(GetWordBetweenArgs(entry.Value, "]", ".")); //Dict Value directly into int.
                        overridePath = GetWordBetweenArgs(entry.Value, ".", "!");

                        switch (uniqueGuid == inputAction.bindings[_bindingIndex].id)
                        {
                            case true:  //'inputAction.bindings[_bindingIndex].id' comes from each (.is)PartOfComposite directly.
                            {
                                inputAction.ApplyBindingOverride(bindingIndex, overridePath);
                                break;
                            }
                            case false: //Comparing 'inputAction.bindings[_bindingIndex].id' from the children, instead from the Composite parent.
                            {
                                foreach (InputBinding partOfComposite in inputAction.bindings)
                                {
                                    if (uniqueGuid == partOfComposite.id)
                                        inputAction.ApplyBindingOverride(bindingIndex, overridePath);
                                }
                                break;
                            }
                        }
                    }
                    break;
                }
                #endregion
                #region Using unique Guid to reapply the specific Gamepad Override
                case m_gamePadScheme:
                {
                    foreach (var entry in m_gamepadRebindDict)
                    {
                        uniqueGuid = Guid.Parse(entry.Key);
                        //actionMap = GetWordBetweenArgs(entry.Value, "[", "]");
                        bindingIndex = int.Parse(GetWordBetweenArgs(entry.Value, "]", ".")); //Dict Value directly into int.
                        overridePath = GetWordBetweenArgs(entry.Value, ".", "!");

                        switch (uniqueGuid == inputAction.bindings[_bindingIndex].id)
                        {
                            case true:  //'inputAction.bindings[_bindingIndex].id' comes from each (.is)PartOfComposite directly.
                            {
                                inputAction.ApplyBindingOverride(bindingIndex, overridePath);
                                break;
                            }
                            case false: //Comparing 'inputAction.bindings[_bindingIndex].id' from the children, instead from the Composite parent.
                            {
                                foreach (InputBinding partOfComposite in inputAction.bindings)
                                {
                                    if (uniqueGuid == partOfComposite.id)
                                        inputAction.ApplyBindingOverride(bindingIndex, overridePath);
                                }
                                break;
                            }
                        }
                    }
                    break;
                }
                #endregion
            }
        }

        /// <summary>
        /// Reset Keyboard Overrides by the buttonIndex, if the dictionary contains the Guid key.
        /// </summary>
        /// <param name="_inputAction"></param>
        /// <param name="_bindingIndex"></param>
        private static void ResetKeyboardIndexOverrides(InputAction _inputAction, int _bindingIndex)
        {
            Guid buttonIndexGuid = _inputAction.bindings[_bindingIndex].id;

            for (int i = 0; i < _inputAction.bindings.Count; i++)
            {
                #region Unique Guid to remove the specific Override
                bool dictHasKey = m_keyboardRebindDict.ContainsKey($"{buttonIndexGuid}");

                #region Using unique Guid to remove the specific Keyboard Override
                //If the dictionary has the guidKey, the entry gets removed.
                switch (dictHasKey)
                {
                    case true:
                    {
                        m_keyboardRebindDict.Remove($"{buttonIndexGuid}");
                        break;
                    }
                    case false:
                    {
                        break;
                    }
                } 
                #endregion

                m_persistentData.SaveData(m_keyBindingOverrideFolderPath, m_keyboardMapFileName + $"{m_playerIndex}", m_fileFormat, m_keyboardRebindDict, m_encryptionEnabled, true);
                #endregion
            }
        }

        /// <summary>
        /// Reset Gamepad Overrides by the buttonIndex, if the dictionary contains the Guid key.
        /// </summary>
        /// <param name="_inputAction"></param>
        /// <param name="_bindingIndex"></param>
        private static void ResetGamepadIndexOverrides(InputAction _inputAction, int _bindingIndex)
        {
            Guid buttonIndexGuid = _inputAction.bindings[_bindingIndex].id;

            for (int i = 0; i < _inputAction.bindings.Count; i++)
            {
                #region Unique Guid                
                bool dictHasKey = m_gamepadRebindDict.ContainsKey($"{buttonIndexGuid}");

                #region Using unique Guid to remove the specific Gamepad Override
                //If the dictionary has the guidKey, save the entry. Else create a new entry with the new informations. 
                switch (dictHasKey)
                {
                    case true:
                    {
                        m_gamepadRebindDict.Remove($"{buttonIndexGuid}");
                        break;
                    }
                    case false:
                    {
                        break;
                    }
                }
                #endregion

                m_persistentData.SaveData(m_keyBindingOverrideFolderPath, m_gamepadMapFileName + $"{m_playerIndex}", m_fileFormat, m_gamepadRebindDict, m_encryptionEnabled, true);
                #endregion
            }
        }

        /// <summary>
        /// Resets each Composite child. Control Schemes are still required to 'switch' between the corresponding device Type dicts.
        /// </summary>
        /// <param name="_inputAction"></param>
        private static void ResetCompositeOverrides(InputAction _inputAction)
        {
            string deviceScheme;
            Guid childIndexGuid;

            for (int i = 0; i < _inputAction.bindings.Count; i++)
            {
                if (!_inputAction.bindings[i].isComposite)              //'!.isComposite' excludes 'WASD' parent.
                {
                    deviceScheme = _inputAction.bindings[i].groups;     //Get the deviceScheme for each Index.
                    childIndexGuid = _inputAction.bindings[i].id;       //Get the child's Guid for each Index.

                    switch (deviceScheme)                               //Check and remove child's Guid "guided" by it's deviceScheme.
                    {
                        #region Using unique Guid to remove the specific Keyboard Override in Composites
                        case m_keyboardMouseScheme:
                        {
                            bool dictHasKey = m_keyboardRebindDict.ContainsKey($"{childIndexGuid}");

                            switch (dictHasKey)
                            {
                                case true:
                                {
                                    m_keyboardRebindDict.Remove($"{childIndexGuid}");
                                    break;
                                }
                                case false:
                                {
                                    break;
                                }
                            }

                            m_persistentData.SaveData(m_keyBindingOverrideFolderPath, m_keyboardMapFileName + $"{m_playerIndex}", m_fileFormat, m_keyboardRebindDict, m_encryptionEnabled, true);
                            break;
                        }
                        #endregion
                        #region Using unique Guid to remove the specific Gamepad Override in Composites
                        case m_gamePadScheme:
                        {
                            bool dictHasKey = m_gamepadRebindDict.ContainsKey($"{childIndexGuid}");

                            switch (dictHasKey)
                            {
                                case true:
                                {
                                    m_gamepadRebindDict.Remove($"{childIndexGuid}");
                                    break;
                                }
                                case false:
                                {
                                    break;
                                }
                            }

                            m_persistentData.SaveData(m_keyBindingOverrideFolderPath, m_gamepadMapFileName + $"{m_playerIndex}", m_fileFormat, m_gamepadRebindDict, m_encryptionEnabled, true);
                            break;
                        }
                        #endregion
                        default:
                            break;
                    }
                }
            }
        }
        #endregion
    }
}