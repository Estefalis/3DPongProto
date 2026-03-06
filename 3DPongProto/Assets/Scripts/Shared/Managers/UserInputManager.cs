using System;
using System.Collections.Generic;
using System.Linq;
using ThreeDeePongProto.Shared.InputActions;
using ThreeDeePongProto.Shared.PlayerCharacter;
using UnityEngine;
using UnityEngine.InputSystem;

internal enum ESceneNames
{
    BootScene = 0,
    StartMenu = 1,
    LocalGame = 2,
    LanGame = 3,
    NetGame = 4
}

internal enum EInputActionMaps
{
    None,
    PlayerActions,
    UserInterface
}

namespace ThreeDeePongProto.Shared.Managers
{
    [RequireComponent(typeof(PlayerInputManager))]
    public class UserInputManager : PersistentSingleton<UserInputManager>
    {
        [Header("Player Spawning")]
        [Tooltip("PlayerControllerBase with PlayerInputManager.")]
        [SerializeField] private GameObject m_playerController;
        [Tooltip("Visual AvatarControls-Prefabs for Player 0-3.")]
        [SerializeField] private GameObject[] m_playerAvatarControls;

        [Header("Settings")]
        [SerializeField] private bool m_joinByDefault = true;
        [SerializeField] private InputActionAsset m_originalActionAsset;
        
        private PlayerInputManager m_playerInputManager;
        private PlayerInputActions m_centralInputActions;

        internal static string SetActionMap { get => m_lastSetActionMap; }
        private static string m_lastSetActionMap;
        internal static int FocusedKeyboardPlayerID { get; private set; } = 0;  //Keep track of focused playerWindow. Standard PlayerID 0.

        #region Lists_and_Dictionaries
        private readonly List<PlayerInput> m_activePlayers = new();
        private List<InputDevice> m_availableGamepads = new();
        private readonly Dictionary<int, InputDevice> m_originalPlayerDevices = new();
        #endregion

        internal static event Action<string> AChangeActiveActionMap;            //Announce scheme-switch, so PlayerInput components can react.

        private const string m_keyboardID = "KeyboardPlayerID";
        private const string m_gamePadScheme = "Gamepad";

        internal string DefaultDeviceName { get => m_iconDisplayName; }
        private string m_iconDisplayName;
        private readonly string m_defaultDeviceName = "PS5 Controller";     //Standard, if no gamepad is connected.

        protected override void Awake()
        {
            base.Awake();

            m_lastSetActionMap = "";
            m_iconDisplayName = m_defaultDeviceName;

            m_centralInputActions ??= new();     //Old if (m_centralInputActions == null)
            m_centralInputActions.Enable();

            m_centralInputActions.PlayerActions.Disable();
            m_centralInputActions.UserInterface.Disable();

            m_playerInputManager = GetComponent<PlayerInputManager>();
            SetUpPlayerInputManager();
        }

        private void OnEnable()
        {
            if (m_playerInputManager != null)
            {
                m_playerInputManager.onPlayerJoined += OnPlayerJoined;
                m_playerInputManager.onPlayerLeft += OnPlayerLeft;
            }

            InputSystem.onDeviceChange += OnDeviceChange;

            m_centralInputActions.PlayerActions.SelectPlayer1.performed += _ => SetFocusedKeyboardPlayer(0);
            m_centralInputActions.PlayerActions.SelectPlayer2.performed += _ => SetFocusedKeyboardPlayer(1);
            m_centralInputActions.PlayerActions.SelectPlayer3.performed += _ => SetFocusedKeyboardPlayer(2);
            m_centralInputActions.PlayerActions.SelectPlayer4.performed += _ => SetFocusedKeyboardPlayer(3);
        }
        
        private void OnDisable()
        {
            if (m_playerInputManager != null)
            {
                m_playerInputManager.onPlayerJoined -= OnPlayerJoined;
                m_playerInputManager.onPlayerLeft -= OnPlayerLeft;
            }

            InputSystem.onDeviceChange -= OnDeviceChange;

            m_centralInputActions.PlayerActions.SelectPlayer1.performed -= _ => SetFocusedKeyboardPlayer(0);
            m_centralInputActions.PlayerActions.SelectPlayer2.performed -= _ => SetFocusedKeyboardPlayer(1);
            m_centralInputActions.PlayerActions.SelectPlayer3.performed -= _ => SetFocusedKeyboardPlayer(2);
            m_centralInputActions.PlayerActions.SelectPlayer4.performed -= _ => SetFocusedKeyboardPlayer(3);
        }

        private void OnApplicationQuit()
        {
            m_centralInputActions.Disable();
        }

