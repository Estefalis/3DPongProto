using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ThreeDeePongProto.Offline.UI.Menu;
using ThreeDeePongProto.Shared.HelperClasses;
using ThreeDeePongProto.Shared.PlayerCharacter;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using UnityEngine.SceneManagement;

internal enum ESceneNames
{
    StartMenu = 0,
    LocalGame = 1,
    LanGame = 2,
    NetGame = 3
}

//internal enum EPlayerMenuControl
//{
//    None,
//    SpecificPlayer,
//    FirstPlayer,
//    LastPlayer,
//    EachPlayer,
//    HostPlayer,
//}

internal enum EInputActionMaps
{
    None,
    PlayerActions,
    UserInterface
}

namespace ThreeDeePongProto.Shared.Managers
{
    public class UserInputManager : MonoBehaviour
    {
        //public static UserInputManager Instance { get; private set; }

        [SerializeField] private PlayerInputManager m_playerInputManager;
        [SerializeField] private bool m_joinByDefault = true;

        private Transform m_playfieldParent;
        private PlayerInput m_menuPlayerInput;

        private MenuManager m_menuManager;
        internal bool GDevicesInitialized { get; private set; } = false;

        private const string m_keyboardMouseScheme = "KeyboardMouse";
        private const string m_gamePadScheme = "Gamepad";

        internal static string SetActionMap { get => m_lastActionMap; }
        private static string m_lastActionMap;

        private static EInputActionMaps m_newActiveActionMap = EInputActionMaps.None;

        #region Lists_and_Dictionaries
        private readonly List<InputDevice> m_usableGamepads = new();
        private readonly Dictionary<uint, InputDevice> m_playerDeviceMap = new();
        private readonly Dictionary<uint, InputDevice[]> m_startPlayerSetup = new();
        #endregion

        #region Actions_and_Functions
        internal static event Action<uint> ACheckForPlayerInput;            //Tells Player's PlayerInput to configurate itself now.
        internal static event Action<string> AChangeActiveActionMap;        //PlayerInput switch controlScheme between Menu <-> Game.
        #endregion

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

            m_lastActionMap = "";
            m_playerInputManager = GetComponent<PlayerInputManager>();
            SetUpPlayerInputManager(m_playerInputManager);
        }

        private void OnEnable()
        {
            InputSystem.onDeviceChange += OnDeviceChange;
            LocalMatchManager.AGroundInstantiated += SpawnLocalPlayers;
            SceneManager.sceneLoaded += OnSceneManagerLoaded;
            MenuManager.AReLoadScene += OnReLoadScene;
        }

        private void OnDisable()
        {
            InputSystem.onDeviceChange -= OnDeviceChange;
            LocalMatchManager.AGroundInstantiated -= SpawnLocalPlayers;
            SceneManager.sceneLoaded -= OnSceneManagerLoaded;
            MenuManager.AReLoadScene -= OnReLoadScene;
        }

        #region Optional_Update_DevicePress_Comparison
        ////Keyboard (and Mouse) Input check.
        //if (Keyboard.current.anyKey.wasPressedThisFrame || Mouse.current.delta.ReadValue() != Vector2.zero)
        //{
        //    CustomControlSchemeSwitch(m_keyboardMouseScheme, new InputDevice[] { Keyboard.current, Mouse.current });
        //}
        ////Gamepad-Input check.
        //else if (Gamepad.current != null && GamepadPressed(out InputDevice gamepadDevice))
        //{
        //    CustomControlSchemeSwitch(m_gamePadScheme, new InputDevice[] { gamepadDevice });
        //}
        #endregion

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

        #region Menu-PlayerInput-Configuration
        private void GetMenuManager()
        {
            m_menuManager = FindObjectOfType<MenuManager>();

            if (m_menuManager != null)
            {
                m_menuPlayerInput = m_menuManager.GetComponent<PlayerInput>();

                if (m_menuPlayerInput == null)
                {
                    Debug.LogError("MenuManager found, but no PlayerInput attached.");
                }
            }
            else
            {
                Debug.LogWarning("No MenuManager found in scene.");
            }
        }

