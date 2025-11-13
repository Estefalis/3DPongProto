using System;
using System.Collections.Generic;
using System.Linq;
using ThreeDeePongProto.Offline.UI.Menu;
using ThreeDeePongProto.Shared.InputActions;
using ThreeDeePongProto.Shared.PlayerCharacter;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

internal enum ESceneNames
{
    BootScene = 0,
    StartMenu = 1,
    LocalGame = 2,
    LanGame = 3,
    NetGame = 4
}

internal enum EPlayerMenuControl
{
    None,
    SpecificPlayer,
    FirstPlayer,
    LastPlayer,
    EachPlayer,
    HostPlayer,
}

internal enum EInputActionMaps
{
    None,
    PlayerActions,
    UserInterface
}

namespace ThreeDeePongProto.Shared.Managers
{
    public class UserInputManager : PersistentSingleton<UserInputManager>
    {
        [Header("Player Spawning")]
        [Tooltip("PlayerControllerBase with PlayerInputManager.")]
        [SerializeField] private GameObject m_playerController;
        [Tooltip("Visual AvatarControls-Prefabs for Player 0-3.")]
        [SerializeField] private GameObject[] m_playerAvatarControls;

        [Header("References")]
        private PlayerInputManager m_playerInputManager;
        [SerializeField] private bool m_joinByDefault = true;
        private PlayerInputActions m_centralInputActions;

        internal static string SetActionMap { get => m_lastSetActionMap; }
        private static string m_lastSetActionMap;
        internal static int FocusedKeyboardPlayerID { get; private set; } = 0; //Keep track of focused playerWindow. Standard PlayerID 0.

        #region Lists_and_Dictionaries
        private readonly List<PlayerInput> m_activePlayers = new();
        private readonly Dictionary<int, InputDevice> m_originalPlayerDevices = new();

        private List<PlayerProfileData> m_playerProfiles;
        private List<InputDevice> m_availableGamepads;
        #endregion

        internal static event Action<string> AChangeActiveActionMap;    //Announce scheme-switch, so PlayerInput components can react.

        private const string m_keyboardID = "KeyboardPlayerID";
        private const string m_gamePadScheme = "Gamepad";

        protected override void Awake()
        {
            base.Awake();

            m_lastSetActionMap = "";

            m_centralInputActions ??= new();     //Old if (m_centralInputActions == null)
            m_centralInputActions.Enable();

            m_centralInputActions.PlayerActions.Disable();
            m_centralInputActions.UserInterface.Disable();

            m_playerInputManager = GetComponent<PlayerInputManager>();
            SetUpPlayerInputManager(m_playerInputManager);
        }

        private void OnEnable()
        {
            m_playerInputManager.onPlayerJoined += OnPlayerJoined;
            m_playerInputManager.onPlayerLeft += OnPlayerLeft;

            MenuManager.AReLoadScene += OnReLoadScene;
            InputSystem.onDeviceChange += OnDeviceChange;
            m_centralInputActions.PlayerActions.SelectPlayer1.performed += SetPlayerOne;
            m_centralInputActions.PlayerActions.SelectPlayer2.performed += SetPlayerTwo;
            m_centralInputActions.PlayerActions.SelectPlayer3.performed += SetPlayerThree;
            m_centralInputActions.PlayerActions.SelectPlayer4.performed += SetPlayerFour;
        }
        
        private void OnDisable()
        {
            m_playerInputManager.onPlayerJoined -= OnPlayerJoined;
            m_playerInputManager.onPlayerLeft -= OnPlayerLeft;

            MenuManager.AReLoadScene -= OnReLoadScene;
            InputSystem.onDeviceChange -= OnDeviceChange;
            m_centralInputActions.PlayerActions.SelectPlayer1.performed -= SetPlayerOne;
            m_centralInputActions.PlayerActions.SelectPlayer2.performed -= SetPlayerTwo;
            m_centralInputActions.PlayerActions.SelectPlayer3.performed -= SetPlayerThree;
            m_centralInputActions.PlayerActions.SelectPlayer4.performed -= SetPlayerFour;
        }

        private void OnApplicationQuit()
        {
            m_centralInputActions.Disable();
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

        #region Custom_Methods
        private void SetFocusedKeyboardPlayer(int _playerFocusID)
        {
            if (FocusedKeyboardPlayerID != _playerFocusID)
                FocusedKeyboardPlayerID = _playerFocusID;
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
                var playerInput = m_activePlayers.Find(p => p.playerIndex == playerIndex);
                if (playerInput != null)
                {
                    Debug.Log($"Gamepad disconnected for Player {playerIndex}. Switching to Keyboard.");
                    //Switching to Fallback-Keyboard
                    playerInput.SwitchCurrentControlScheme($"{m_keyboardID}{playerIndex}", Keyboard.current, Mouse.current);
                }
            }
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
                var playerInput = m_activePlayers.Find(p => p.playerIndex == playerIndex);
                // Prüfe, ob der Spieler aktuell wirklich ein anderes Gerät benutzt
                if (playerInput != null && !playerInput.devices.Contains(_reconnectedGamepad))
                {
                    Debug.Log($"Gamepad reconnected for Player {playerIndex}.");
                    playerInput.SwitchCurrentControlScheme(m_gamePadScheme, _reconnectedGamepad);
                }
            }
        }
        #endregion