        #region PlayerInputManager_Configuration
        private void SetUpPlayerInputManager()
        {
            if (m_playerInputManager == null)
            {
                m_playerInputManager = gameObject.AddComponent<PlayerInputManager>();
            }

            m_playerInputManager.notificationBehavior = PlayerNotifications.InvokeCSharpEvents;
            m_playerInputManager.joinBehavior = PlayerJoinBehavior.JoinPlayersWhenJoinActionIsTriggered;

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

        #region Spawn-Logic
        public void SpawnPlayersForMatch(int _playerCount)
        {
            //Create a new list of available Gamepads
            m_availableGamepads.Clear();
            foreach(var gamepad in Gamepad.all)
                m_availableGamepads.Add(gamepad);

            //Remove Gamepads that are already used by Players in the game.
            foreach (var player in m_activePlayers)
            {
                foreach (var device in player.devices)
                {
                    if (device is Gamepad)
                    {
                        m_availableGamepads.Remove(device);
                    }
                }
            }

            //Spawn the set amount of players.
            for (int i = 0; i < _playerCount; i++)
            {
                //If the player already exists, skip the index.
                if (m_activePlayers.Any(p => p.playerIndex == i))
                    continue;

                //Find the matching device and controlScheme for the player.
                InputDevice[] devicesToPair = GetDevicesForPlayer(i);
                string controlScheme = devicesToPair.Any(d => d is Gamepad) ? m_gamePadScheme : $"{m_keyboardID}{i}";
                //Spawn the player with it's assigned device.
                m_playerInputManager.JoinPlayer(i, -1, controlScheme, devicesToPair);
            }
        }

        private InputDevice[] GetDevicesForPlayer(int _playerIndex)
        {
            //Load current playerProfiles
            var m_playerProfiles = PlayerProfileManager.Instance.PlayerProfiles;    //Previous in SpawnPlayersForMatch().
            
            if (!m_playerProfiles[_playerIndex].DefaultKeyboard)
            {
                //The player prefers a gamepad. Check if one is available.
                if (m_availableGamepads.Count > 0)
                {
                    var gamepad = m_availableGamepads[0];
                    m_iconDisplayName = m_availableGamepads[0].displayName;          //Update controllerType to display gamepadIcons in Settings.
                    m_availableGamepads.RemoveAt(0); //And remove it from the list.
                    return new InputDevice[] { gamepad };
                }
            }

            //Fallback-Keyboard.
            return (Mouse.current != null) ? new InputDevice[] { Keyboard.current, Mouse.current }
                                    : new InputDevice[] { Keyboard.current };  //Else return keyboard.
        }
        #endregion

        #region Player-Participation_Behavior
        private void OnPlayerJoined(PlayerInput _playerInput)
        {
            //Keep the active playerCount!
            m_activePlayers.Add(_playerInput);

            int playerIndex = _playerInput.playerIndex;
            //Get the playerProfile for each player from the PlayerProfileManager.
            var playerProfile = PlayerProfileManager.Instance.PlayerProfiles[playerIndex];
            //Seting the original Asset, enables Hover-,Select- and Click actions on Mouse.Current, even if Player1 has gamepad paired. 
            //InputActionAsset clonedAsset = Instantiate(m_originalActionAsset);
            _playerInput.actions = m_originalActionAsset;
            //Set name of the playerController gameObject.
            _playerInput.gameObject.name = $"{playerProfile.PlayerName}";
            _playerInput.neverAutoSwitchControlSchemes = true;

            //Instantiate AvatarControls-Prefab gameObject.
            GameObject avatarInstance = null;
            if (m_playerAvatarControls != null && playerIndex < m_playerAvatarControls.Length)
            {
                avatarInstance = Instantiate(m_playerAvatarControls[playerIndex], _playerInput.transform);
            }

            //Get CharacterMainController on the CharacterBase-Prefab gameObject.
            if (avatarInstance != null && _playerInput.gameObject.TryGetComponent<CharacterMainController>(out var controller))
            {
                var controlData = SettingsManager.Instance.CurrentSettings.Control;
                var matchData = SettingsManager.Instance.CurrentSettings.Match;

                //Set Base-Prefab values.
                controller.StoreData(controlData, matchData, playerProfile);

                var transformParent = LocalMatchManager.Instance.PrefabParent;
                controller.transform.SetParent(transformParent);   //previous FindObjectOfType<LocalMatchManager>();
            }
            else
                Debug.LogError($"Could not find CharacterMainController for Player {playerIndex}.");

            //Keep track of original PlayerDevices set on start.
            if (_playerInput.devices.Count > 0)
                m_originalPlayerDevices[playerIndex] = _playerInput.devices[0];

            //Apply potentially existing keybind overrides on players.
            RebindManager.Instance.ApplyOverridesToPlayer(_playerInput);
            #if UNITY_EDITOR
            //Debug.Log($"Player {playerIndex} ({playerProfile.PlayerID}) joined with Device '{_playerInput.devices[0].displayName}'.");
            #endif
        }

        private void OnPlayerLeft(PlayerInput _playerInput)
        {
            m_activePlayers.Remove(_playerInput);
            m_originalPlayerDevices.Remove(_playerInput.playerIndex);
        }
        #endregion

        #region Device_Hot-Plugging
        private void OnDeviceChange(InputDevice _inputDevice, InputDeviceChange _deviceChange)
        {
            if (_inputDevice is not Gamepad gamepad)
                return;

            switch (_deviceChange)
            {
                case InputDeviceChange.Disconnected:
                case InputDeviceChange.Removed:
                    HandleDeviceDisconnect(gamepad);
                    break;

                case InputDeviceChange.Reconnected:
                case InputDeviceChange.Added:
                    HandleDeviceReconnect(gamepad);
                    break;
                default:
                    break;
            }
        }

        private void HandleDeviceDisconnect(Gamepad _disconnectedGamepad)
        {
            int playerIndex = -1;
            
            foreach (var pair in m_originalPlayerDevices)
            {
                if (pair.Value.deviceId == _disconnectedGamepad.deviceId)
                {
                    playerIndex = pair.Key;
                    break;
                }
            }

            if (playerIndex != -1)
            {
                var playerInput = m_activePlayers.Find(p => p.playerIndex == playerIndex);  //.Find or .FirstOrDefault.
                if (playerInput != null)
                {
                    #if UNITY_EDITOR
                    // Debug.Log($"Gamepad {_disconnectedGamepad.displayName} disconnected for Player {playerIndex}. Switching to Keyboard.");
                    #endif
                    //Switching to Fallback-Keyboard
                    playerInput.SwitchCurrentControlScheme($"{m_keyboardID}{playerIndex}", Keyboard.current, Mouse.current);
                }
            }
            else
            {
                //Remove unused Gamepad from the List.
                if (m_availableGamepads.Contains(_disconnectedGamepad))
                {
                    #if UNITY_EDITOR
                    // Debug.Log($"Removing unused Gamepad {_disconnectedGamepad.displayName} from the available Gamepad List!");
                    #endif
                    m_availableGamepads.Remove(_disconnectedGamepad);
                }
            }

            if (Gamepad.all.Count == 0)
                m_iconDisplayName = m_defaultDeviceName;
            else
                m_iconDisplayName = Gamepad.all[0].displayName;
        }

        private void HandleDeviceReconnect(Gamepad _reconnectedGamepad)
        {
            int playerIndex = -1;

            foreach (var pair in m_originalPlayerDevices)
            {
                if (pair.Value.deviceId == _reconnectedGamepad.deviceId)
                {
                    playerIndex = pair.Key;
                    break;
                }
            }

            if (playerIndex != -1)
            {
                var playerInput = m_activePlayers.Find(p => p.playerIndex == playerIndex);  //.Find or .FirstOrDefault
                //If the player currently runs on Keyboard-Fallback switch back.
                if (playerInput != null && !playerInput.devices.Contains(_reconnectedGamepad))
                {
                    #if UNITY_EDITOR
                    // Debug.Log($"Gamepad {_reconnectedGamepad.displayName} for Player {playerIndex} reconnected.");
                    #endif
                    playerInput.SwitchCurrentControlScheme(m_gamePadScheme, _reconnectedGamepad);
                    var defaultDevice = _reconnectedGamepad.displayName;
                }
            }
            else
            {
                //Add new Gamepad to List.
                if (!m_availableGamepads.Contains(_reconnectedGamepad))
                {
                    #if UNITY_EDITOR
                    // Debug.Log($"New unused Gamepad {_reconnectedGamepad.displayName} added. Action required!");
                    #endif
                    m_availableGamepads.Add(_reconnectedGamepad);
                }
            }

            m_iconDisplayName = _reconnectedGamepad.displayName;
        }
        #endregion

        public PlayerInputActions GetCentralActions()
        {
            return m_centralInputActions;
        }

        #region Helper_Methods
        /// <summary>
        /// Switches ActionMaps, if the active actionMap isn't equal to the submitted one. But does not disable the old actionMaps!
        /// </summary>
        /// <param name="_actionMap"></param>
        internal void ToggleActionMaps(string _actionMap)
        {
            if (m_lastSetActionMap == _actionMap)
                return;

            m_lastSetActionMap = _actionMap;

            m_centralInputActions.asset.FindActionMap(_actionMap).Enable();
            foreach (var map in m_centralInputActions.asset.actionMaps)
            {
                if (map.name != _actionMap)
                {
                    map.Disable();
                }
            }
            
            AChangeActiveActionMap?.Invoke(_actionMap);
        }

        private void SetFocusedKeyboardPlayer(int _playerFocusID)
        {
            if (FocusedKeyboardPlayerID != _playerFocusID)
                FocusedKeyboardPlayerID = _playerFocusID;
        }
        #endregion
    }
}