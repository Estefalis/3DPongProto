using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ThreeDeePongProto.Offline.UI.Menu;
using ThreeDeePongProto.Shared.PlayerCharacter;
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

        private Transform m_playfieldParent;

        private const string m_keyboardMouse = "KeyboardMouse", m_keyboardSchemePID0 = "KeyboardPlayerID0", m_keyboardSchemePID1 = "KeyboardPlayerID1", m_keyboardSchemePID2 = "KeyboardPlayerID2", m_keyboardSchemePID3 = "KeyboardPlayerID3", m_keyboardDevice = "Keyboard";
        private const string m_gamePadScheme = "Gamepad", m_gamePadSchemePID0 = "GamepadPlayerID0", m_gamePadSchemePID1 = "GamepadPlayerID1", m_gamePadSchemePID2 = "GamepadPlayerID2", m_gamePadSchemePID3 = "GamepadPlayerID3", m_gamepadDevice = "Gamepad";

        private const string m_playerActionMap = "PlayerActions";
        private string m_currentControlScheme;
        private static string m_lastActionMap;

        private static ActiveInputActionMap m_newActiveActionMap = ActiveInputActionMap.None;
        
        private InputDevice m_lastUsedDevice;
        //private readonly Dictionary<int, InputDevice[]> m_originalPlayerDevices = new();

        internal static Dictionary<int, int> PlayerControllerMap => m_playerControllerMap;
        private static readonly Dictionary<int, int> m_playerControllerMap = new();
        private readonly List<InputDevice> m_usableGamepads = new();

        internal static event Action<int> ACheckForPlayerInput;
        internal static event Action<string, InputDevice[]> AOnDeviceInput;
        internal static event Action<string> AChangeActiveActionMap;

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
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneManagerLoaded;
            MenuManager.AReLoadScene += OnReLoadScene;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneManagerLoaded;
            MenuManager.AReLoadScene -= OnReLoadScene;
        }

        private void Update()
        {
            //Keyboard (and Mouse) Input check.
            if (Keyboard.current.anyKey.wasPressedThisFrame || Mouse.current.delta.ReadValue() != Vector2.zero)
            {
                CustomControlSchemeSwitch(m_keyboardMouse, new InputDevice[] { Keyboard.current, Mouse.current });
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

        private void SetInitialInput()
        {
            CustomControlSchemeSwitch(m_keyboardMouse, new InputDevice[] { Keyboard.current, Mouse.current });
        }

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

            //Mapping playerController.m_playerID to the fix playerInput.user.index, to bypass changing indices on varying playerInput components.
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
            
            //m_originalPlayerDevices.Add(playerInput.playerIndex, _assignedDevices);
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
            Debug.Log("Cleaning up players and inputs...");
            m_matchValues.PlayerSOData.Clear();
            //m_originalPlayerDevices.Clear();
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
                    //Debug.Log($"Switched to: {_newControlScheme} | Devices: {string.Join(", ", _newDevices.Select(d => d.name))}");

                    break;
                }
            }
        }
    }
}