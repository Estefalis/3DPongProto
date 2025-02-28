using System;
using System.Collections;
using System.Collections.Generic;
using ThreeDeePongProto.Offline.UI.Menu;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.SceneManagement;

public enum ESceneNames
{
    StartMenu,
    LocalGame,
    LanGame,
    NetGame
}

namespace ThreeDeePongProto.Shared.Managers
{
    public class UserInputManager : MonoBehaviour
    {
        private enum ActiveInputActionMap
        {
            None,
            PlayerActions,
            UserInterface
        }

        public static UserInputManager Instance { get; private set; }

        [SerializeField] private PlayerInputManager m_playerInputManager;
        [SerializeField] private bool m_joinByDefault = true;

        //private PlayerInput m_menuInput;
        //private MenuManager m_menuManager;
        private Transform m_playfieldParent;

        private const string m_keyboardScheme = "KeyboardMouse", m_keyboardDevice = "Keyboard";
        private const string m_gamePadScheme = "Gamepad", m_gamepadDevice = "Gamepad";
        private const string m_uiActionMap = "UserInterface", m_playerActionMap = "PlayerActions";
        private string m_currentControlScheme;
        private static string m_lastActionMap;

        private InputDevice m_lastUsedDevice;

        private readonly List<InputDevice> m_usableGamepads = new();
        internal static event Action<int> ACheckForPlayerInput;
        internal static event Action<string, InputDevice[]> AOnDeviceInput;
        //internal static event Action<PlayerInputActions.PlayerActionsActions> ASetInitialSceneActionMap;
        private static ActiveInputActionMap m_newActiveActionMap = ActiveInputActionMap.None;
        public static event Action<string> AChangeActiveActionMap;

        #region Scriptable_Objects
        [SerializeField] private MatchUIStates m_matchUIStates;
        [SerializeField] private MatchValues m_matchValues;
        #endregion