        private void SetMenuInputScheme()
        {
            if (m_menuPlayerInput == null)
            {
                Debug.LogError($"MenuPlayerInput is not set ({m_menuPlayerInput == null}). Aborting.");
                return;
            }

            var allPlayerInputs = PlayerInput.all;
            var menuPlayerInput = m_menuPlayerInput;

            //Separate playerInputs from menuInput.
            var playerInputList = allPlayerInputs.Where(aPI => aPI != menuPlayerInput).ToList();
            PlayerInput selectedPlayerInput = DeterminePrimaryController(playerInputList);

            string controlScheme;
            InputDevice[] devices;

            if (selectedPlayerInput == null || selectedPlayerInput == m_menuPlayerInput)
            {
                controlScheme = Gamepad.all.Count > 0 ? m_gamePadScheme : m_keyboardMouseScheme;
                devices = Gamepad.all.Count > 0 ? new InputDevice[] { Gamepad.current ?? Gamepad.all[0] } : new InputDevice[] { Keyboard.current, Mouse.current };

                selectedPlayerInput = menuPlayerInput; //Menu controls itself.
            }
            else
            {
                controlScheme = selectedPlayerInput.currentControlScheme;
                devices = selectedPlayerInput.devices.ToArray();
            }
#if UNITY_EDITOR
            //Debug.Log($"SetMenuInputScheme: Menu now gets controlled with {string.Join(", ", devices.Select(d => d.name))}.");
#endif
        }
        #endregion

        private void SpawnLocalPlayers(Transform _playfieldParent)
        {
            m_playfieldParent = _playfieldParent;
            m_playerDeviceMap.Clear();

            InstantiatePlayer();
        }

        #region Subscription_Methods
        /// <summary>
        /// Method to react after a scene has been fully loaded beforehand.
        /// </summary>
        /// <param name="scene"></param>
        /// <param name="mode"></param>
        private void OnSceneManagerLoaded(Scene scene, LoadSceneMode mode)
        {
            int sceneIndex = scene.buildIndex;

            GetMenuManager();

            switch (sceneIndex)
            {
                case 0:
                {
                    CleanUpPlayers();
                    break;
                }
                default:
                    break;
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
                {
                    HandleGamepadDisconnected(_inputDevice);
                }
                break;

                case InputDeviceChange.Added:
                case InputDeviceChange.Reconnected:
                    HandleGamepadReconnect(_inputDevice);
                    break;
            }
        }
        #endregion

