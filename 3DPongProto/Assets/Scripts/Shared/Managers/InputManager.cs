using System;
using System.Collections.Generic;
using ThreeDeePongProto.Offline.Settings;
using ThreeDeePongProto.Offline.UI;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public enum EKeyControlScheme
{
    None,
    Keyboard,
    Gamepad,
    PSGamepad,
    XBoxGamepad
}

namespace ThreeDeePongProto.Shared.InputActions
{
    public class InputManager : MonoBehaviour
    {
        public static PlayerInputActions m_playerInputActions;  //Reference to the PlayerAction-InputAsset.
#if UNITY_EDITOR
        [SerializeField] private bool m_deleteAllPlayerPrefs = false;
#endif
        [SerializeField] private bool m_LoadRebindDicts = true;

        #region Change Action Maps
        //ActionEvent to switch between ActionMaps within the new InputActionAsset.
        public static event Action<InputActionMap> m_changeActiveActionMap;

        private int m_currentSceneIndex;
        private string m_currentSceneName;
        #endregion

        #region KeyRebinding
        public static event Action<string, Guid> m_RebindComplete;
        public static event Action<string, Guid> m_RebindCanceled;
        public static event Action<InputAction, int> m_rebindStarted;

        private static Dictionary<string, string> m_keyboardRebindDict = new Dictionary<string, string>();
        private static Dictionary<string, string> m_gamepadRebindDict = new Dictionary<string, string>();

        private const string m_cancelWithKeyboardButton = "<Keyboard>/escape";
        private const string m_cancelWithGamepadButton = "<Gamepad>/select";

        //private const string m_keyboardString = "Keyboard";
        //private const string m_gamePadString = "Gamepad";
        //private const string m_dualShockGamepadString = "DualShockGamepad";
        //private const string m_dualSenseGamepadHIDString = "DualSenseGamepadHID";
        #endregion

        #region KeyBinding Icons
        [SerializeField] public GamepadIcons m_pS;
        [SerializeField] public GamepadIcons m_xbox;
        private static event Func<EKeyControlScheme, string, Sprite> m_extractButtonImage;
        #endregion

        #region Serialization
        private static int m_playerIndex;
        private static readonly string m_keyBindingOverrideFolderPath = "/SaveData/KeyReBinds";
        private static readonly string m_keyboardMapFileName = "/Keyboard/Player";
        private static readonly string m_gamepadMapFileName = "/Gamepad/Player";
        private static readonly string m_fileFormat = ".json";

        private static IPersistentData m_persistentData = new SerializingData();
        private static bool m_encryptionEnabled = false;
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
            if (m_LoadRebindDicts)
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
                    ToggleActionMaps(m_playerInputActions.PlayerActions);
                    break;
                case "LanGameScene":
                    ToggleActionMaps(m_playerInputActions.PlayerActions);
                    break;
                case "NetGameScene":
                    ToggleActionMaps(m_playerInputActions.PlayerActions);
                    break;
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
#if UNITY_EDITOR
            //Debug.Log($"InputManager while debugging: {_actionMap.name} loaded.");
#endif
        }
        #endregion

        #region Get and Return Sprites
        public static Sprite GetControllerIcons(EKeyControlScheme _controlScheme, string _controlPath)
        {
            if (string.IsNullOrEmpty($"{_controlScheme}") || string.IsNullOrEmpty(_controlPath))
                return null;

            Sprite buttonImage = m_extractButtonImage?.Invoke(_controlScheme, _controlPath);
            return buttonImage;
        }

        private Sprite ExtractImage(EKeyControlScheme _controlScheme, string _controlPath)
        {
            Sprite buttonImage = default;

            switch (_controlScheme)
            {
                case EKeyControlScheme.Keyboard:
                {
                    break;
                }
                case EKeyControlScheme.Gamepad:
                case EKeyControlScheme.PSGamepad:
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
                case EKeyControlScheme.XBoxGamepad:
                {
                    buttonImage = m_xbox.GetGamepadSprite(_controlPath);
                    return buttonImage;
                }
                default:
                {
                    buttonImage = m_pS.GetGamepadSprite(_controlPath);
                    return buttonImage;
                }
            }

            return null;
        }

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
        #endregion

        #region KeyRebinding
        public static void StartRebindProcess(string _actionName, int _bindingIndex, TextMeshProUGUI _statusText, bool _excludeMouse, EKeyControlScheme _eKeyControlScheme, Guid _bindingId)
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
                var firstParentSubIndex = _bindingIndex + 1;    //Examples: First entry after WASD.

