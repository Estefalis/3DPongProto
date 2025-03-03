using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ThreeDeePongProto.Offline.UI.Menu;
using ThreeDeePongProto.Shared.PlayerCharacter;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Users;
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

        //public static UserInputManager Instance { get; private set; }

        [SerializeField] private PlayerInputManager m_playerInputManager;
        [SerializeField] private bool m_joinByDefault = true;

        private PlayerInput m_playerInput;
        private Transform m_playfieldParent;

        private const string m_keyboardMouse = "KeyboardMouse", m_keyboardSchemePID0 = "KeyboardPlayerID0", m_keyboardSchemePID1 = "KeyboardPlayerID1", m_keyboardSchemePID2 = "KeyboardPlayerID2", m_keyboardSchemePID3 = "KeyboardPlayerID3", m_keyboardDevice = "Keyboard";
        private const string m_gamePadScheme = "Gamepad", m_gamePadSchemePID0 = "GamepadPlayerID0", m_gamePadSchemePID1 = "GamepadPlayerID1", m_gamePadSchemePID2 = "GamepadPlayerID2", m_gamePadSchemePID3 = "GamepadPlayerID3", m_gamepadDevice = "Gamepad";

        private const string m_playerActionMap = "PlayerActions";
        private string m_currentControlScheme;
        private static string m_lastActionMap;

        private static ActiveInputActionMap m_newActiveActionMap = ActiveInputActionMap.None;

        private InputDevice m_lastUsedDevice;

        private readonly List<InputDevice> m_usableGamepads = new();
        private static readonly Dictionary<int, int> m_playerControllerMap = new();
        private Dictionary<int, InputDevice> m_playerDeviceMap = new();
        private Dictionary<int, (string controlScheme, InputDevice[] devices)> m_playerLastKnownSetup = new();

        internal static event Action<int> ACheckForPlayerInput;             //PlayerInput component on character gets told to configurate itself now.
        internal static event Action<string, InputDevice[]> AOnDeviceInput; //Menu shall switch between Keyboard and Gamepad for it's navigation.
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

            if (m_playerInput != null)
                SetPlayerDeviceMap(m_playerInput);
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

            SetInitialInput();
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
            if (!(_inputDevice is Gamepad))
                return;

            switch (_deviceChange)
            {
                case InputDeviceChange.Removed:
                case InputDeviceChange.Disconnected:
                    HandleGamepadDisconnected(_inputDevice);
                    break;

                case InputDeviceChange.Added:
                case InputDeviceChange.Reconnected:
                    HandleGamepadReconnect(_inputDevice);
                    Debug.Log($"Gamepad connected: {_inputDevice.name}");
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
                string setControlScheme = assignedDevices[0] is Gamepad ? m_gamePadScheme : m_keyboardMouse;

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

            if (!_playerPrefab.TryGetComponent<CharacterMainController>(out var playerController))
            {
                Debug.LogError($"Player {_playerIndex}: No PlayerController found!");
                return;
            }

            //Mapping playerController.m_playerID to the fix _playerInput.user.index, to bypass changing indices on varying _playerInput components.
            m_playerControllerMap[playerInput.user.index] = playerController.m_playerId;

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

            playerInput.SwitchCurrentActionMap(m_playerActionMap);

            StartCoroutine(SwitchControlSchemeNextFrame(playerInput, playerInput.currentControlScheme, _assignedDevices));

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
                SetPlayerDeviceMap(_playerInput);
            }
            else
            {
                Debug.LogError($"No valid user on PlayerID {_playerInput.playerIndex}!");
            }
        }
        #endregion

        private void SetInitialInput()
        {
            CustomControlSchemeSwitch(m_keyboardMouse, new InputDevice[] { Keyboard.current, Mouse.current });
        }

        private void SetPlayerDeviceMap(PlayerInput _playerInput)
        {
            int playerIndex = _playerInput.playerIndex;
            var deviceMap = GetPlayerDeviceMap();

            InputDevice newDevice = _playerInput.devices.Count > 0 ? _playerInput.devices[0] : null;

            if (newDevice == null)
            {
                Debug.LogWarning($"No active device found for Player {playerIndex}");
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
                Debug.LogWarning($"Unknown device type for Player {playerIndex}: {newDevice.name}");
                return;
            }

            if (deviceMap.TryGetValue(playerIndex, out var currentDevice))
            {
                if (newDevice != currentDevice)
                {
                    deviceMap[playerIndex] = newDevice;

                    //Pair the new device to the player.
                    InputUser.PerformPairingWithDevice(newDevice, _playerInput.user);

                    //Switch to the correct control scheme with the right devices.
                    _playerInput.SwitchCurrentControlScheme(newControlScheme, newDevices);

                    //Notify MenuManager or other systems.
                    AOnDeviceInput?.Invoke(newControlScheme, newDevices);

                    Debug.Log($"Updated Player {playerIndex} to {newControlScheme} with {newDevice.name}");
                }
                else
                {
                    Debug.Log($"Device for Player {playerIndex} unchanged ({newDevice.name})");
                }
            }
            else
            {
                deviceMap.Add(playerIndex, newDevice);

                // Pair the new device to the player
                InputUser.PerformPairingWithDevice(newDevice, _playerInput.user);

                // Switch to the correct control scheme with the right devices
                _playerInput.SwitchCurrentControlScheme(newControlScheme, newDevices);

                // Notify MenuManager or other systems
                AOnDeviceInput?.Invoke(newControlScheme, newDevices);

                Debug.Log($"Added Player {playerIndex} to {newControlScheme} with {newDevice.name}");
            }

            m_playerLastKnownSetup[playerIndex] = (newControlScheme, newDevices);
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
                    m_lastUsedDevice = _newDevices[0];

                    AOnDeviceInput(_newControlScheme, _newDevices);
                    Debug.Log($"Switched to: {_newControlScheme} | Devices: {string.Join(", ", _newDevices.Select(d => d.name))}");

                    break;
                }
            }
        }

        private void HandleGamepadReconnect(InputDevice reconnectedDevice)
        {
            if (reconnectedDevice as Gamepad == null)
            {
                Debug.LogWarning("Reconnect called, but device is not a Gamepad.");
                return;
            }

            foreach (var player in PlayerInput.all)
            {
                if (m_playerLastKnownSetup.TryGetValue(player.playerIndex, out var setup))
                {
                    if (setup.controlScheme == m_gamePadScheme)
                    {
                        var deviceMap = GetPlayerDeviceMap();
                        deviceMap[player.playerIndex] = reconnectedDevice as Gamepad;

                        InputUser.PerformPairingWithDevice(reconnectedDevice as Gamepad, player.user);

                        var newDevices = new InputDevice[] { reconnectedDevice as Gamepad };
                        player.SwitchCurrentControlScheme(m_gamePadScheme, newDevices);

                        AOnDeviceInput?.Invoke(m_gamePadScheme, newDevices);

                        Debug.Log($"🔄 Reconnected {(reconnectedDevice as Gamepad).name} to Player {player.playerIndex}");
                        break; // Nur einem Spieler zuweisen
                    }
                }
            }
        }

        private void HandleGamepadDisconnected(InputDevice _disconnectedDevice)
        {
            foreach (var player in PlayerInput.all)
            {
                if (player.devices.Contains(_disconnectedDevice))
                {
                    Debug.Log($"Gamepad {_disconnectedDevice.name} disconnected from Player {player.playerIndex}. Switching to Keyboard.");

                    var newDevices = new InputDevice[] { Keyboard.current, Mouse.current };
                    var deviceMap = GetPlayerDeviceMap();

                    deviceMap[player.playerIndex] = Keyboard.current;

                    //Switch controlScheme.
                    player.SwitchCurrentControlScheme(m_keyboardMouse, newDevices);

                    //Invoke event for MenuManager, etc.
                    AOnDeviceInput?.Invoke(m_keyboardMouse, newDevices);
                }
            }
        }

        internal Dictionary<int, InputDevice> GetPlayerDeviceMap()
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

            m_newActiveActionMap = _actionMap == ActiveInputActionMap.PlayerActions.ToString()
                ? ActiveInputActionMap.PlayerActions : ActiveInputActionMap.UserInterface;

            AChangeActiveActionMap?.Invoke(_actionMap);
        }
        #endregion
    }
}