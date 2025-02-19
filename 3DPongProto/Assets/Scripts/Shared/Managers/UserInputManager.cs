using System;
using System.Collections;
using System.Collections.Generic;
using ThreeDeePongProto.Offline.UI.Menu;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using UnityEngine.SceneManagement;

namespace ThreeDeePongProto.Shared.Managers
{
    public class UserInputManager : MonoBehaviour
    {
        private enum PlayerInputUsage
        {
            KeepOriginal,
            CreateNew
        }
        [SerializeField] private PlayerInputUsage m_playerInputUsage = PlayerInputUsage.CreateNew;

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
        private Dictionary<int, InputUser> m_playerUsers = new();

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

            switch (m_playerInputUsage)
            {
                case PlayerInputUsage.KeepOriginal:
                {
                    for (int pk = 0; pk < playerCount; pk++)
                    {
                        GameObject playerPrefab = Instantiate(m_matchValues.PlayerSOData[pk].Prefab, m_playfieldParent);
                        if (!playerPrefab.TryGetComponent<PlayerInput>(out var playerInput))
                            playerInput = playerPrefab.AddComponent<PlayerInput>();

                        ConfigureExistingPlayerInput(playerInput, pk);
                    }
                    break;
                }
                case PlayerInputUsage.CreateNew:
                {
                    for (int pn = 0; pn < playerCount; pn++)
                    {
                        InputDevice assignedDevice = GetDeviceForPlayer(pn);
                        string setControlScheme = assignedDevice is Gamepad ? m_gamePadScheme : m_keyboardScheme;

                        PlayerInput newPlayerInput = PlayerInput.Instantiate(m_matchValues.PlayerSOData[pn].Prefab, controlScheme: setControlScheme, pairWithDevice: assignedDevice);

                        GameObject newPlayer = newPlayerInput.gameObject;
                        newPlayer.transform.SetParent(m_playfieldParent);
                        Debug.Log($"✅ Player {pn + 1} instantiated with {setControlScheme} ({assignedDevice?.name ?? "No device!"})");

                        //Waits a frame until old _playerInput component is destroyed.
                        StartCoroutine(DelayedPlayerInputSetup(newPlayer, pn, assignedDevice));
                    }
                    break;
                }
            }
        }

