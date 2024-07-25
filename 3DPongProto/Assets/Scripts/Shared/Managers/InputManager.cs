using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using TMPro;
using ThreeDeePongProto.Offline.UI;
using UnityEngine.UI;
using ThreeDeePongProto.Offline.Settings;

public enum EButtonControlScheme
{
    KeyboardMouse,
    Gamepad,
    PSGamepad,
    XBoxGamepad
}

namespace ThreeDeePongProto.Shared.InputActions
{
    public class InputManager : MonoBehaviour
    {
        //Reference to the PlayerAction-InputAsset.
        public static PlayerInputActions m_playerInputActions;

        #region Change Action Maps
        //ActionEvent to switch between ActionMaps within the new InputActionAsset.
        public static event Action<InputActionMap> m_changeActiveActionMap;

        private int m_currentSceneIndex;
        private string m_currentSceneName;
        #endregion

        #region KeyRebinding
        public static event Action<string, Image> m_RebindComplete;
        public static event Action<string, Image> m_RebindCanceled;
        public static event Action<InputAction, int> m_rebindStarted;

        private const string m_cancelWithKeyboardButton = "<Keyboard>/escape";
        private const string m_cancelWithGamepadButton = "<Gamepad>/buttonEast";

        #region KeyBinding Icons
        [SerializeField] public GamepadIcons m_pS;
        [SerializeField] public GamepadIcons m_xbox;
        private static event Func<EButtonControlScheme, string, Sprite> m_extractButtonImage;
        #endregion

        #region Serialization
        private static int m_playerSaveIndex;
        private static readonly string m_keyBindingOverrideFolderPath = "/SaveData/KeyBindingOverride";
        private static readonly string m_playerFileName = "/Player";
        private static readonly string m_fileFormat = ".json";

        private static IPersistentData m_persistentData = new SerializingData();
        private static bool m_encryptionEnabled = false;
        #endregion
        #endregion

        /// <summary>
        /// PlayerController and UIControls need to be moved into 'Start()' and the PlayerInputActions of the InputManager into 'Awake()', to prevent Exceptions.
        /// </summary>
        private void Awake()
        {
            //Alternative: m_playerInputActions ??= new PlayerInputActions();
            if (m_playerInputActions == null)
            {
                m_playerInputActions = new PlayerInputActions();
            }
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneFinishedLoading;
            m_extractButtonImage += ExtractImage;
            ControlSettings.PlayerViewIndex += PlayerIndex;
            //InputSystem.onDeviceChange += OnDeviceChange;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneFinishedLoading;
            m_extractButtonImage -= ExtractImage;
            ControlSettings.PlayerViewIndex -= PlayerIndex;
            //InputSystem.onDeviceChange -= OnDeviceChange;
        }

        private static void PlayerIndex(int _playerIndex)
        {
            m_playerSaveIndex = _playerIndex;
#if UNITY_EDITOR
            Debug.Log($"PlayerIndex {m_playerSaveIndex}");
#endif
        }

        #region Get and Return Sprites
        public static Sprite GetControllerIcons(EButtonControlScheme _controlScheme, string _controlPath)
        {
            Sprite buttonImage = m_extractButtonImage?.Invoke(_controlScheme, _controlPath);
            return buttonImage;
        }

        private Sprite ExtractImage(EButtonControlScheme _controlScheme, string _controlPath)
        {
            //Debug.Log($"Scheme: {_controlScheme} - Path: {_controlPath}"); //WORKED!
            switch (_controlScheme)
            {
                case EButtonControlScheme.KeyboardMouse:
                {
                    break;
                }
                case EButtonControlScheme.Gamepad:
                case EButtonControlScheme.PSGamepad:
                {
                    string wordBetween = GetWordInBetween(_controlPath, "<", ">");
#if UNITY_EDITOR
                    //Debug.Log($"Start-{wordBetween}-End");
#endif
                    switch (wordBetween)
                    {
                        case "Gamepad":
                        {
                            Sprite buttonImage = m_pS.GetGamepadSprite(_controlPath);
                            return buttonImage;
                        }
                        case "DualShockGamepad":
                        {
                            Sprite buttonImage = m_pS.GetDualShockGamepadSprite(_controlPath);
                            return buttonImage;

                        }
                        case "DualSenseGamepadHID":
                        {
                            Sprite buttonImage = m_pS.GetDualSenseGamepadHIDSprite(_controlPath);
                            return buttonImage;
                        }
                    }
                    break;
                }
                case EButtonControlScheme.XBoxGamepad:
                {
                    Sprite buttonImage = m_xbox.GetGamepadSprite(_controlPath);
                    return buttonImage;
                }
                default:
                {
                    Sprite buttonImage = m_pS.GetGamepadSprite(_controlPath);
                    return buttonImage;
                }
            }

            return null;
        }

