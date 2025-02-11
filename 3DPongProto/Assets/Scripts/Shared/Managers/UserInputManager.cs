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

        private int m_alreadySetGamepads;
        private Transform m_playfieldParent;
        private bool isInGameScene;  //True, while being in a GameScene.
        private const string m_keyboardDevice = "Keyboard", m_keyboardScheme = "KeyboardMouse";
        private const string m_gamepadDevice = "Gamepad", m_gamePadScheme = "Gamepad";

        private const string m_gameScene = "GameScene";

        private const string m_uiActionMap = "UserInterface";
        private MenuManager m_menuManager;
        private List<Gamepad> m_availableGamepads = new List<Gamepad>();

        #region Scriptable_Objects
        [SerializeField] private MatchUIStates m_matchUIStates;
        [SerializeField] private MatchValues m_matchValues;
        #endregion

        private void Awake()
        {
            #region Custom_InstanceSetup_as_Child
            //Singleton-Pattern für globalen Zugriff.
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }
            #endregion

            for (int i = 0; i < Gamepad.all.Count; i++)
                m_availableGamepads.Add(Gamepad.all[i]);
            m_alreadySetGamepads = 0;

            SceneManager.sceneLoaded += OnSceneLoaded;

            m_playerInputManager = GetComponent<PlayerInputManager>();
            SetUpPlayerInputManager(m_playerInputManager);
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            isInGameScene = scene.name.Contains(m_gameScene) ? isInGameScene = true : isInGameScene = false;

            switch (isInGameScene)
            {
                case true:
                {
                    RebindManager.ToggleActionMaps(RebindManager.m_PlayerInputActions.PlayerActions);
                    m_playfieldParent = FindObjectOfType<LocalMatchManager>().m_PlayfieldParent;   //Public getter => private Transform.
                    InstantiatePlayer();
                    break;
                }
                case false:
                {
                    RebindManager.ToggleActionMaps(RebindManager.m_PlayerInputActions.UserInterface);
                    CleanupPlayers();
                    SetMenuPlayerInput();
                    break;
                }
            }
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

        private void InstantiatePlayer()
        {
            uint playerCount = (uint)m_matchUIStates.EPlayerAmount;

            for (int i = 0; i < playerCount; i++)
            {
                GameObject playerAvatar = Instantiate(m_matchValues.PlayerSOData[i].Prefab, m_playfieldParent);

                if (!playerAvatar.TryGetComponent<PlayerInput>(out var playerInput))
                    playerInput = playerAvatar.AddComponent<PlayerInput>();

                ConfigurePlayerInput(playerInput, i);
            }
        }

        private void ConfigurePlayerInput(PlayerInput _playerInput, int _playerIndex)
        {
            //Load and set the InputActionAsset.
            if (_playerInput.actions == null)
                _playerInput.actions = Resources.Load<InputActionAsset>("InputActions/PlayerInputActions");

            //Determine, if gamepads are left to be set as device.
            int availableGamepads = Gamepad.all.Count - m_alreadySetGamepads;

            InputDevice assignedDevice;
            InputDevice mouseDevice = Mouse.current;

            string controlScheme;

            //Set Gamepad or Keyboard as InputDevice.
            switch (availableGamepads <= 0 || m_matchValues.PlayerSOData[_playerIndex].DefaultKeyboard)
            {
                //If no gamepad is available, or keyboard has priority for this player: Keyboard gets set as priority.
                case true:
                {
                    assignedDevice = Keyboard.current; //Player1 (WASD) | Player2 (Arrow-Keys) | Player3 (TFGH) | Player4 (IJKL).
                    controlScheme = m_keyboardScheme;
                    AssignKeyboard(_playerInput, _playerIndex, availableGamepads, assignedDevice, mouseDevice, controlScheme);
                    break;
                }
                case false: //'availableGamepads > 0' and not 'm_matchValues.PlayerSOData[_playerIndex].DefaultKeyboard'
                {
                    m_alreadySetGamepads += 1;

                    switch (availableGamepads > 0)
                    {
                        case true:
                        {
                            //If a gamepad is connected and keyboard has no priority for this player: Gamepad gets set.
                            for (int i = 0; i < m_availableGamepads.Count; i++)
                            {
                                assignedDevice = Gamepad.all[i];
                                m_availableGamepads.Remove(Gamepad.all[i]);
                                controlScheme = m_gamePadScheme;
                                AssignGamepad(_playerInput, _playerIndex, availableGamepads, assignedDevice, controlScheme);
                            }
                            break;
                        }
                        case false:
                        {
                            assignedDevice = Keyboard.current;
                            controlScheme = m_keyboardScheme;
                            AssignKeyboard(_playerInput, _playerIndex, availableGamepads, assignedDevice, mouseDevice, controlScheme);
                            break;
                        }
                    }

                    break;
                }
            }

            //Force activation of PlayerActions.
            _playerInput.SwitchCurrentActionMap("PlayerActions");
            _playerInput.neverAutoSwitchControlSchemes = false;
            //Set the notificationBehavior of the PlayerInput component.
            _playerInput.notificationBehavior = PlayerNotifications.InvokeCSharpEvents;
        }

        private static void AssignGamepad(PlayerInput _playerInput, int _playerIndex, int availableGamepads, InputDevice assignedDevice, string controlScheme)
        {
            _playerInput.SwitchCurrentControlScheme(controlScheme, new[] { assignedDevice });
            //PlayerInput.all[_playerIndex].SwitchCurrentControlScheme(controlScheme, new[] { assignedDevice });
            Debug.Log($"Player {_playerIndex + 1} uses a {assignedDevice?.name ?? "No"}-Device. TotalGamepads: {Gamepad.all.Count} | AvailableGamepads: {availableGamepads}");
        }

        private static void AssignKeyboard(PlayerInput _playerInput, int _playerIndex, int availableGamepads, InputDevice assignedDevice, InputDevice mouseDevice, string controlScheme)
        {
            _playerInput.SwitchCurrentControlScheme(controlScheme, new[] { assignedDevice, mouseDevice });
            //PlayerInput.all[_playerIndex].SwitchCurrentControlScheme(controlScheme, new[] { assignedDevice, mouseDevice });
            Debug.Log($"Player {_playerIndex + 1} uses a {assignedDevice?.name ?? "No"}-Device and a shared Mouse. TotalGamepads: {Gamepad.all.Count} | AvailableGamepads: {availableGamepads}");
        }

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