        #region None-CallbackContext_Subscription_Methods
        private void OnPlayerJoined(PlayerInput _playerInput)
        {
            //Keep the active playerCount!
            m_activePlayers.Add(_playerInput);
            int playerIndex = _playerInput.playerIndex;

            //Get the playerProfile for each player from the PlayerProfileManager.
            PlayerProfileData profile = PlayerProfileManager.Instance.PlayerProfiles[playerIndex];

            //Set name of the playerController gameObject.
            _playerInput.gameObject.name = $"{profile.PlayerName}";

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
                controller.StoreData(controlData, matchData, profile);

                var transformParent = LocalMatchManager.Instance.PrefabParent;
                controller.transform.SetParent(transformParent);   //previous FindObjectOfType<LocalMatchManager>();
            }
            else
                Debug.LogError($"Could not find CharacterMainController for Player {playerIndex}.");

            //Keep track of original PlayerDevices set on start.
            m_originalPlayerDevices[playerIndex] = _playerInput.devices[0];

            //Apply potentially existing keybind overrides on players.
            RebindManager.Instance.ApplyOverridesToPlayer(_playerInput);

            //Debug.Log($"Player {playerIndex} ({profile.PlayerID}) joined with Device '{_playerInput.devices[0].displayName}'.");
        }

        private void OnPlayerLeft(PlayerInput _playerInput)
        {
            m_activePlayers.Remove(_playerInput);
            m_originalPlayerDevices.Remove(_playerInput.playerIndex);
        }

        /// <summary>
        /// Method to receive the buildIndex of the scene that shall be reloaded.
        /// </summary>
        /// <param name="_sceneIndex"></param>
        private void OnReLoadScene(int _sceneIndex)
        {
            if (_sceneIndex > 0) //Exclude BootScene with DDOL-Managers.
            {
                if (_sceneIndex < SceneManager.sceneCountInBuildSettings - 1)   //-1 marks last SceneIndex.
                    SceneManager.LoadScene(_sceneIndex);
            }
        }

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
        #endregion

        #region Delegate-Methods
        private InputDevice[] GetDevicesForPlayer(int _playerIndex)
        {
            if (!m_playerProfiles[_playerIndex].DefaultKeyboard)
            {
                //The player prefers a gamepad. Check if one is available.
                if (m_availableGamepads.Count > 0)
                {
                    var gamepad = m_availableGamepads[0];
                    m_availableGamepads.RemoveAt(0); //And remove it from the list.
                    return new InputDevice[] { gamepad };
                }
                else
                {
                    //Fallback-Keyboard.
                    //Debug.LogWarning($"No gamepad available. Assigning available Keyboard and Mouse to Player {_playerIndex + 1}.");
                }
            }

            return (Mouse.current != null) ? new InputDevice[] { Keyboard.current, Mouse.current }
                                    : new InputDevice[] { Keyboard.current };  //Else return keyboard.
        }
        #endregion

        #region Change_Action_Maps
        /// <summary>
        /// Switches ActionMaps, if the active actionMap isn't equal to the submitted one. But does not disable the old actionMaps!
        /// </summary>
        /// <param name="_actionMap"></param>
        internal /*static */void ToggleActionMaps(string _actionMap)
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

            //foreach (InputActionMap actionMap in m_centralInputActions.asset.actionMaps)
            //{
            //    if (actionMap.name != _actionMap)
            //        actionMap.Disable();
            //    else
            //        actionMap.Enable();
            //}

            AChangeActiveActionMap?.Invoke(_actionMap);
        }
        #endregion

        public void SpawnPlayersForMatch(int _playerCount)
        {
            //Load current playerProfiles
            m_playerProfiles = PlayerProfileManager.Instance.PlayerProfiles;

            //Create a new list of available Gamepads
            m_availableGamepads = new List<InputDevice>(Gamepad.all);

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
                {
                    continue;
                }

                //Find the matching device and controlScheme for the player.
                InputDevice[] devicesToPair = GetDevicesForPlayer(i);
                string controlScheme = devicesToPair.Any(d => d is Gamepad) ? m_gamePadScheme : $"{m_keyboardID}{i}";
                //Spawn the player with it's assigned device.
                m_playerInputManager.JoinPlayer(i, -1, controlScheme, devicesToPair);
            }
        }

        public PlayerInputActions GetCentralActions()
        {
            return m_centralInputActions;
        }

        private void SetPlayerOne(InputAction.CallbackContext _callbackContext)
        {
            SetFocusedKeyboardPlayer(0);
        }

        private void SetPlayerTwo(InputAction.CallbackContext _callbackContext)
        {
            SetFocusedKeyboardPlayer(1);
        }

        private void SetPlayerThree(InputAction.CallbackContext _callbackContext)
        {
            SetFocusedKeyboardPlayer(2);
        }

        private void SetPlayerFour(InputAction.CallbackContext _callbackContext)
        {
            SetFocusedKeyboardPlayer(3);
        }
    }
}