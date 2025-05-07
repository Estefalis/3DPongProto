using System;
using System.Collections.Generic;
using System.Linq;
using ThreeDeePongProto.Offline.UI.Menu;
using ThreeDeePongProto.Shared.HelperClasses;
using ThreeDeePongProto.Shared.InputActions;
using ThreeDeePongProto.Shared.PlayerCharacter;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

internal enum ESceneNames
{
    StartMenu = 0,
    LocalGame = 1,
    LanGame = 2,
    NetGame = 3
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
    public class UserInputManager : MonoBehaviour
    {
        //public static UserInputManager Instance { get; private set; }
        public static PlayerInputActions m_CentralActionsInstance;
        private readonly InputAction[] m_selectPlayers = new InputAction[4];

        [Header("References")]
        [SerializeField] private PlayerInputManager m_playerInputManager;
        [SerializeField] private bool m_joinByDefault = true;

        [Header("Settings")]
        [SerializeField] private int m_defaultPlayerNumber = 2; //Fallback.

        #region Scriptable_Objects
        [SerializeField] private MatchUIStates m_matchUIStates;
        [SerializeField] private MatchValues m_matchValues;
        #endregion

        private static EInputActionMaps m_newActiveActionMap = EInputActionMaps.None;
        internal static string SetActionMap { get => m_lastActionMap; }
        private static string m_lastActionMap;
        internal static int FocusedKeyboardPlayerID { get; private set; } = 0; //Keeps track of focused player. Standard PlayerID 0.

        #region Lists_and_Dictionaries
        private List<InputDevice> m_availableGamepads;
        private readonly Dictionary<int, InputDevice> m_playerOriginalDevice = new();
        private readonly Dictionary<int, string> m_playerOriginalScheme = new();
        private readonly Dictionary<int, bool> m_isPlayerUsingFallback = new();

        #endregion

        #region Actions_and_Functions
        internal static event Action<string> AChangeActiveActionMap;    //Announce scheme-switch, so PlayerInput components can react.
        #endregion

        private const string m_keyboardMouseScheme = "KeyboardMouse";
        private const string m_gamePadScheme = "Gamepad";

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

            if (m_CentralActionsInstance == null)
                m_CentralActionsInstance = new();

            m_CentralActionsInstance.Enable();
            m_selectPlayers[0] = m_CentralActionsInstance.PlayerActions.SelectPlayer1;
            m_selectPlayers[1] = m_CentralActionsInstance.PlayerActions.SelectPlayer2;
            m_selectPlayers[2] = m_CentralActionsInstance.PlayerActions.SelectPlayer3;
            m_selectPlayers[3] = m_CentralActionsInstance.PlayerActions.SelectPlayer4;

            m_playerInputManager = GetComponent<PlayerInputManager>();
            SetUpPlayerInputManager(m_playerInputManager);
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneManagerLoaded;
            MenuManager.AReLoadScene += OnReLoadScene;
            InputSystem.onDeviceChange += OnDeviceChange;

            EnablePlayerActionMap(true);
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneManagerLoaded;
            MenuManager.AReLoadScene -= OnReLoadScene;
            InputSystem.onDeviceChange -= OnDeviceChange;

            EnablePlayerActionMap(false);
        }

