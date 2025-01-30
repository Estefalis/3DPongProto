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
        [SerializeField] private bool m_keyboardHasPriority;

        private int m_alreadySetGamepads;
        private Transform m_playfieldParent;
        private bool isInGameScene = false;  //True, while being in a GameScene.
        private const string m_keyboardDevice = "Keyboard", m_keyboardScheme = "KeyboardMouse";
        private const string m_gamepadDevice = "Gamepad", m_gamePadScheme = "Gamepad";

        private const string m_gameScene = "GameScene";

        #region Scriptable_Objects
        [SerializeField] private MatchUIStates m_matchUIStates;
        [SerializeField] private MatchValues m_matchValues;
        #endregion

        private void Awake()
        {
            #region Custom_InstanceSetup_as_Child
            //// Singleton-Pattern für globalen Zugriff
            //if (Instance == null)
            //{
            Instance = this;
            //    DontDestroyOnLoad(gameObject);
            //}
            //else
            //{
            //    Destroy(gameObject);
            //    return;
            //}
            #endregion

            m_alreadySetGamepads = 0;
            SceneManager.sceneLoaded += OnSceneLoaded;

            m_playerInputManager = GetComponent<PlayerInputManager>();
            SetUpPlayerInputManager(m_playerInputManager);
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            isInGameScene = scene.name.Contains(m_gameScene);

            if (isInGameScene)
            {
                m_playfieldParent = FindObjectOfType<LocalMatchManager>()?.m_PlayfieldParent;   //Public getter => private Transform.
                InstantiatePlayer();
            }
            else
            {
                CleanupPlayers();
                ActivateMenuControls();
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
                GameObject playerAvatar = Instantiate(m_matchValues.PlayerData[i].Prefab, m_playfieldParent);
                PlayerInput playerInput = playerAvatar.GetComponent<PlayerInput>();

                if (playerInput == null)
                    playerInput = playerAvatar.AddComponent<PlayerInput>();

                ConfigurePlayerInput(playerInput, i);
            }
        }

        private void ConfigurePlayerInput(PlayerInput _playerInput, int _playerIndex)
        {
            //Load and set the InputActionAsset.
            if (_playerInput.actions == null)
                _playerInput.actions = Resources.Load<InputActionAsset>("InputActions/PlayerInputActions");

            //Set default ControlDevice by connected device.
            int totalGamepadCount = Gamepad.all.Count;
            int availableGamepads = totalGamepadCount - m_alreadySetGamepads;

            InputDevice assignedDevice = null;
            InputDevice mouseDevice = Mouse.current;

            string controlScheme = m_keyboardScheme; //Keyboard and Mouse as standard.

            //Set Gamepad or Keyboard as InputDevice.
            switch (totalGamepadCount == 0 || availableGamepads > 0 && m_keyboardHasPriority)
            {
                //If no gamepad is available, or keyboard has priority for this player: Keyboard gets set as priority.
                case true:
                {
                    switch (_playerIndex)
                    {
                        case 0:
                            assignedDevice = Keyboard.current; //Player1 (WASD)
                            Debug.Log($"Player {_playerIndex + 1} uses Keyboard (WASD) & shared Mouse.");
                            break;
                        case 1:
                            assignedDevice = Keyboard.current; //Player2 (Arrow-Keys)
                            Debug.Log($"Player {_playerIndex + 1} uses Keyboard (Arrow Keys) & shared Mouse.");
                            break;
                        case 2:
                            assignedDevice = Keyboard.current; //Player3 (TFGH)
                            Debug.Log($"Player {_playerIndex + 1} uses Keyboard (TFGH) & shared Mouse.");
                            break;
                        case 3:
                            assignedDevice = Keyboard.current; //Player4 (IJKL)
                            Debug.Log($"Player {_playerIndex + 1} uses Keyboard (IJKL) & shared Mouse.");
                            break;
                        default:
                            Debug.LogError($"No available device for Player {_playerIndex + 1}!");
                            return;
                    }

                    _playerInput.SwitchCurrentControlScheme(controlScheme, new[] { assignedDevice, mouseDevice });
                    //PlayerInput.all[_playerIndex].SwitchCurrentControlScheme(controlScheme, new[] { assignedDevice, mouseDevice });
                    break;
                }
                case false:
                {
                    m_alreadySetGamepads += 1;

                    if (totalGamepadCount > 0 & availableGamepads > 0/* && _playerIndex + 1 <= totalGamepadCount*/)
                    {
                        Debug.Log($"Player: {_playerIndex + 1} - Gamepads: {totalGamepadCount}");
                        //If a gamepad is connected and keyboard has no priority for this player: Gamepad gets set.
                        assignedDevice = Gamepad.all[_playerIndex];
                        controlScheme = m_gamePadScheme;

                        _playerInput.SwitchCurrentControlScheme(controlScheme, new[] { assignedDevice });
                        //PlayerInput.all[_playerIndex].SwitchCurrentControlScheme(controlScheme, new[] { assignedDevice });
                    }

                    break;
                }
            }

            #region Old_Version
            ////Set Gamepad or Keyboard as InputDevice.
            //switch (totalGamepadCount > _playerIndex)
            //{
            //    //If enough gamepads are connected, every player gets an own gamepad.
            //    case true:
            //    {
            //        assignedDevice = Gamepad.all[_playerIndex];
            //        controlScheme = m_gamePadScheme;
            //        break;
            //    }
            //    //No gamepad is available. Keyboard gets set as priority.
            //    case false:
            //    {
            //        switch (_playerIndex)
            //        {
            //            case 0:
            //                assignedDevice = Keyboard.current; //Player1 (WASD)
            //                Debug.Log($"Player {_playerIndex + 1} uses Keyboard (WASD) & shared Mouse.");
            //                break;
            //            case 1:
            //                assignedDevice = Keyboard.current; //Player2 (Arrow-Keys)
            //                Debug.Log($"Player {_playerIndex + 1} uses Keyboard (Arrow Keys) & shared Mouse.");
            //                break;
            //            case 2:
            //                assignedDevice = Keyboard.current; //Player3 (TFGH)
            //                Debug.Log($"Player {_playerIndex + 1} uses Keyboard (TFGH) & shared Mouse.");
            //                break;
            //            case 3:
            //                assignedDevice = Keyboard.current; //Player4 (IJKL)
            //                Debug.Log($"Player {_playerIndex + 1} uses Keyboard (IJKL) & shared Mouse.");
            //                break;
            //            default:
            //                Debug.LogError($"No available device for Player {_playerIndex + 1}!");
            //                return;
            //        }
            //        break;
            //    }
            //}
            #endregion

            //Force activation of PlayerActions.
            _playerInput.SwitchCurrentActionMap("PlayerActions");
            _playerInput.neverAutoSwitchControlSchemes = false;
            //Set the notificationBehavior of the PlayerInput component.
            _playerInput.notificationBehavior = PlayerNotifications.InvokeCSharpEvents;

            Debug.Log($"✅ Player: Player{_playerIndex + 1} | ControlScheme: {controlScheme} | Device: {assignedDevice?.name ?? "None"} | ActionMap: {_playerInput.currentActionMap.name}");
        }

        private void CleanupPlayers()
        {
            Debug.Log("Cleaning up players...");

            m_matchValues.PlayerData.Clear();
        }

        private void ActivateMenuControls()
        {
            Debug.Log("Activating menu controls...");
            var menuInput = FindObjectOfType<PlayerInput>();
            if (menuInput == null)
            {
                menuInput = gameObject.AddComponent<PlayerInput>();
                menuInput.actions = Resources.Load<InputActionAsset>("InputActions/PlayerInputActions");
                //Enable active ControlScheme switch.
                menuInput.neverAutoSwitchControlSchemes = true;
                //Set the notificationBehavior of the PlayerInput component.
                menuInput.notificationBehavior = PlayerNotifications.InvokeCSharpEvents;
            }
            menuInput.SwitchCurrentActionMap("UserInterface");
        }
    }
}