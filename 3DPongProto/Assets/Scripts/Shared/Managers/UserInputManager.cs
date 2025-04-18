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
        #endregion

        #region Actions_and_Functions
        internal static event Action<string> AChangeActiveActionMap;    //Announce cheme switch, so PlayerInput components can react.
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

        private void SpawnPlayers()
        {
            if (m_playerInputManager == null)
            {
                Debug.LogError("PlayerInputManager not set!", this);
                return;
            }

            if (m_matchValues == null)
            {
                Debug.LogError("MatchValues ScriptableObject not set!", this);
                return;
            }

            int numberOfPlayers = (m_matchValues.PlayerSOData != null && m_matchValues.PlayerSOData.Count > 0)
                                ? m_matchValues.PlayerSOData.Count : m_defaultPlayerNumber;
#if UNITY_EDITOR
            //Debug.Log($"Spawning {numberOfPlayers} Players...");
#endif

            //Liste of available Gamepads.
            m_availableGamepads = new(Gamepad.all);

            for (int playerIndex = 0; playerIndex < numberOfPlayers; playerIndex++)
            {
                InputDevice[] devicesToPair = null;
                string controlScheme = "";

                devicesToPair = GetDevicesForPlayer(playerIndex);
                controlScheme = GetControlSchemeForPlayer(playerIndex, devicesToPair);

                //Let Player join.
                if (devicesToPair != null && devicesToPair.Length > 0 && !string.IsNullOrEmpty(controlScheme))
                {
                    //Ensure that the devices are valid. (May be null at start.)
                    if (devicesToPair.Any(d => d == null))
                    {
                        Debug.LogError($"Atleast one device is null. Player {playerIndex} can't join.");
                        //If the device is invalid and a Gamepad re-add it to the list of available Gamepads.
                        if (controlScheme == m_gamePadScheme && devicesToPair.Length > 0)
                            m_availableGamepads.Add(devicesToPair[0]);
                        continue;
                    }
#if UNITY_EDITOR
                    //Debug.Log($"Player {playerIndex} joins with '{controlScheme}' and device(s): {string.Join(", ", devicesToPair.Select(d => d.displayName))} via .JoinPlayer.");
#endif
                    PlayerInput joinedPlayerInput = m_playerInputManager.JoinPlayer(playerIndex, -1, controlScheme, devicesToPair);

                    if (joinedPlayerInput != null)
                    {
#if UNITY_EDITOR
                        //Debug.Log($"Player {playerIndex} joined succesfully. GameObject: {joinedPlayerInput.gameObject.name}, PlayerInput Index: {joinedPlayerInput.playerIndex}, PlayerInput UserID: {joinedPlayerInput.user.id}");
#endif
                        //Pass PlayerID to CharacterMainController script.
                        if (joinedPlayerInput.TryGetComponent<CharacterMainController>(out var playerController))
                        {
                            playerController.gameObject.name = $"Player{playerIndex}";
                            playerController.ReceivePlayerID(playerIndex);
                        }
                        else
                        {
                            Debug.LogError($"No CharacterMainController script found on {joinedPlayerInput.gameObject} for Player {playerIndex}!");
                        }
                    }
                    else
                    {
                        Debug.LogError($"An error occured on .JoinPlayer for Player {playerIndex}!");
                        //If the device is invalid and a Gamepad re-add it to the list of available Gamepads.
                        if (controlScheme == m_gamePadScheme && devicesToPair.Length > 0)
                            m_availableGamepads.Add(devicesToPair[0]);
                    }
                }
                else
                {
                    Debug.LogWarning($"No valid device and/or controlScheme found for Player {playerIndex}.");
                }
            }
        }

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