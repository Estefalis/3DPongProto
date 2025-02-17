using System;
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
        public static event Action<int> ACheckForPlayerInput;
        #region boll_array_idea...
        private InputDevice m_gamepadToRemove;
        private readonly bool[] m_setDevices = new bool[4];
        #endregion

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
                DontDestroyOnLoad(gameObject);
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
            m_usableGamepads.Add(GetConnectedGamepads());
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
            //bool setKeyboard;

            for (int p = 0; p < playerCount; p++)
            {
                #region boll_array_idea...
                //setKeyboard = m_matchValues.PlayerSOData[p].DefaultKeyboard;
                //switch (setKeyboard)
                //{
                //    case true:
                //    {
                //        m_setDevices[p] = true;
                //        break;
                //    }
                //    case false:
                //    {
                //        switch (m_usableGamepads.Count <= 0)
                //        {
                //            case true:
                //            {
                //                m_setDevices[p] = true;   //No gamepads available = setKeyboard == true;
                //                break;
                //            }
                //            case false:
                //            {
                //                m_setDevices[p] = false;   //Gamepads available = setKeyboard == false;
                //                m_gamepadToRemove = m_usableGamepads[0];
                //                m_usableGamepads.Remove(m_gamepadToRemove);
                //                break;
                //            }
                //        }
                //        break;
                //    }
                //} 
                #endregion

                GameObject playerPrefab = Instantiate(m_matchValues.PlayerSOData[p].Prefab, m_playfieldParent);
                if (!playerPrefab.TryGetComponent<PlayerInput>(out var playerInput))
                    playerInput = playerPrefab.AddComponent<PlayerInput>();

                ConfigurePlayerInput(playerInput, p);
            }
        }

        private void ConfigurePlayerInput(PlayerInput _playerInput, int _playerIndex)
        {
            //Load and set the InputActionAsset.
            if (_playerInput.actions == null)
                _playerInput.actions = Resources.Load<InputActionAsset>("InputActions/PlayerInputActions");

            InputDevice assignedDevice = Keyboard.current;  //Player1 (WASD) | Player2 (Arrow-Keys) | Player3 (TFGH) | Player4 (IJKL).
            InputDevice mouseDevice = Mouse.current;
            string controlScheme = m_keyboardScheme;

            bool keyboardAsDefault = m_matchValues.PlayerSOData[_playerIndex].DefaultKeyboard;

            if (!keyboardAsDefault)             //DefaultKeyboard == false!
            {
                if (m_usableGamepads.Count > 0)  //If atleast one gamepad is registered...
                {
                    assignedDevice = GetAvailableGamepad();    //...get the first available gamepad and remove it from the list.                    
                    controlScheme = m_gamePadScheme;

                    _playerInput.SwitchCurrentControlScheme(controlScheme, new[] { assignedDevice });
                    Debug.Log($"Player {_playerIndex + 1} uses a {assignedDevice?.name ?? "No"}-Device and a shared Mouse. TotalGamepads: {Gamepad.all.Count} | UsableGamepads: {m_usableGamepads.Count} | KeyboardAsDefault: {keyboardAsDefault}");
                }
                else
                {
                    _playerInput.SwitchCurrentControlScheme(controlScheme, new[] { assignedDevice, mouseDevice });
                    Debug.Log($"Player {_playerIndex + 1} uses a {assignedDevice?.name ?? "No"}-Device and a shared Mouse. TotalGamepads: {Gamepad.all.Count} | UsableGamepads: {m_usableGamepads.Count} | KeyboardAsDefault: {keyboardAsDefault}");
                }
            }
            else
            {
                _playerInput.SwitchCurrentControlScheme(controlScheme, new[] { assignedDevice, mouseDevice });
                Debug.Log($"Player {_playerIndex + 1} uses a {assignedDevice?.name ?? "No"}-Device and a shared Mouse. TotalGamepads: {Gamepad.all.Count} | UsableGamepads: {m_usableGamepads.Count} | KeyboardAsDefault: {keyboardAsDefault}");
            }

            //Force activation of PlayerActions.
            _playerInput.SwitchCurrentActionMap("PlayerActions");
            _playerInput.neverAutoSwitchControlSchemes = false;
            //Set the notificationBehavior of the PlayerInput component.
            _playerInput.notificationBehavior = PlayerNotifications.InvokeCSharpEvents;

            ACheckForPlayerInput?.Invoke(_playerIndex);
        }
        #endregion

        #region Delegate_Methods
        private InputDevice GetConnectedGamepads()
        {
            for (int i = 0; i < Gamepad.all.Count; i++)
            {
                if (Gamepad.all[i] != null && !m_usableGamepads.Contains(Gamepad.all[i]))
                {
                    return Gamepad.all[i];
                }
            }
            return null;
        }

        private InputDevice GetAvailableGamepad()
        {
            for (int j = 0; j < m_usableGamepads.Count; j++)
            {
                if (Gamepad.all[j] != null && m_usableGamepads.Contains(Gamepad.all[j]))
                {
                    m_usableGamepads.Remove(Gamepad.all[j]);
                    return Gamepad.all[j];
                }
            }
            return null;
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