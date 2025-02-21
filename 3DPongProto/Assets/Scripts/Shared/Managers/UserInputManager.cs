using System;
using System.Collections;
using System.Collections.Generic;
using ThreeDeePongProto.Offline.UI.Menu;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace ThreeDeePongProto.Shared.Managers
{
    public class UserInputManager : MonoBehaviour
    {
        public static UserInputManager Instance { get; private set; }

        [SerializeField] private PlayerInputManager m_playerInputManager;
        [SerializeField] private bool m_joinByDefault = true;

        private MenuManager m_menuManager;

        private Transform m_playfieldParent;
        private bool isInGameScene;  //True, while being in a GameScene.

        private const string m_gameScene = "GameScene";
        private const string m_keyboardScheme = "KeyboardMouse", m_keyboardDevice = "Keyboard";
        private const string m_gamePadScheme = "Gamepad", m_gamepadDevice = "Gamepad";
        private const string m_uiActionMap = "UserInterface";

        private readonly List<InputDevice> m_usableGamepads = new();
        //private Dictionary<int, InputUser> m_playerUsers = new();

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
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

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

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            isInGameScene = scene.name.Contains(m_gameScene) ? isInGameScene = true : isInGameScene = false;

            switch (isInGameScene)
            {
                case true:
                {
                    //RebindManager.ToggleActionMaps(RebindManager.m_PlayerInputActions.PlayerActions);
                    m_playfieldParent = FindObjectOfType<LocalMatchManager>().m_PlayfieldParent;   //Public getter => private Transform.
                    InstantiatePlayer();
                    break;
                }
                case false:
                {
                    //RebindManager.ToggleActionMaps(RebindManager.m_PlayerInputActions.UserInterface);
                    CleanupPlayers();
                    SetMenuPlayerInput();
                    break;
                }
            }
        }

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
                //Debug.Log($"Player {np + 1} instantiated with {setControlScheme} ({string.Join(", ", assignedDevices.Select(d => d.name))})");

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

            string controlScheme = _assignedDevices[0] is Gamepad ? m_gamePadScheme : m_keyboardScheme;

            playerInput.SwitchCurrentActionMap("PlayerActions");
            StartCoroutine(SwitchControlSchemeNextFrame(playerInput, controlScheme, _assignedDevices));
            //Debug.Log($"Player {_playerIndex} owns PlayerID {playerInput.playerIndex} & received ID {_playerIndex} with active ActionMap: {playerInput.currentActionMap.name}");
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
            if (Gamepad.all.Count > _playerIndex)
            {
                return new InputDevice[] { Gamepad.all[_playerIndex] };    //If enough gamepads are available, return gamepad.
            }
            return new InputDevice[] { Keyboard.current, Mouse.current };  //Else return keyboard.
        }
        #endregion

        private void CleanupPlayers()
        {
            Debug.Log("Cleaning up players...");
            m_matchValues.PlayerSOData.Clear();
        }

        private void SetMenuPlayerInput()
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