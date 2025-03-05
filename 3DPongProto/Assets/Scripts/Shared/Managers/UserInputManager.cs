using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ThreeDeePongProto.Offline.UI.Menu;
using ThreeDeePongProto.Shared.PlayerCharacter;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using UnityEngine.SceneManagement;

public enum ESceneNames
{
    StartMenu = 0,
    LocalGame = 1,
    LanGame = 2,
    NetGame = 3
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

        //public static UserInputManager Instance { get; private set; }

        [SerializeField] private PlayerInputManager m_playerInputManager;
        [SerializeField] private bool m_joinByDefault = true;

        private readonly PlayerInput m_playerInput;
        private Transform m_playfieldParent;
        //private bool m_menuInputInitialized = false;

        internal bool gDevicesInitialized { get; private set; } = false;

        private const string m_keyboardMouse = "KeyboardMouse", m_keyboardSchemePID0 = "KeyboardPlayerID0", m_keyboardSchemePID1 = "KeyboardPlayerID1", m_keyboardSchemePID2 = "KeyboardPlayerID2", m_keyboardSchemePID3 = "KeyboardPlayerID3", m_keyboardDevice = "Keyboard";
        private const string m_gamePadScheme = "Gamepad", m_gamePadSchemePID0 = "GamepadPlayerID0", m_gamePadSchemePID1 = "GamepadPlayerID1", m_gamePadSchemePID2 = "GamepadPlayerID2", m_gamePadSchemePID3 = "GamepadPlayerID3", m_gamepadDevice = "Gamepad";

        private const string m_playerActionMap = "PlayerActions";
        private string m_currentControlScheme;
        private static string m_lastActionMap;

        private static ActiveInputActionMap m_newActiveActionMap = ActiveInputActionMap.None;

        private readonly List<InputDevice> m_usableGamepads = new();
        //private static readonly Dictionary<int, int> m_playerControllerMap = new();
        private readonly Dictionary<uint, InputDevice> m_playerDeviceMap = new();
        private readonly Dictionary<uint, (string controlScheme, InputDevice[] devices)> m_playerLastKnownSetup = new();

        internal static event Action<uint> ACheckForPlayerInput;            //PlayerInput component on character gets told to configurate itself now.
        internal static event Action<string, InputDevice[]> AOnDeviceInput; //Menus shall switch between Keyboard and Gamepad for navigation.
        internal static event Action<string> AChangeActiveActionMap;        //Player- & Menu-PlayerInput switch controlScheme between Menu <-> Game.

        #region Scriptable_Objects
        [SerializeField] private MatchUIStates m_matchUIStates;
        [SerializeField] private MatchValues m_matchValues;
        #endregion

        private void Awake()
        {
            #region Singleton_Pattern
            //if (Instance == null)
            //{
            //    Instance = this;
            //    //DontDestroyOnLoad(gameObject);
            //}
            //else
            //{
            //    Destroy(gameObject);
            //    return;
            //}
            #endregion

            m_playerInputManager = GetComponent<PlayerInputManager>();
            SetUpPlayerInputManager(m_playerInputManager);
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneManagerLoaded;
            MenuManager.AReLoadScene += OnReLoadScene;
            InputSystem.onDeviceChange += OnDeviceChange;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneManagerLoaded;
            MenuManager.AReLoadScene -= OnReLoadScene;
            InputSystem.onDeviceChange -= OnDeviceChange;
        }