        #region Instantiate_and_Configurate_Player_PlayerInput
        private void InstantiatePlayer()
        {
            uint playerCount = (uint)m_matchUIStates.EPlayerAmount;

            for (int playerIndex = 0; playerIndex < playerCount; playerIndex++)     //int playerIndex 0 - 3.
            {
                InputDevice[] assignedDevices = GetDevicesForPlayer(playerIndex);
                string setControlScheme = GetControlSchemeForPlayer(playerIndex, assignedDevices);
                PlayerInput newPlayerInput = PlayerInput.Instantiate(m_matchValues.PlayerSOData[playerIndex].Prefab, controlScheme: setControlScheme, pairWithDevice: assignedDevices.Length > 0 ? assignedDevices[0] : null);
                //Debug.Log($"InstantiatePlayer-Device(s): {string.Join(", ", assignedDevices.Select(d => d.name))}.");
                GameObject newPlayer = newPlayerInput.gameObject;
                newPlayer.transform.SetParent(m_playfieldParent);
                //Waits a frame until old _playerInput component is destroyed.
                StartCoroutine(DelayedPlayerInputSetup(newPlayer, playerIndex, assignedDevices));
            }

            GDevicesInitialized = true;
#if UNITY_EDITOR
            //Debug.Log("DeviceMap initialized.");
#endif
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

            //Load and set the InputActionAsset.
            playerInput.actions = Resources.Load<InputActionAsset>("InputActions/PlayerInputActions");
            //Keep no Input-Device allocation. (Do not force Gamepad allocation!)
            playerInput.defaultControlScheme = null;
            playerInput.neverAutoSwitchControlSchemes = false;
            //How the PlayerInput component invokes events.
            playerInput.notificationBehavior = PlayerNotifications.InvokeCSharpEvents;

            //If no valid device is assigned, check for an available Gamepad.
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
                    Debug.LogError($"No Gamepad found for PlayerIndex {_playerIndex} | PlayerUserID {playerInput.user.id}, falling back to Keyboard and Mouse.");
                    _assignedDevices = new InputDevice[] { Keyboard.current, Mouse.current };
                }
            }

            playerInput.SwitchCurrentActionMap(EInputActionMaps.PlayerActions.ToString());

            StartCoroutine(SwitchControlSchemeNextFrame(playerInput, _playerIndex, playerInput.currentControlScheme, _assignedDevices));
        }

        private IEnumerator SwitchControlSchemeNextFrame(PlayerInput _playerInput, int _playerIndex, string _controlScheme, InputDevice[] _assignedDevices)
        {
            yield return null;   //Wait until next frame to ensure, that PlayerInput is registered right.

            if (_playerInput.user.valid)
            {
                _playerInput.SwitchCurrentControlScheme(_controlScheme, _assignedDevices);
#if UNITY_EDITOR
                //Debug.Log($"PlayerID {_playerIndex}: ControlScheme: {_controlScheme} - ArgDevices: {string.Join(", ", _assignedDevices.Select(d => d.name))} - assigned Device(s): {string.Join(", ", _playerInput.devices.Select(d => d.name))}.");
#endif
                SetPlayerDeviceMap(_playerInput, _playerIndex);
            }
            else
            {
                Debug.LogError($"No valid user on PlayerIndex {_playerIndex} | PlayerUserID {_playerInput.user.id}!");
            }
        }
        #endregion

        private void SetPlayerDeviceMap(PlayerInput _playerInput, int _playerIndex)
        {
            uint playerUserID = _playerInput.user.id;
            var deviceMap = GetPlayerDeviceMap();

            InputDevice newDevice = _playerInput.devices.Count > 0 ? _playerInput.devices[0] : null;

            if (newDevice == null)
            {
                Debug.LogWarning($"No active device found for PlayerIndex {_playerIndex} | PlayerUserID {playerUserID}.");
                return;
            }

            string newControlScheme;
            InputDevice[] newDevices;

            if (newDevice is Gamepad)
            {
                newDevices = new InputDevice[] { newDevice };
                newControlScheme = GetControlSchemeForPlayer(_playerIndex, newDevices);
            }
            else if (newDevice is Keyboard/*.current || newDevice == Mouse.current*/)
            {
                newDevices = new InputDevice[] { Keyboard.current, Mouse.current };
                newControlScheme = GetControlSchemeForPlayer(_playerIndex, newDevices);
            }
            else
            {
                Debug.LogWarning($"Unknown device type for PlayerIndex {_playerInput.playerIndex} | PlayerUserID {playerUserID}: {newDevice.name}");
                return;
            }
#if UNITY_EDITOR
            //Debug.Log($"SetPlayerDeviceMap-Device(s): {string.Join(", ", _playerInput.devices.Select(d => d.name))} | Index: {_playerInput.playerIndex} | UserID: {_playerInput.user.id}.");  
#endif
            if (deviceMap.TryGetValue(playerUserID, out var currentDevice))
            {
                switch (newDevice != currentDevice)
                {
                    case true:
                    {
                        //New device replaces the old.
                        deviceMap[playerUserID] = newDevice;

                        //Pair the new device to the playerInput.
                        if (!_playerInput.user.pairedDevices.Contains(newDevice))
                            InputUser.PerformPairingWithDevice(newDevice, _playerInput.user);

                        //Switch to the correct control scheme with the right devices.
                        _playerInput.SwitchCurrentControlScheme(newControlScheme, newDevices);

                        Debug.Log($"Updated PlayerIndex {_playerInput.playerIndex} | PlayerUserID {playerUserID} to {newControlScheme} with {newDevice.name}");
                        break;
                    }
                    case false:
                    {
#if UNITY_EDITOR
                        //Debug.Log($"Device for PlayerIndex {_playerInput.playerIndex} | PlayerUserID {playerUserID} unchanged ({newDevice.name})");  
#endif
                        break;
                    }
                }
            }
            else
            {
                //If there is no currentDevice, just add the incoming device.
                deviceMap.Add(playerUserID, newDevice);

                //Pair the new device to the playerInput.
                if (!_playerInput.user.pairedDevices.Contains(newDevice))
                    InputUser.PerformPairingWithDevice(newDevice, _playerInput.user);

                //Switch to the correct controlScheme with the right devices.
                _playerInput.SwitchCurrentControlScheme(newControlScheme, newDevices);
            }
#if UNITY_EDITOR
            //Debug.Log($"PlayerUserID: {_playerInput.user.id} | PlayerIndex: {_playerInput.playerIndex} | NewScheme: {newControlScheme} | PlayerUserScheme: {_playerInput.user.controlScheme} | PlayerScheme: {_playerInput.currentControlScheme} | PlayerDevices: {string.Join(", ", _playerInput.devices.Select(d => d.name))} | PairedDevices: {string.Join(", ", _playerInput.user.pairedDevices.Select(d => d.name))}");  
#endif
            _playerInput.ActivateInput();   //Forces the inputsystem to use the assigned device.
            m_startPlayerSetup[playerUserID] = newDevices;
            ACheckForPlayerInput?.Invoke(_playerInput.user.id);
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

        private string GetControlSchemeForPlayer(int _playerIndex, InputDevice[] _assignedDevices)
        {
            if (_assignedDevices.Length > 0)
            {
                GetDeviceHelper.GetPlayerControlScheme(_playerIndex, _assignedDevices[0], out string controlScheme, out _);
                return controlScheme;
            }

            return m_keyboardMouseScheme;
        }

        private PlayerInput DeterminePrimaryController(List<PlayerInput> _allPlayerInputs)
        {
            var menuPlayerInput = m_menuPlayerInput;

            var allPlayers = _allPlayerInputs.Where(p => p != menuPlayerInput).ToList();
            var firstSelectedPlayerInput = allPlayers.FirstOrDefault(p => p.GetComponent<CharacterMainController>() != null);
            var firstSelectedMenuInput = _allPlayerInputs.FirstOrDefault(m => m == menuPlayerInput);

            return allPlayers.Count > 0 ? firstSelectedPlayerInput : firstSelectedMenuInput;
        }

        internal Dictionary<uint, InputDevice> GetPlayerDeviceMap()
        {
            return m_playerDeviceMap;
        }

        #region Gamepad_ButtonPresses
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
        #endregion

        #region Dis-_and_Reconnect_Devices
        private void HandleGamepadDisconnected(InputDevice _disconnectedDevice)
        {
            if (_disconnectedDevice is not Gamepad disconnectedGamepad)
            {
                Debug.LogWarning("Disconnect called, but device is not a Gamepad.");
                return;
            }

            foreach (var playerInput in PlayerInput.all)
            {
                if (m_playerDeviceMap.TryGetValue(playerInput.user.id, out var dictDevice))
                {
                    if (dictDevice == disconnectedGamepad)
                    {
                        var newDevices = new InputDevice[] { Keyboard.current, Mouse.current };
                        var deviceMap = GetPlayerDeviceMap();

                        deviceMap[playerInput.user.id] = Keyboard.current;   //Representative for Keyboard and Mouse.

                        //Switch controlScheme.
                        playerInput.SwitchCurrentControlScheme(m_keyboardMouseScheme, newDevices);
                        InputUser.PerformPairingWithDevice(newDevices[0], playerInput.user);

                        Debug.Log($"Object: {playerInput.gameObject.name} | PlayerIndex {playerInput.playerIndex} | PlayerUserID {playerInput.user.id} switched to Keyboard/Mouse after disconnecting Gamepad.");
                    }
                }
            }
        }

        private void HandleGamepadReconnect(InputDevice reconnectedGamepad)
        {
            if (reconnectedGamepad is not Gamepad)
            {
                Debug.LogWarning("Reconnect called, but device is not a Gamepad.");
                return;
            }

            var allPlayerInputs = PlayerInput.all.ToList();
            var takenPlayerInput = DeterminePrimaryController(allPlayerInputs);

            if (takenPlayerInput == null)
            {
                Debug.LogWarning("No valid PlayerInput found to assign Gamepad.");
                return;
            }

            if (!m_startPlayerSetup.ContainsKey(takenPlayerInput.user.id))
            {
                m_startPlayerSetup[takenPlayerInput.user.id] = new InputDevice[] { reconnectedGamepad };
                Debug.Log($"Added missing device entry for Player {takenPlayerInput.user.id}: {reconnectedGamepad.displayName}");
            }

            var deviceMap = GetPlayerDeviceMap();
            deviceMap[takenPlayerInput.user.id] = reconnectedGamepad;

            int deviceIndex = -1;
            foreach (var arraySlot in deviceMap)
            {
                deviceIndex += 1;
                if (arraySlot.Value == reconnectedGamepad)
                    break;
            }

            InputDevice[] newDevices = new InputDevice[] { reconnectedGamepad };
            InputUser.PerformPairingWithDevice(reconnectedGamepad, takenPlayerInput.user);
            takenPlayerInput.SwitchCurrentControlScheme(GetControlSchemeForPlayer(deviceIndex, newDevices), newDevices);
#if UNITY_EDITOR
            Debug.Log($"Reconnected {reconnectedGamepad.name} to PlayerUserID {takenPlayerInput.user.id}.");
#endif
        }
        #endregion

        //Scene-Specific Reset.
        private void CleanUpPlayers()
        {
#if UNITY_EDITOR
            //Debug.Log("Cleaning up players and inputs...");
#endif
            m_matchValues.PlayerSOData.Clear();
            m_usableGamepads.Clear();
            for (int i = 0; i < Gamepad.all.Count; i++)
            {
                if (Gamepad.all[i] != null && !m_usableGamepads.Contains(Gamepad.all[i]))
                    m_usableGamepads.Add(Gamepad.all[i]);
            }
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
            m_newActiveActionMap = _actionMap == EInputActionMaps.PlayerActions.ToString()
                ? EInputActionMaps.PlayerActions : EInputActionMaps.UserInterface;

            AChangeActiveActionMap?.Invoke(_actionMap);
        }
        #endregion
    }
}