        private void Awake()
        {
            #region Singleton_Pattern
            if (Instance == null)
            {
                Instance = this;
                //DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }
            #endregion

            m_playerInputManager = GetComponent<PlayerInputManager>();
            SetUpPlayerInputManager(m_playerInputManager);

            m_usableGamepads.Clear();
            for (int i = 0; i < Gamepad.all.Count; i++)
            {
                if (Gamepad.all[i] != null && !m_usableGamepads.Contains(Gamepad.all[i]))
                    m_usableGamepads.Add(Gamepad.all[i]);
            }
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneManagerLoaded;
            MenuManager.AReLoadScene += OnReLoadScene;
            //MenuManager.AResumeTheGame += ResetActionMap;
            //CharacterInputHandler.AOpenMenu += OnMenuOpens;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneManagerLoaded;
            MenuManager.AReLoadScene -= OnReLoadScene;
            //MenuManager.AResumeTheGame -= ResetActionMap;
            //CharacterInputHandler.AOpenMenu -= OnMenuOpens;
        }

        private void Update()
        {
            //KeyboardMouse-Input check.
            if (Keyboard.current.anyKey.wasPressedThisFrame || Mouse.current.delta.ReadValue() != Vector2.zero)
            {
                CustomControlSchemeSwitch(m_keyboardScheme, new InputDevice[] { Keyboard.current, Mouse.current });
            }
            //Gamepad-Input check.
            else if (Gamepad.current != null && GamepadPressed(out InputDevice gamepadDevice))
            {
                CustomControlSchemeSwitch(m_gamePadScheme, new InputDevice[] { gamepadDevice });
            }
        }

        #region PlayerInputManager-Configuration
        private void SetUpPlayerInputManager(PlayerInputManager _playerInputManager)
        {
            if (_playerInputManager == null)
            {
                PlayerInputManager playerInputManager = gameObject.AddComponent<PlayerInputManager>();
                m_playerInputManager = playerInputManager;
            }

            m_playerInputManager.notificationBehavior = PlayerNotifications.InvokeCSharpEvents;
            m_playerInputManager.joinBehavior = PlayerJoinBehavior.JoinPlayersManually;

            switch (m_joinByDefault)
            {
                case true:
                    m_playerInputManager.EnableJoining();
                    break;
                case false:
                    m_playerInputManager.DisableJoining();
                    break;
            }
        }
        #endregion

        #region Old_PlayerInput-Configuration_on_Menu
        private void SetInitialInput()
        {
            CustomControlSchemeSwitch(m_keyboardScheme, new InputDevice[] { Keyboard.current, Mouse.current });
        }
        #endregion

        #region Non_InputAction_Subscriptions
        /// <summary>
        /// Method to react after a scene has been fully loaded beforehand.
        /// </summary>
        /// <param name="scene"></param>
        /// <param name="mode"></param>
        private void OnSceneManagerLoaded(Scene scene, LoadSceneMode mode)
        {
            int sceneIndex = scene.buildIndex;

            switch (sceneIndex)
            {
                case 0:
                {
                    CleanupPlayers();
                    break;
                }
                case 1:
                {
                    m_playfieldParent = FindObjectOfType<LocalMatchManager>().m_PlayfieldParent;   //Public getter => private Transform.
                    InstantiatePlayer();
                    break;
                }
                case 2:
                case 3:
                {
                    Debug.Log("Lan- and NetGames still have to get thought about!");
                    break;
                }
            }

            SetInitialInput();
        }

        #region Change_Action_Maps
        /// <summary>
        /// Switches ActionMaps, if the active actionMap isn't equal to the submitted one. But does not disable the old actionMaps!
        /// </summary>
        /// <param name="_actionMap"></param>
        internal static void ToggleActionMaps(string _actionMap)
        {
            if (m_lastActionMap == _actionMap)
                return;

            m_newActiveActionMap = _actionMap == ActiveInputActionMap.PlayerActions.ToString()
                ? ActiveInputActionMap.PlayerActions : ActiveInputActionMap.UserInterface;

            AChangeActiveActionMap?.Invoke(_actionMap);
        }
        #endregion

        //private void OnMenuOpens(string _actionMap)
        //{
        //    ResetActionMap((int)ESceneNames.StartMenu); //Abusing the StartMenu enum entry to toggle UserInterface ActionMap! Am lazy. <(o.o)>
        //}

        /// <summary>
        /// Method to reSet the active ActionMap, depending on resuming to the game from the pauseMenu, or on scene reLoads.
        /// </summary>
        /// <param name="_index"></param>
        //private void ResetActionMap(int _index)
        //{
        //    //TODO: Move ToggleActionMaps method into UserInputManager!
        //    switch (_index)
        //    {
        //        case 0:
        //            RebindManager.ToggleActionMaps(RebindManager.m_PlayerInputActions.UserInterface);
        //            break;
        //        case 1:
        //        case 2:
        //        case 3:
        //            //RebindManager.ToggleActionMaps(RebindManager.m_PlayerInputActions.PlayerActions);
        //            break;
        //        default:
        //            Debug.Log("Scene is not implemented, yet!");
        //            break;
        //    }
        //}

        /// <summary>
        /// Method to receive the buildIndex of the scene that shall be reloaded.
        /// </summary>
        /// <param name="_sceneIndex"></param>
        private void OnReLoadScene(int _sceneIndex)
        {
            switch (_sceneIndex)
            {
                case 0:
                    SceneManager.LoadScene((int)ESceneNames.StartMenu);
                    break;
                case 1:
                    SceneManager.LoadScene((int)ESceneNames.LocalGame);
                    break;
                case 2:
                {
                    Debug.Log("When the LanGame mode is finished... .");
                    //SceneManager.LoadScene((int)ESceneNames.LanGame);
                }
                break;
                case 3:
                {
                    Debug.Log("When the NetGame mode is finished... .");
                    //SceneManager.LoadScene((int)ESceneNames.NetGame);
                }
                break;
                default:
                    Debug.Log("Scene is not implemented, yet!");
                    break;
            }
        }
        #endregion

        #region Instantiate_and_Configurate_Player_PlayerInput
        private void InstantiatePlayer()
        {
            uint playerCount = (uint)m_matchUIStates.EPlayerAmount;

            for (int np = 0; np < playerCount; np++)
            {
                InputDevice[] assignedDevices = GetDevicesForPlayer(np);
                string setControlScheme = assignedDevices[0] is Gamepad ? m_gamePadScheme : m_keyboardScheme;

                PlayerInput newPlayerInput = PlayerInput.Instantiate(m_matchValues.PlayerSOData[np].Prefab, controlScheme: setControlScheme, pairWithDevice: assignedDevices[0]);

                GameObject newPlayer = newPlayerInput.gameObject;
                newPlayer.transform.SetParent(m_playfieldParent);
                //Debug.Log($"PlayerID {np} assigned device(s): {string.Join(", ", assignedDevices.Select(d => d.name))}");

                //Waits a frame until old _playerInput component is destroyed.
                StartCoroutine(DelayedPlayerInputSetup(newPlayer, np, assignedDevices));
            }
        }

        #region Setup-Delay
        private IEnumerator DelayedPlayerInputSetup(GameObject _playerPrefab, int _playerIndex, InputDevice[] _assignedDevices)
        {
            yield return null;  //Wait until next frame.

            ConfigureNewPlayerInput(_playerPrefab, _playerIndex, _assignedDevices);
        }

        private void ConfigureNewPlayerInput(GameObject _playerPrefab, int _playerIndex, InputDevice[] _assignedDevices)
        {
            if (!_playerPrefab.TryGetComponent<PlayerInput>(out var playerInput))
            {
                Debug.LogError($"PlayerID {_playerIndex}: No PlayerInput found!");
                return;
            }

            //Load and set the InputActionAsset.
            playerInput.actions = Resources.Load<InputActionAsset>("InputActions/PlayerInputActions");
            //Keep no Input-Device allocation. (Do not force Gamepad allocation!)
            playerInput.defaultControlScheme = null;
            playerInput.neverAutoSwitchControlSchemes = false;
            //How the PlayerInput component invokes events.
            playerInput.notificationBehavior = PlayerNotifications.InvokeCSharpEvents;

            // **FIX**: If no valid device is assigned, check for an available Gamepad.
            if (_assignedDevices == null || _assignedDevices.Length == 0 || _assignedDevices[0] == null)
            {
                Debug.LogWarning($"PlayerID {_playerIndex} has no assigned device! Checking for available Gamepad...");
                if (Gamepad.all.Count > 0)
                {
                    _assignedDevices = new InputDevice[] { Gamepad.all[0] };
                    Debug.Log($"PlayerID {_playerIndex} assigned to Gamepad: {Gamepad.all[0].name}");
                }
                else
                {
                    Debug.LogError($"No Gamepad found for PlayerID {_playerIndex}, falling back to Keyboard.");
                    _assignedDevices = new InputDevice[] { Keyboard.current };
                }
            }
            //Debug.Log($"curCtrlScheme: {playerInput.currentControlScheme} | User: {playerInput.user} | pairedDevice: {playerInput.user.pairedDevices.Count}");
            //string controlScheme = _assignedDevices[0] is Gamepad ? m_gamePadScheme : m_keyboardScheme;
            playerInput.SwitchCurrentActionMap(m_playerActionMap);

            StartCoroutine(SwitchControlSchemeNextFrame(playerInput, playerInput.currentControlScheme, _assignedDevices));
            //Debug.Log($"PlayerID {_playerIndex} with Device: {_assignedDevices[0].name}, ControlScheme: {controlScheme} & active ActionMap: {playerInput.currentActionMap.name}");
            ACheckForPlayerInput?.Invoke(_playerIndex);
        }

        private IEnumerator SwitchControlSchemeNextFrame(PlayerInput _playerInput, string _controlScheme, InputDevice[] _assignedDevices)
        {
            yield return null;   //Wait until next frame to ensure, that PlayerInput is registered right.

            if (_playerInput.user.valid)
            {
                _playerInput.SwitchCurrentControlScheme(_controlScheme, _assignedDevices);
                //InputUser.PerformPairingWithDevice(_assignedDevices[0], _playerInput.user);
                _playerInput.ActivateInput();   //Forces the inputsystem to use the assigned device.
                //Debug.Log($"PlayerID {_playerInput.playerIndex}: ControlScheme: {_controlScheme} - assigned Device(s): {string.Join(", ", _playerInput.devices.Select(d => d.name))}.");
            }
            else
            {
                Debug.LogError($"No valid user on PlayerID {_playerInput.playerIndex}!");
            }
        }
        #endregion
        #endregion

        #region Delegate_Methods
        private InputDevice[] GetDevicesForPlayer(int _playerIndex)
        {
            if (!m_matchValues.PlayerSOData[_playerIndex].DefaultKeyboard)
            {
                for (int uGp = 0; uGp < Gamepad.all.Count; uGp++)
                {
                    if (m_usableGamepads.Contains(Gamepad.all[uGp]))
                    {
                        m_usableGamepads.Remove(Gamepad.all[uGp]);
                        return new InputDevice[] { Gamepad.all[uGp] };    //If enough gamepads are available, return gamepad.
                    }
                }
            }
            return new InputDevice[] { Keyboard.current, Mouse.current };  //Else return keyboard.
        }

        private bool GamepadPressed(out InputDevice _gamepadDevice)
        {
            foreach (var gamepad in Gamepad.all)
            {
                foreach (var control in gamepad.allControls)
                {
                    if (control is ButtonControl button && button.wasPressedThisFrame)
                    {
                        _gamepadDevice = gamepad;
                        return true;
                    }
                }
            }
            _gamepadDevice = null;
            return false;
        }
        #endregion

        private void CleanupPlayers()
        {
            Debug.Log("Cleaning up players...");
            m_matchValues.PlayerSOData.Clear();
        }

        private void CustomControlSchemeSwitch(string _newControlScheme, InputDevice[] _newDevices)
        {
            switch (m_currentControlScheme == _newControlScheme)
            {
                case true:
                    return;
                case false:
                {
                    m_currentControlScheme = _newControlScheme;
                    m_lastUsedDevice = _newDevices[0];

                    AOnDeviceInput(_newControlScheme, _newDevices);
                    //Debug.Log($"Switched to: {_newControlScheme} | Devices: {string.Join(", ", _newDevices.Select(d => d.name))}");

                    break;
                }
            }
        }

        #region Debug-Methods
        //public void DebugDevices()
        //{
        //    //Debug.Log("Debugging InputSystem...");

        //    foreach (var player in PlayerInput.all)
        //    {
        //        Debug.Log($"PlayerID {player.playerIndex} -> Device: {player.devices[0].name}, ControlScheme: {player.currentControlScheme}");
        //    }

        //    //foreach (var user in InputUser.all)
        //    //{
        //    //    Debug.Log($"User {user.index}: Device {user.pairedDevices[0].name} | Control Scheme: {user.controlScheme}");
        //    //}

        //    //foreach (var device in InputSystem.devices)
        //    //{
        //    //    Debug.Log($"Registered Device: {device.name} - {device.deviceId}");
        //    //}
        //}
        #endregion
    }
}