        private void Update()
        {
            ////Keyboard (and Mouse) Input check.
            //if (Keyboard.current.anyKey.wasPressedThisFrame || Mouse.current.delta.ReadValue() != Vector2.zero)
            //{
            //    CustomControlSchemeSwitch(m_keyboardMouse, new InputDevice[] { Keyboard.current, Mouse.current });
            //}
            ////Gamepad-Input check.
            //else if (Gamepad.current != null && GamepadPressed(out InputDevice gamepadDevice))
            //{
            //    CustomControlSchemeSwitch(m_gamePadScheme, new InputDevice[] { gamepadDevice });
            //}

            //if (m_playerInput != null)
            //    SetPlayerDeviceMap(m_playerInput);
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

        #region Subscription_Methods
        /// <summary>
        /// Method to react after a scene has been fully loaded beforehand.
        /// </summary>
        /// <param name="scene"></param>
        /// <param name="mode"></param>
        private void OnSceneManagerLoaded(Scene scene, LoadSceneMode mode)
        {
            //Debug.Log($"OnSceneManagerLoaded fired at {Time.time}");
            int sceneIndex = scene.buildIndex;
            gDevicesInitialized = false;
            //m_menuInputInitialized = false;

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
                    m_playerDeviceMap.Clear();
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

            SetMenuInputScheme();
        }

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

        /// <summary>
        /// InputSystem.OnDeviceChange Subscription Method.
        /// </summary>
        /// <param name="_inputDevice"></param>
        /// <param name="_deviceChange"></param>
        private void OnDeviceChange(InputDevice _inputDevice, InputDeviceChange _deviceChange)
        {
            if (_inputDevice is not Gamepad)
                return;

            switch (_deviceChange)
            {
                case InputDeviceChange.Removed:
                case InputDeviceChange.Disconnected:
                    HandleGamepadDisconnected(_inputDevice);
                    Debug.Log($"OnDeviceChange fired at {Time.time}");
                    break;

                case InputDeviceChange.Added:
                case InputDeviceChange.Reconnected:
                    HandleGamepadReconnect(_inputDevice);
                    Debug.Log($"OnDeviceChange fired at {Time.time}");
                    break;
            }
        }
        #endregion

        #region Instantiate_and_Configurate_Player_PlayerInput
        private void InstantiatePlayer()
        {
            uint playerCount = (uint)m_matchUIStates.EPlayerAmount;

            for (int playerIndex = 0; playerIndex < playerCount; playerIndex++)
            {
                InputDevice[] assignedDevices = GetDevicesForPlayer(playerIndex);
                string setControlScheme = assignedDevices[0] is Gamepad ? m_gamePadScheme : m_keyboardMouse;

                PlayerInput newPlayerInput = PlayerInput.Instantiate(m_matchValues.PlayerSOData[playerIndex].Prefab, controlScheme: setControlScheme, pairWithDevice: assignedDevices[0]);

                GameObject newPlayer = newPlayerInput.gameObject;
                newPlayer.transform.SetParent(m_playfieldParent);
                //Debug.Log($"PlayerID {playerIndex} assigned device(s): {string.Join(", ", assignedDevices.Select(d => d.name))}");

                //Waits a frame until old _playerInput component is destroyed.
                StartCoroutine(DelayedPlayerInputSetup(newPlayer, playerIndex, assignedDevices));
            }

            gDevicesInitialized = true;
            Debug.Log("DeviceMap initialized.");
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
                Debug.LogError($"PlayerIndex {_playerIndex} | PlayerUserID {playerInput.user.id}: No PlayerInput found!");
                return;
            }

            if (!_playerPrefab.TryGetComponent<CharacterMainController>(out var playerController))
            {
                Debug.LogError($"PlayerIndex {_playerIndex} | PlayerUserID {playerInput.user.id}: No PlayerController found!");
                return;
            }

            ////Mapping playerController.m_playerID to the fix _playerInput.user.index, to bypass changing indices on varying _playerInput components.
            //m_playerControllerMap[playerInput.user.index] = playerController.m_playerId;

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
                Debug.LogWarning($"PlayerIndex {_playerIndex} | PlayerUserID {playerInput.user.id} has no assigned device! Checking for available Gamepad...");
                if (Gamepad.all.Count > 0)
                {
                    _assignedDevices = new InputDevice[] { Gamepad.all[0] };
                    Debug.Log($"PlayerIndex {_playerIndex} | PlayerUserID {playerInput.user.id} assigned to Gamepad: {Gamepad.all[0].name}");
                }
                else
                {
                    Debug.LogError($"No Gamepad found for PlayerIndex {_playerIndex} | PlayerUserID {playerInput.user.id}, falling back to Keyboard.");
                    _assignedDevices = new InputDevice[] { Keyboard.current };
                }
            }

            playerInput.SwitchCurrentActionMap(m_playerActionMap);