        private void Update()
        {
            if (m_CentralActionsInstance == null)
                return;

            for (int actionIndex = 0; actionIndex < m_selectPlayers.Length; actionIndex++)
            {
                if (m_selectPlayers[actionIndex] != null && m_selectPlayers[actionIndex].WasPressedThisFrame())
                {
                    SetFocusedKeyboardPlayer(actionIndex);
                }
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

        #region Custom_Methods
        private void SpawnPlayers()
        {
            if (m_playerInputManager == null)
            {
#if UNITY_EDITOR
                Debug.LogError("PlayerInputManager not set!", this);
#endif
                return;
            }

            if (m_matchValues == null)
            {
#if UNITY_EDITOR
                Debug.LogError("MatchValues ScriptableObject not set!", this);
#endif
                return;
            }

            int numberOfPlayers = (m_matchValues.PlayerSOData != null && m_matchValues.PlayerSOData.Count > 0)
                                ? m_matchValues.PlayerSOData.Count : m_defaultPlayerNumber;

            //List of available Gamepads.
            m_availableGamepads = new(Gamepad.all);

            for (int playerIndex = 0; playerIndex < numberOfPlayers; playerIndex++)
            {
                InputDevice[] devicesToPair = null;
                string controlScheme = "";

                devicesToPair = GetDevicesForPlayer(playerIndex);
                controlScheme = GetControlSchemeForPlayer(playerIndex, devicesToPair);

                #region Dictionary and Boolean-Section to handle gamepad re-connects.
                //When spawning player 'playerIndex' with device and controlScheme, store gamepad, null for keyboard origin.
                m_playerOriginalDevice[playerIndex] = (devicesToPair[0] is not Gamepad) ? null : devicesToPair[0];
                m_playerOriginalScheme[playerIndex] = controlScheme;
                m_isPlayerUsingFallback[playerIndex] = false;
                #endregion

                //Let Player join.
                if (devicesToPair != null && devicesToPair.Length > 0 && !string.IsNullOrEmpty(controlScheme))
                {
                    //Ensure that the devices are valid. (May be null at start.)
                    if (devicesToPair.Any(d => d == null))
                    {
#if UNITY_EDITOR
                        Debug.LogError($"Atleast one device is null. Player {playerIndex} can't join.");
#endif
                        //If the inputDevice is invalid and a Gamepad re-add it to the list of available Gamepads.
                        if (controlScheme == m_gamePadScheme && devicesToPair.Length > 0)
                            m_availableGamepads.Add(devicesToPair[0]);
                        continue;
                    }

                    PlayerInput joinedPlayerInput = m_playerInputManager.JoinPlayer(playerIndex, -1, controlScheme, devicesToPair);

                    if (joinedPlayerInput != null)
                    {
                        //Pass PlayerID to CharacterMainController script.
                        if (joinedPlayerInput.TryGetComponent<CharacterMainController>(out var playerController))
                        {
                            playerController.gameObject.name = $"Player{playerIndex}";
                            playerController.ReceivePlayerID(playerIndex);
                        }
                        else
                        {
#if UNITY_EDITOR
                            Debug.LogError($"No CharacterMainController script found on {joinedPlayerInput.gameObject} for Player {playerIndex}!");
#endif
                        }
                    }
                    else
                    {
#if UNITY_EDITOR
                        Debug.LogError($"An error occured on .JoinPlayer for Player {playerIndex}!");
#endif
                        //If the inputDevice is invalid and a Gamepad re-add it to the list of available Gamepads.
                        if (controlScheme == m_gamePadScheme && devicesToPair.Length > 0)
                            m_availableGamepads.Add(devicesToPair[0]);
                    }
                }
                else
                {
#if UNITY_EDITOR
                    Debug.LogWarning($"No valid device and/or controlScheme found for Player {playerIndex}.");
#endif
                }
            }
        }

        private void SetFocusedKeyboardPlayer(int _newPlayerFocus)
        {
            if (FocusedKeyboardPlayerID != _newPlayerFocus)
                FocusedKeyboardPlayerID = _newPlayerFocus;
        }

        private void HandleDeviceDisconnect(Gamepad _disconnectedGamepad)
        {
            PlayerInput disconnectedPlayerInput = null;
            int disconnectedPlayerIndex = -1;

            //Find player using this gamepad.
            foreach (PlayerInput pi in PlayerInput.all)
            {
                if (pi.devices.Contains(_disconnectedGamepad))
                {
                    disconnectedPlayerInput = pi;
                    disconnectedPlayerIndex = pi.playerIndex;
                    break;
                }
            }

            if (disconnectedPlayerInput != null)
            {
#if UNITY_EDITOR
                Debug.Log($"Gamepad '{_disconnectedGamepad.displayName}' disconnected from Player {disconnectedPlayerIndex}. Switching to keyboard.");
#endif
                //Store that this player is now using fallback.
                m_isPlayerUsingFallback[disconnectedPlayerIndex] = true;

                //Determine the correct Keyboard scheme.
                string keyboardScheme = $"KeyboardPlayerID{disconnectedPlayerIndex}";
                //Check if scheme exists. (important!)
                if (m_CentralActionsInstance.asset.FindControlScheme(keyboardScheme) == null)
                {
#if UNITY_EDITOR
                    Debug.LogError($"Fallback scheme '{keyboardScheme}' not found! Cannot switch player {disconnectedPlayerIndex} to keyboard.");
#endif
                    //Potentially disable input for this player entirely?
                    disconnectedPlayerInput.DeactivateInput(); //Or just leave them without device?
                    return;
                }

                //Get keyboard devices.
                List<InputDevice> keyboardDeviceList = new() { Keyboard.current };
                if (Mouse.current != null)
                    keyboardDeviceList.Add(Mouse.current);

                //Switch the player.
                disconnectedPlayerInput.SwitchCurrentControlScheme(keyboardScheme, keyboardDeviceList.ToArray());
            }
        }

        private void HandleDeviceReconnect(Gamepad _reconnectedGamepad)
        {
            int playerIndexToSwitchBack = -1;

            foreach (var kvp in m_playerOriginalDevice)
            {
                int playerIndex = kvp.Key;
                InputDevice originalDevice = kvp.Value;

                if (originalDevice != null)
                {
                    bool currentlyUsingFallback = m_isPlayerUsingFallback.TryGetValue(playerIndex, out bool usingFallback) && usingFallback;

                    //Check if original device matches AND player is currently using fallback.
                    if (originalDevice.deviceId == _reconnectedGamepad.deviceId && currentlyUsingFallback)
                    {
                        playerIndexToSwitchBack = playerIndex;
                        break;
                    }
                }
            }

            if (playerIndexToSwitchBack != -1)
            {
                //Find the PlayerInput for this playerIndex.
                PlayerInput playerToSwitchBack = PlayerInput.all.FirstOrDefault(pi => pi.playerIndex == playerIndexToSwitchBack);
                if (playerToSwitchBack != null)
                {
                    //No longer using fallback.
                    m_isPlayerUsingFallback[playerIndexToSwitchBack] = false;
                    //Use original controlScheme. ("Gamepad")
                    playerToSwitchBack.SwitchCurrentControlScheme(m_playerOriginalScheme[playerIndexToSwitchBack], _reconnectedGamepad);
                }
            }
            else
            {
                //Is the Gamepad already assigned to another player?
                bool alreadyAssigned = PlayerInput.all.Any(pi => pi.devices.Any(d => d.deviceId == _reconnectedGamepad.deviceId));
                //If the gamepad is not already assigned to a player, add it the 'm_availableGamepads'-list.
                if (!m_availableGamepads.Contains(_reconnectedGamepad))
                {
                    m_availableGamepads.Add(_reconnectedGamepad);
                    //TODO: Additional logic to assigning available Gamepads here, if needed?
                }
            }
        }
        #endregion

        #region None-CallbackContext_Subscription_Methods
        private void EnablePlayerActionMap(bool _enable)
        {
            if (m_CentralActionsInstance == null)
                return;

            for (int action = 0; action < m_selectPlayers.Length; action++)
            {
                switch (_enable)
                {
                    case true:
                    {
                        m_selectPlayers[action].Enable();
                        break;
                    }
                    case false:
                    {
                        m_selectPlayers[action].Disable();
                        break;
                    }
                }
            }
        }

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
                case 1:
                {
                    SpawnPlayers();
                    break;
                }
                default:
                    break;
            }
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

        private void OnDeviceChange(InputDevice _inputDevice, InputDeviceChange _deviceChange)
        {
            if (_inputDevice is Gamepad gamepad)
            {
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
        }
        #endregion

        #region Delegate-Methods
        private InputDevice[] GetDevicesForPlayer(int _playerIndex)
        {
            if (!m_matchValues.PlayerSOData[_playerIndex].DefaultKeyboard)
            {
                for (int uGp = 0; uGp < Gamepad.all.Count; uGp++)
                {
                    if (m_availableGamepads.Contains(Gamepad.all[uGp]))
                    {
                        m_availableGamepads.Remove(Gamepad.all[uGp]);
                        return new InputDevice[] { Gamepad.all[uGp] };    //If enough gamepads are available, return gamepad.
                    }
                }
            }
            return (Mouse.current != null) ? new InputDevice[] { Keyboard.current, Mouse.current }
                                    : new InputDevice[] { Keyboard.current };  //Else return keyboard.
        }

        private string GetControlSchemeForPlayer(int _playerIndex, InputDevice[] _devicesToPair)
        {
            if (_devicesToPair.Length > 0)
            {
                GetDeviceHelper.GetPlayerControlScheme(_playerIndex, _devicesToPair[0], out string controlScheme, out _);
                return controlScheme;
            }

            return m_keyboardMouseScheme;
        }
        #endregion

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