        private void ConfigureExistingPlayerInput(PlayerInput _playerInput, int _playerIndex)
        {
            //Load and set the InputActionAsset.
            if (_playerInput.actions == null)
                _playerInput.actions = Resources.Load<InputActionAsset>("InputActions/PlayerInputActions");

            InputDevice assignedDevice = Keyboard.current;  //Player1 (WASD) | Player2 (Arrow-Keys) | Player3 (TFGH) | Player4 (IJKL).
            InputDevice mouseDevice = Mouse.current;
            string controlScheme = m_keyboardScheme;

            bool keyboardAsDefault = m_matchValues.PlayerSOData[_playerIndex].DefaultKeyboard || m_usableGamepads.Count < 1;

            if (!keyboardAsDefault)             //DefaultKeyboard == false!
            {
                if (Gamepad.all.Count > 0)  //If atleast one gamepad is registered...
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

        #region NewPlayerInput-Setup
        private IEnumerator DelayedPlayerInputSetup(GameObject _playerPrefab, int _playerIndex, InputDevice _assignedDevice)
        {
            yield return null;  //Wait until next frame.

            ConfigureNewPlayerInput(_playerPrefab, _playerIndex, _assignedDevice);
        }

        private void ConfigureNewPlayerInput(GameObject _playerPrefab, int _playerIndex, InputDevice _assignedDevice)
        {
            if (!_playerPrefab.TryGetComponent<PlayerInput>(out var playerInput))
            {
                Debug.LogError($"Player {_playerIndex}: No PlayerInput found!");
                return;
            }

            //Load and set the InputActionAsset.
            playerInput.actions = Resources.Load<InputActionAsset>("InputActions/PlayerInputActions");
            //How the PlayerInput component invokes events.
            playerInput.notificationBehavior = PlayerNotifications.InvokeCSharpEvents;

            //Keep no Input-Device allocation. (Do not force Gamepad allocation!)
            playerInput.defaultControlScheme = null;
            playerInput.neverAutoSwitchControlSchemes = true;

            string controlScheme = _assignedDevice is Gamepad ? m_gamePadScheme : m_keyboardScheme;
            Debug.Log($"🎮 Player {_playerIndex + 1} uses {controlScheme} mit {_assignedDevice?.name ?? "kein Gerät"}");

            //InputDevice[] assignedDevices;
            //if (Gamepad.all.Count > _playerIndex)
            //{
            //    //Allocate gamepad to _playerPrefab, if one is available.
            //    assignedDevices = new InputDevice[] { Gamepad.all[_playerIndex] };
            //    controlScheme = m_gamePadScheme;
            //}
            //else
            //{
            //    //Else allocate keyboard and mouse.
            //    assignedDevices = new InputDevice[] { Keyboard.current, Mouse.current };
            //    controlScheme = m_keyboardScheme;
            //}

            ////Manual Pairing with InputUser.
            //if (!playerInput.user.valid)
            //{
            //    //Pair user with device and save it in a dictionary.
            //    InputUser newUser = InputUser.PerformPairingWithDevice(assignedDevices[0]);

            //    if (m_playerUsers.ContainsKey(_playerIndex))
            //        m_playerUsers[_playerIndex] = newUser;      //If the user already exists, overwrite it.
            //    else
            //        m_playerUsers.Add(_playerIndex, newUser);   //Else add the entry.

            //    Debug.Log($"✅ Player {_playerIndex + 1} owns UserID {newUser.index} with Device {assignedDevices[0]?.name}.");
            //}

            //Force activation of PlayerActions.
            playerInput.SwitchCurrentActionMap("PlayerActions");
            ////Set _controlScheme without "Invalid user"-error.
            //_playerInput.SwitchCurrentControlScheme(_controlScheme, assignedDevices);
            StartCoroutine(SwitchControlSchemeNextFrame(playerInput, controlScheme, _assignedDevice));

            ACheckForPlayerInput?.Invoke(_playerIndex);
        }

        private IEnumerator SwitchControlSchemeNextFrame(PlayerInput _playerInput, string _controlScheme, InputDevice _assignedDevice)
        {
            yield return null;   //Wait until next frame to ensure, that PlayerInput is registered right.

            if (_playerInput.user.valid)
            {
                _playerInput.SwitchCurrentControlScheme(_controlScheme, new InputDevice[] { _assignedDevice });
                Debug.Log($"✅ Player {_playerInput.playerIndex + 1}: Control Scheme = {_controlScheme}, Device = {_assignedDevice?.name}");
            }
            else
            {
                Debug.LogError($"❌ No valid user on Player {_playerInput.playerIndex + 1}!");
            }
        }
        #endregion
        #endregion

        #region Delegate_Methods
        private InputDevice GetDeviceForPlayer(int playerIndex)
        {
            if (Gamepad.all.Count > playerIndex)
            {
                return Gamepad.all[playerIndex];    //If enough gamepads are available, return gamepad.
            }
            return Keyboard.current;                //Else return keyboard.
        }

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
            for (int j = 0; j < Gamepad.all.Count; j++)
            {
                if (Gamepad.all[j] != null && m_usableGamepads.Contains(Gamepad.all[j]))
                {
                    m_usableGamepads.Remove(Gamepad.all[j]);
                    return Gamepad.all[j];
                }
            }
            return null;
        }

        public InputUser? GetUserByPlayerIndex(int playerIndex)
        {
            if (m_playerUsers.TryGetValue(playerIndex, out InputUser user))
            {
                return user;
            }
            return null;  // Falls kein User für den Index existiert
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