        public static string GetWordInBetween(string _source, string _firstArg, string _secondArg)
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

        #region KeyRebinding
        public static void StartRebindProcess(string _actionName, int _bindingIndex, TextMeshProUGUI _statusText, bool _excludeMouse, EButtonControlScheme _ebuttonControlScheme, Image _targetImage)
        {
            //Look up the action name of the inputAction in the generated C#-Script, not the Scriptable Object.
            InputAction inputAction = m_playerInputActions.asset.FindAction(_actionName);

            //Check for null references and valid indices.
            if (inputAction == null || inputAction.bindings.Count <= _bindingIndex)
            {
#if UNITY_EDITOR
                Debug.LogError("InputManager: Could not find action or binding.");
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
                    ExecuteKeyRebind(inputAction, firstParentSubIndex, _statusText, true, _excludeMouse, _ebuttonControlScheme, _targetImage);
                }
            }
            else
                ExecuteKeyRebind(inputAction, _bindingIndex, _statusText, false, _excludeMouse, _ebuttonControlScheme, _targetImage);
        }

        /// <summary>
        /// Execusion of the Key-Rebindings with the submitted parameters, related to ActionMap and Binding-Indices.
        /// </summary>
        /// <param name="_actionToRebind"></param>
        /// <param name="_bindingIndex"></param>
        /// <param name="_statusText"></param>
        /// <param name="_allCompositeParts"></param>
        /// <param name="_excludeMouse"></param>
        private static void ExecuteKeyRebind(InputAction _actionToRebind, int _bindingIndex, TextMeshProUGUI _statusText, bool _allCompositeParts, bool _excludeMouse, EButtonControlScheme _ebuttonControlScheme, Image _targetImage)
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
                        ExecuteKeyRebind(_actionToRebind, nextBindingIndex, _statusText, _allCompositeParts, _excludeMouse, _ebuttonControlScheme, _targetImage);
                }

                SaveKeyBindingOverride(_actionToRebind);    //TODO: replace this with the save system interface.
#if UNITY_EDITOR
                //Debug.Log($"effectivePath: {_actionToRebind.bindings[_bindingIndex].effectivePath}");
#endif
                m_RebindComplete?.Invoke(_actionToRebind.bindings[_bindingIndex].effectivePath, _targetImage); //Invoke on finished rebinding.
            });

            //assignment of the OnCancel'operation' delegate.
            rebind.OnCancel(operation =>
            {
                _actionToRebind.Enable();
                operation.Dispose();    //Releases memory held by the operation to prevent memory leaks.

                m_RebindCanceled?.Invoke(_actionToRebind.bindings[_bindingIndex].effectivePath, _targetImage); //Invoke on canceled rebinding.
            });

            switch (_ebuttonControlScheme)  //ONLY ONE cancelButton gets recognized in code at a time.
            {
                case EButtonControlScheme.KeyboardMouse:
                    rebind.WithCancelingThrough(m_cancelWithKeyboardButton);
                    break;
                case EButtonControlScheme.Gamepad:
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
            #endregion

            m_rebindStarted?.Invoke(_actionToRebind, _bindingIndex);

            rebind.Start(); //Real Start of the rebind process.
        }

        /// <summary>
        /// Returns button infos according to the submitted Parameter.
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

        /// <summary>
        /// Each binding requires getting called by an own 'Remove-Override-Binding-Extension-Function'.
        /// </summary>
        /// <param name="_actionName"></param>
        /// <param name="_bindingIndex"></param>
        public static void ResetRebinding(string _actionName, int _bindingIndex)
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

            SaveKeyBindingOverride(inputAction);
        }
        #endregion

        #region Save / Load Binding Override
        private static void SaveKeyBindingOverride(InputAction _inputAction)
        {
            for (int i = 0; i < _inputAction.bindings.Count; i++)
            {
                //Possible input system paths: 'path', 'effectivePath' and 'overridePath' during Runtime in the 'InputAction-Asset'.
                PlayerPrefs.SetString(_inputAction.actionMap + _inputAction.name + i, _inputAction.bindings[i].overridePath);
            }
        }

        internal static void LoadKeyBindingOverride(string _actionName)
        {
            if (m_playerInputActions == null)
                m_playerInputActions = new PlayerInputActions();

            InputAction inputAction = m_playerInputActions.asset.FindAction(_actionName);

            for (int i = 0; i < inputAction.bindings.Count; i++)
            {
                if (!string.IsNullOrEmpty(PlayerPrefs.GetString(inputAction.actionMap + inputAction.name + i)))
                {
                    inputAction.ApplyBindingOverride(i, PlayerPrefs.GetString(inputAction.actionMap + inputAction.name + i));
                    //return inputAction.bindings[i]/*.overridePath*/;  //InputBinding
                }
            }
            //return default;
        }
        #endregion
    }
}