            StartCoroutine(SwitchControlSchemeNextFrame(playerInput, playerInput.currentControlScheme, _assignedDevices));
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
                SetPlayerDeviceMap(_playerInput);
            }
            else
            {
                Debug.LogError($"No valid user on PlayerIndex {_playerInput.playerIndex} | PlayerUserID {_playerInput.user.id}!");
            }
        }
        #endregion

        private void SetMenuInputScheme()
        {
            //if (m_menuInputInitialized)
            //{
            //    Debug.Log("MENU INPUT SCHEME ALREADY INITIALIZED!!!.");
            //    return;
            //}

            //m_menuInputInitialized = true;

            var menuPlayerInput = GetComponent<PlayerInput>();
            var allPlayers = PlayerInput.all;

            //Exclude Menu.
            var playerInputs = allPlayers.Where(p => p != menuPlayerInput && p.GetComponent<CharacterMainController>() != null).ToList();

            if (playerInputs.Count > 0)
            {
                var firstPlayer = playerInputs[0];

                var devices = firstPlayer.devices.ToArray();
                var controlScheme = firstPlayer.currentControlScheme;

                CustomControlSchemeSwitch(controlScheme, devices);

                Debug.Log($"Menu uses PlayerIndex {firstPlayer.playerIndex}'s | PlayerUserID {firstPlayer.user.id}'s controlScheme: {controlScheme}.");
            }
            else if (Gamepad.all.Count > 0)     //Fallback, if only the menu exists.
            {
                var gamepad = Gamepad.current ?? Gamepad.all[0];
                CustomControlSchemeSwitch(m_gamePadScheme, new InputDevice[] { gamepad });

                Debug.Log($"Menu uses default Gamepad scheme: {gamepad.name}.");
            }
            else
            {
                CustomControlSchemeSwitch(m_keyboardMouse, new InputDevice[] { Keyboard.current, Mouse.current });

                Debug.Log("Menu uses default Keyboard/Mouse scheme.");
            }
        }

        private void SetPlayerDeviceMap(PlayerInput _playerInput)
        {
            uint playerUserID = _playerInput.user.id;
            //int playerIndex = _playerInput.playerIndex;
            var deviceMap = GetPlayerDeviceMap();

            InputDevice newDevice = _playerInput.devices.Count > 0 ? _playerInput.devices[0] : null;

            if (newDevice == null)
            {
                Debug.LogWarning($"No active device found for PlayerIndex {_playerInput.playerIndex} | PlayerUserID {playerUserID}.");
                return;
            }

            string newControlScheme;
            InputDevice[] newDevices;

            if (newDevice is Gamepad)
            {
                newControlScheme = m_gamePadScheme;
                newDevices = new InputDevice[] { newDevice };
            }
            else if (newDevice == Keyboard.current || newDevice == Mouse.current)
            {
                newControlScheme = m_keyboardMouse;
                newDevices = new InputDevice[] { Keyboard.current, Mouse.current };
            }
            else
            {
                Debug.LogWarning($"Unknown device type for PlayerIndex {_playerInput.playerIndex} | PlayerUserID {playerUserID}: {newDevice.name}");
                return;
            }

            if (deviceMap.TryGetValue(playerUserID, out var currentDevice))
            {
                if (newDevice != currentDevice)
                {
                    deviceMap[playerUserID] = newDevice;

                    //Pair the new device to the playerInput.
                    InputUser.PerformPairingWithDevice(newDevice, _playerInput.user);

                    //Switch to the correct control scheme with the right devices.
                    _playerInput.SwitchCurrentControlScheme(newControlScheme, newDevices);

                    //Notify MenuManager or other systems.
                    AOnDeviceInput?.Invoke(newControlScheme, newDevices);

                    Debug.Log($"Updated PlayerIndex {_playerInput.playerIndex} | PlayerUserID {playerUserID} to {newControlScheme} with {newDevice.name}");
                }
                else
                {
                    Debug.Log($"Device for PlayerIndex {_playerInput.playerIndex} | PlayerUserID {playerUserID} unchanged ({newDevice.name})");
                }
            }
            else
            {
                deviceMap.Add(playerUserID, newDevice);

                // Pair the new device to the playerInput
                InputUser.PerformPairingWithDevice(newDevice, _playerInput.user);

                // Switch to the correct control scheme with the right devices
                _playerInput.SwitchCurrentControlScheme(newControlScheme, newDevices);

                // Notify MenuManager or other systems
                AOnDeviceInput?.Invoke(newControlScheme, newDevices);

                Debug.Log($"Added PlayerIndex {_playerInput.playerIndex} | PlayerUserID {playerUserID} to {newControlScheme} with {newDevice.name}");
            }

            m_playerLastKnownSetup[playerUserID] = (newControlScheme, newDevices);
            ACheckForPlayerInput?.Invoke(playerUserID);
        }
        #endregion

        #region Delegate_Methods (with return value)
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

        //private bool GamepadPressed(out InputDevice _gamepadDevice)
        //{
        //    foreach (var gamepad in Gamepad.all)
        //    {
        //        foreach (var control in gamepad.allControls)
        //        {
        //            if (control is ButtonControl button && button.wasPressedThisFrame)
        //            {
        //                _gamepadDevice = gamepad;
        //                return true;
        //            }
        //        }
        //    }
        //    _gamepadDevice = null;
        //    return false;
        //}
        #endregion

        private void CleanupPlayers()
        {
            Debug.Log("Cleaning up players and inputs...");
            m_matchValues.PlayerSOData.Clear();
            m_usableGamepads.Clear();
            for (int i = 0; i < Gamepad.all.Count; i++)
            {
                if (Gamepad.all[i] != null && !m_usableGamepads.Contains(Gamepad.all[i]))
                    m_usableGamepads.Add(Gamepad.all[i]);
            }
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

                    AOnDeviceInput(_newControlScheme, _newDevices);
                    Debug.Log($"Switched to: {_newControlScheme} | Devices: {string.Join(", ", _newDevices.Select(d => d.name))}");

                    break;
                }
            }
        }

        private void HandleGamepadReconnect(InputDevice _reconnectedDevice)
        {
            if (_reconnectedDevice is not Gamepad reconnectedGamepad)
            {
                Debug.LogWarning("Reconnect called, but device is not a Gamepad.");
                return;
            }

            foreach (var playerInput in PlayerInput.all)
            {
                if (m_playerLastKnownSetup.TryGetValue(playerInput.user.id, out var setup))
                {
                    if (setup.controlScheme == m_gamePadScheme)
                    {
                        var deviceMap = GetPlayerDeviceMap();
                        deviceMap[playerInput.user.id] = reconnectedGamepad;

                        InputUser.PerformPairingWithDevice(reconnectedGamepad, playerInput.user);

                        var newDevices = new InputDevice[] { reconnectedGamepad };
                        playerInput.SwitchCurrentControlScheme(m_gamePadScheme, newDevices);

                        AOnDeviceInput?.Invoke(m_gamePadScheme, newDevices);

                        Debug.Log($"Reconnected {reconnectedGamepad.name} to PlayerIndex {playerInput.playerIndex} | PlayerUserID {playerInput.user.id}.");
                        break; //Allocate to one specific playerInput and exit.
                    }
                }
            }
        }

        private void HandleGamepadDisconnected(InputDevice _disconnectedDevice)
        {
            if (_disconnectedDevice is not Gamepad disconnectedGamepad)
            {
                Debug.LogWarning("Disconnect called, but device is not a Gamepad.");
                return;
            }

            foreach (var playerInput in PlayerInput.all)
            {
                if (playerInput.devices.Contains(disconnectedGamepad))
                {
                    var newDevices = new InputDevice[] { Keyboard.current, Mouse.current };
                    var deviceMap = GetPlayerDeviceMap();

                    deviceMap[playerInput.user.id] = Keyboard.current;   //Representative for Keyboard and Mouse.

                    //Switch controlScheme.
                    playerInput.SwitchCurrentControlScheme(m_keyboardMouse, newDevices);

                    //Invoke event for MenuManager, etc.
                    AOnDeviceInput?.Invoke(m_keyboardMouse, newDevices);

                    Debug.Log($"PlayerIndex {playerInput.playerIndex} | PlayerUserID {playerInput.user.id} switched to Keyboard/Mouse after disconnecting Gamepad.");
                    break;
                }
            }
        }

        internal Dictionary<uint, InputDevice> GetPlayerDeviceMap()
        {
            return m_playerDeviceMap;
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

            m_lastActionMap = _actionMap;
            m_newActiveActionMap = _actionMap == ActiveInputActionMap.PlayerActions.ToString()
                ? ActiveInputActionMap.PlayerActions : ActiveInputActionMap.UserInterface;

            AChangeActiveActionMap?.Invoke(_actionMap);
        }
        #endregion
    }
}