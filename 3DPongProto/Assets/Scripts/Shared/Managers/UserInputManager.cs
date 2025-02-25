using System;
using System.Collections;
using System.Collections.Generic;
using ThreeDeePongProto.Offline.UI.Menu;
using ThreeDeePongProto.Shared.PlayerCharacter;
using UnityEngine;
using UnityEngine.InputSystem;
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
        public static UserInputManager Instance { get; private set; }

        [SerializeField] private PlayerInputManager m_playerInputManager;
        [SerializeField] private bool m_joinByDefault = true;

        private MenuManager m_menuManager;
        private Transform m_playfieldParent;

        private const string m_keyboardScheme = "KeyboardMouse", m_keyboardDevice = "Keyboard";
        private const string m_gamePadScheme = "Gamepad", m_gamepadDevice = "Gamepad";
        private const string m_uiActionMap = "UserInterface";

        private readonly List<InputDevice> m_usableGamepads = new();
        public static event Action<int> ACheckForPlayerInput;

        #region Scriptable_Objects
        [SerializeField] private MatchUIStates m_matchUIStates;
        [SerializeField] private MatchValues m_matchValues;
        #endregion

        private void Awake()
        {
            #region Singleton_pattern
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
            MenuManager.AResumeTheGame += ResetActionMap;
            CharacterInputHandler.AMenuOpens += OnMenuOpens;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneManagerLoaded;
            MenuManager.AReLoadScene -= OnReLoadScene;
            MenuManager.AResumeTheGame -= ResetActionMap;
            CharacterInputHandler.AMenuOpens -= OnMenuOpens;
        }

        //public void DebugDevices()
        //{
        //    //Debug.Log("Debugging InputSystem...");

        //    foreach (var player in PlayerInput.all)
        //    {
        //        Debug.Log($"Player {player.playerIndex} -> Device: {player.devices[0].name}, ControlScheme: {player.currentControlScheme}");
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

        #region On_Scene_Reload_Or_Resume
        /// <summary>
        /// Method to react after a scene has been fully loaded beforehand.
        /// </summary>
        /// <param name="scene"></param>
        /// <param name="mode"></param>
        private void OnSceneManagerLoaded(Scene scene, LoadSceneMode mode)
        {
            int sceneIndex = scene.buildIndex;
            ResetActionMap(sceneIndex);

            switch (sceneIndex)
            {
                case 0:
                {
                    CleanupPlayers();
                    SetMenuInput();
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
        }

        private void OnMenuOpens()
        {
            ResetActionMap((int)ESceneNames.StartMenu); //Abusing the StartMenu enum entry to toggle UserInterface ActionMap! Am lazy. <(o.o)>
        }

        /// <summary>
        /// Method to reSet the active ActionMap, depending on resuming to the game from the pauseMenu, or on scene reLoads.
        /// </summary>
        /// <param name="_index"></param>
        private void ResetActionMap(int _index)
        {
            //TODO: Move ToggleActionMaps method into UserInputManager!
            switch (_index)
            {
                case 0:
                    RebindManager.ToggleActionMaps(RebindManager.m_PlayerInputActions.UserInterface);
                    break;
                case 1:
                case 2:
                case 3:
                    RebindManager.ToggleActionMaps(RebindManager.m_PlayerInputActions.PlayerActions);
                    break;
                default:
                    Debug.Log("Scene is not implemented, yet!");
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
        #endregion

        #region Instantiate_and_configurate_Player_and_PlayerInput
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
                //Debug.Log($"Player {np} assigned device(s): {string.Join(", ", assignedDevices.Select(d => d.name))}");

                //Waits a frame until old _playerInput component is destroyed.
                StartCoroutine(DelayedPlayerInputSetup(newPlayer, np, assignedDevices));
            }
        }

        #region NewPlayerInput-Setup
        private IEnumerator DelayedPlayerInputSetup(GameObject _playerPrefab, int _playerIndex, InputDevice[] _assignedDevices)
        {
            yield return null;  //Wait until next frame.

            ConfigureNewPlayerInput(_playerPrefab, _playerIndex, _assignedDevices);
        }

        private void ConfigureNewPlayerInput(GameObject _playerPrefab, int _playerIndex, InputDevice[] _assignedDevices)
        {
            if (!_playerPrefab.TryGetComponent<PlayerInput>(out var playerInput))
            {
                Debug.LogError($"Player {_playerIndex}: No PlayerInput found!");
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
                Debug.LogWarning($"Player {_playerIndex} has no assigned device! Checking for available Gamepad...");
                if (Gamepad.all.Count > 0)
                {
                    _assignedDevices = new InputDevice[] { Gamepad.all[0] };
                    Debug.Log($"Player {_playerIndex} assigned to Gamepad: {Gamepad.all[0].name}");
                }
                else
                {
                    Debug.LogError($"No Gamepad found for Player {_playerIndex}, falling back to Keyboard.");
                    _assignedDevices = new InputDevice[] { Keyboard.current };
                }
            }
            Debug.Log($"curCtrlScheme: {playerInput.currentControlScheme} | User: {playerInput.user} | pairedDevice: {playerInput.user.pairedDevices.Count}");
            //string controlScheme = _assignedDevices[0] is Gamepad ? m_gamePadScheme : m_keyboardScheme;
            playerInput.SwitchCurrentActionMap("PlayerActions");
            playerInput.ActivateInput();

            StartCoroutine(SwitchControlSchemeNextFrame(playerInput, playerInput.currentControlScheme, _assignedDevices));
            //Debug.Log($"Player {_playerIndex} with Device: {_assignedDevices[0].name}, ControlScheme: {controlScheme} & active ActionMap: {playerInput.currentActionMap.name}");
            ACheckForPlayerInput?.Invoke(_playerIndex);
        }

        private IEnumerator SwitchControlSchemeNextFrame(PlayerInput _playerInput, string _controlScheme, InputDevice[] _assignedDevices)
        {
            yield return null;   //Wait until next frame to ensure, that PlayerInput is registered right.

            if (_playerInput.user.valid)
            {
                _playerInput.SwitchCurrentControlScheme(_controlScheme, _assignedDevices);
                _playerInput.ActivateInput();   //Forces the inputsystem to use the assigned device.
                //Debug.Log($"Player {_playerInput.playerIndex + 1}: ControlScheme: {_controlScheme} - Device(s): {string.Join(", ", _assignedDevices.Select(d => d.name))}.");
            }
            else
            {
                Debug.LogError($"No valid user on Player {_playerInput.playerIndex + 1}!");
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
        #endregion

        private void CleanupPlayers()
        {
            Debug.Log("Cleaning up players...");
            m_matchValues.PlayerSOData.Clear();
        }

        private void SetMenuInput()
        {
            m_menuManager = FindObjectOfType<MenuManager>();
            m_menuManager.TryGetComponent<PlayerInput>(out var menuInput);

            if (menuInput == null)
            {
                menuInput = m_menuManager.gameObject.AddComponent<PlayerInput>();
                menuInput.actions = Resources.Load<InputActionAsset>("InputActions/PlayerInputActions");
                menuInput.defaultActionMap = m_uiActionMap;
                //Enable active ControlScheme switch.
                menuInput.neverAutoSwitchControlSchemes = false;
                //Set the notificationBehavior of the PlayerInput component.
                menuInput.notificationBehavior = PlayerNotifications.InvokeCSharpEvents;
            }

            menuInput.SwitchCurrentActionMap(m_uiActionMap);
        }
    }
}