                //isPartOfComposite: Corresponds to WASD's W for example. (Childs/Binding Properties of the Composite.)
                if (firstParentSubIndex < inputAction.bindings.Count && inputAction.bindings[firstParentSubIndex].isPartOfComposite)
                {
                    ExecuteKeyRebind(inputAction, firstParentSubIndex, _statusText, true, _excludeMouse, _eKeyControlScheme, _bindingId);
                }
            }
            else
                ExecuteKeyRebind(inputAction, _bindingIndex, _statusText, false, _excludeMouse, _eKeyControlScheme, _bindingId);
        }

        /// <summary>
        /// Execusion of the Key-Rebindings with the submitted parameters, related to ActionMap and Binding-Indices.
        /// </summary>
        /// <param name="_actionToRebind"></param>
        /// <param name="_bindingIndex"></param>
        /// <param name="_statusText"></param>
        /// <param name="_allCompositeParts"></param>
        /// <param name="_excludeMouse"></param>
        private static void ExecuteKeyRebind(InputAction _actionToRebind, int _bindingIndex, TextMeshProUGUI _statusText, bool _allCompositeParts, bool _excludeMouse, EKeyControlScheme _eKeyControlScheme, Guid _bindingId)
        {
            if (_actionToRebind == null || _bindingIndex < 0)
                return;

            _statusText.text = $"Please press a Button"; //old: _statusText.text = $"Press a {_actionToRebind.expectedControlType}";

            _actionToRebind.Disable();  //Required while rebinding!

            //Instance creation for the Rebind-Action-Process.
            InputActionRebindingExtensions.RebindingOperation rebind = _actionToRebind.PerformInteractiveRebinding(_bindingIndex);
#if UNITY_EDITOR
            //Debug.Log($"ExpectedControlType: {_actionToRebind.expectedControlType}");
#endif

            //assignment of the OnComplete'operation' delegate.
            rebind.OnComplete(operation =>
            {
                _actionToRebind.Enable();
                operation.Dispose();    //Releases memory held by the operation to prevent memory leaks.

                if (_allCompositeParts) //If the Index has compositeParts/children.
                {
                    var nextBindingIndex = _bindingIndex + 1;

                    if (nextBindingIndex < _actionToRebind.bindings.Count && _actionToRebind.bindings[nextBindingIndex].isPartOfComposite)
                        ExecuteKeyRebind(_actionToRebind, nextBindingIndex, _statusText, _allCompositeParts, _excludeMouse, _eKeyControlScheme, _bindingId);
                }
#if UNITY_EDITOR
                //Debug.Log($"effectivePath: {_actionToRebind.bindings[_bindingIndex].effectivePath}");
#endif
                m_RebindComplete?.Invoke(_actionToRebind.bindings[_bindingIndex].effectivePath, _bindingId); //Invoke on finished rebinding.

                #region New Rebind Save
                switch (_eKeyControlScheme)
                {
                    case EKeyControlScheme.Keyboard:
                        SaveKeyboardOverrides(_actionToRebind, _bindingIndex, _bindingId);
                        break;
                    case EKeyControlScheme.Gamepad:
                    case EKeyControlScheme.PSGamepad:
                    case EKeyControlScheme.XBoxGamepad:
                        SaveGamepadOverrides(_actionToRebind, _bindingIndex, _bindingId);
                        break;
                    default:
                        break;
                }
                #endregion
            });

            //assignment of the OnCancel'operation' delegate.
            rebind.OnCancel(operation =>
            {
                _actionToRebind.Enable();
                operation.Dispose();    //Releases memory held by the operation to prevent memory leaks.

                m_RebindCanceled?.Invoke(_actionToRebind.bindings[_bindingIndex].effectivePath, _bindingId); //Invoke on canceled rebinding.
            });

            switch (_eKeyControlScheme)  //ONLY ONE cancelButton gets recognized in code at a time.
            {
                case EKeyControlScheme.Keyboard:
                    rebind.WithCancelingThrough(m_cancelWithKeyboardButton);
                    break;
                case EKeyControlScheme.Gamepad:
                {
                    rebind.WithCancelingThrough(m_cancelWithGamepadButton);
                    break;
                }
                default:
                    break;
            }

            #region Exclude controls from the rebind process
            rebind.WithControlsExcluding("<Keyboard>/escape");  //Else ESC gets set on keyboard button rebinds on cancellation.
            if (_excludeMouse)  //Ignore the mouse on redinds.
                rebind.WithControlsExcluding("Mouse");
            rebind.WithControlsExcluding("<DualSenseGamepadHID>/systemButton");
            rebind.WithControlsExcluding("<DualSenseGamepadHID>/micButton");
            rebind.WithControlsExcluding("<Gamepad>/start");    //We don't to completely f... all of our rebinds. Trust me. <(o.O)".
            #endregion

            m_rebindStarted?.Invoke(_actionToRebind, _bindingIndex);

            rebind.Start(); //Real Start of the rebind process.
        }

        /// <summary>
        /// Each binding requires getting called by an own 'Remove-Override-Binding-Extension-Function'.
        /// </summary>
        /// <param name="_actionName"></param>
        /// <param name="_bindingIndex"></param>
        public static void ResetRebinding(string _actionName, int _bindingIndex, EKeyControlScheme _eKeyControlScheme, Guid _uniqueGuid)
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
                for (int i = _bindingIndex; i < inputAction.bindings.Count && inputAction.bindings[i].isComposite; i++)
                    inputAction.RemoveBindingOverride(i);
            }
            else
                inputAction.RemoveBindingOverride(_bindingIndex);

            #region New Rebind Save
            if (_eKeyControlScheme == EKeyControlScheme.Keyboard)
                ResetKeyboardOverrides(inputAction, _bindingIndex, _uniqueGuid);

            if (_eKeyControlScheme == EKeyControlScheme.Gamepad || _eKeyControlScheme == EKeyControlScheme.PSGamepad || _eKeyControlScheme == EKeyControlScheme.XBoxGamepad)
                ResetGamepadOverrides(inputAction, _bindingIndex, _uniqueGuid);
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

        private static void SaveKeyboardOverrides(InputAction _inputAction, int _bindingIndex, Guid _uniqueGuid/* = default*/)
        {
            for (int i = 0; i < _inputAction.bindings.Count; i++)
            {
                #region PlayerPref Example
                //PlayerPrefs.SetString(_inputAction.actionMap + _inputAction.name + i, _inputAction.bindings[i].overridePath);
                #endregion

                #region Unique Guid
                if (_inputAction.bindings[i].overridePath != null)
                {
                    //string dictKey = $"{_uniqueGuid}";
                    bool dictHasKey = m_keyboardRebindDict.ContainsKey($"{_uniqueGuid}");

                    //If the dictionary has the guidKey, save the entry. Else create a new entry with the new informations. 
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

                    m_persistentData.SaveData(m_keyBindingOverrideFolderPath, m_keyboardMapFileName + $"{m_playerIndex}", m_fileFormat, m_keyboardRebindDict, m_encryptionEnabled, true);
                }
                #endregion
            }
        }

        private static void SaveGamepadOverrides(InputAction _inputAction, int _bindingIndex, Guid _uniqueGuid/* = default*/)
        {
            for (int i = 0; i < _inputAction.bindings.Count; i++)
            {
                #region PlayerPref Example
                //PlayerPrefs.SetString(_inputAction.actionMap + _inputAction.name + i, _inputAction.bindings[i].overridePath);
                #endregion

                #region Unique Guid
                if (_inputAction.bindings[i].overridePath != null)
                {
                    //string dictKey = $"{_uniqueGuid}";
                    bool dictHasKey = m_gamepadRebindDict.ContainsKey($"{_uniqueGuid}");

                    //If the dictionary has the guidKey, save the entry. Else create a new entry with the new informations. 
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

                    m_persistentData.SaveData(m_keyBindingOverrideFolderPath, m_gamepadMapFileName + $"{m_playerIndex}", m_fileFormat, m_gamepadRebindDict, m_encryptionEnabled, true);
                }
                #endregion
            }
        }

        internal static string LoadKeyboardOverrides(string _actionName, Guid _uiGuid)
        {
            if (m_playerInputActions == null)
            {
                //m_playerInputActions = new PlayerInputActions();
#if UNITY_EDITOR
                Debug.Log("Keyboard-InputAction is null");
#endif
                return null;
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
                        return overridePath;
                    }
                    case false:
                    {
                        continue;
                    }
                }
            }
            #endregion

            return null;
        }

        internal static string LoadGamepadOverrides(string _actionName, Guid _uiGuid)
        {
            if (m_playerInputActions == null)
            {
                //m_playerInputActions = new PlayerInputActions();
#if UNITY_EDITOR
                Debug.Log("Gamepad-InputAction is null");
#endif
                return null;
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
                        return overridePath;
                    }
                    case false:
                    {
                        continue;
                    }
                }
            }
            #endregion

            return null;
        }

        private static void ResetKeyboardOverrides(InputAction _inputAction, int _bindingIndex, Guid _uniqueGuid/* = default*/)
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

        private static void ResetGamepadOverrides(InputAction _inputAction, int _bindingIndex, Guid _uniqueGuid/* = default*/)
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