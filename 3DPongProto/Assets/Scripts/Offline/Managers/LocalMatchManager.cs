using System;
using ThreeDeePongProto.Offline.UI.Menu;
using ThreeDeePongProto.Shared.PlayerCharacter;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

public enum EGameModi
{
    LocalPC,
    LAN,
    Internet
}

namespace ThreeDeePongProto.Shared.Managers
{
    public class LocalMatchManager : MonoBehaviour
    {
        private PlayerInputManager m_playerInputManager;

        #region SerializeField-Member-Variables
        [SerializeField] private string m_NPCName = "Some Test-NPC";    //TODO: Implement NPC for solo play!
        [SerializeField] private GameObject m_playGround;
        [SerializeField] private GameObject m_ballPrefab;
        [SerializeField] private Transform m_prefabParent;

        [Header("Defaults")]
        #region Playground-Variables
        [SerializeField] private float m_playGroundWidthScale = 0.1f;
        [SerializeField] private float m_playGroundLengthScale = 0.1f;
        [SerializeField] private float m_playGroundWidth = 25.0f;
        [SerializeField] private float m_playGroundLength = 50.0f;
        [SerializeField] private float m_minimalFrontLineDistance = 6.0f;
        [SerializeField] private float m_minimalBackLineDistance = 1.5f;
        #endregion

        #region Match-Variables
        [SerializeField] private int m_setMaxRounds = 5;
        [SerializeField] private int m_setMaxPoints = 25;
        [SerializeField] private uint m_startRound = 1;
        [SerializeField] private int m_winPointDifference = 2;
        #endregion

        #region Paddle-Variables
        [SerializeField] private float m_maxPushDistance = 1.5f;
        [SerializeField] private float m_paddleWidthAdjustStep = 0.25f;
        [SerializeField] private Vector3 m_defaultPaddleScale;
        #endregion

        [SerializeField] private bool m_gameIsPaused = false;
        [Space]

        #region Scriptable Objects
        [SerializeField] private MatchUIStates m_matchUIStates;
        [SerializeField] private BasicFieldValues m_basicFieldValues;
        [SerializeField] private MatchValues m_matchValues;
        #endregion
        #endregion

        #region Non-SerializeField-Member-Variables
        private bool m_nextRoundConditionIsMet;
        private string m_scoredPlayer;

        [SerializeField] private bool m_matchHasStarted = false;
        private float m_matchStartTime, m_ballYPos;
        private Vector3 m_ballPopPos;

        #region Properties-Access
        public float DefaultBackLineDistance { get => m_minimalBackLineDistance; }
        public float DefaultFrontLineDistance { get => m_minimalFrontLineDistance; }
        public float DefaultFieldWidth { get => m_playGroundWidth; }
        public float DefaultFieldLength { get => m_playGroundLength; }
        public float MaxPushDistance { get => m_maxPushDistance; }
        public Vector3 DefaultPaddleScale { get => m_defaultPaddleScale; }
        public bool MatchStarted { get => m_matchHasStarted; private set => m_matchHasStarted = value; }
        public float MatchStartTime { get => m_matchStartTime; private set => m_matchStartTime = value; }
        public float PaddleWidthAdjustStep { get => m_paddleWidthAdjustStep; }
        public bool GameIsPaused { get => m_gameIsPaused; }
        #endregion

        #region Actions
        internal static event Action StartNextRound;
        internal static event Action StartWinProcedure;
        internal static event Action LoadUpHighscores;
        internal static event Action<int> CheckForPlayerInput;
        #endregion
        #endregion

        #region Serialization
        private readonly string m_fieldSettingsPath = "/SaveData/FieldSettings";
        private readonly string m_matchFileName = "/Match";
        private readonly string m_fileFormat = ".json";

        private readonly IPersistentData m_persistentData = new SerializingData();
        private readonly bool m_encryptionEnabled = false;
        #endregion

        private void Awake()
        {
            if (m_defaultPaddleScale == Vector3.zero)
                m_defaultPaddleScale = new Vector3(3.5f, 1.0f, 0.5f);

            m_playerInputManager = GetComponent<PlayerInputManager>();
            m_matchUIStates.GameRuns = true;

            PrepareMatchStart();
            LoadMatchSettings();
            ReSetMatch();
            InstantiatePlayer();
        }

        private void OnEnable()
        {
            MenuManager.ResumeTheGame += ResumeGame;
            MenuManager.EndInfiniteMatch += LetsEndInfiniteMatch;
            MenuManager.OnLoadMainScene += ResetPauseAndTimescale;

            Ball.RoundCountStarts += MatchStartValues;
            Ball.HitGoalOne += UpdateTPTwoPoints;
            Ball.HitGoalTwo += UpdateTPOnePoints;

            StartNextRound += LetsStartNextRound;
            StartWinProcedure += LetsStartWinProcedure;
        }

        private void OnDisable()
        {
            MenuManager.ResumeTheGame -= ResumeGame;
            MenuManager.EndInfiniteMatch -= LetsEndInfiniteMatch;
            MenuManager.OnLoadMainScene -= ResetPauseAndTimescale;

            //Useable for Infinite Matches.
            m_matchValues.TotalPointsTPOne = 0;
            m_matchValues.TotalPointsTPTwo = 0;
            m_matchUIStates.GameRuns = false;

            Ball.RoundCountStarts -= MatchStartValues;
            Ball.HitGoalOne -= UpdateTPTwoPoints;
            Ball.HitGoalTwo -= UpdateTPOnePoints;

            StartNextRound -= LetsStartNextRound;
            StartWinProcedure -= LetsStartWinProcedure;

            CharacterInputHandler.m_menuOpens -= PauseAndTimeScale;
            m_playerInputManager.onPlayerJoined -= OnPlayerJoined;
        }

        private void Start()
        {
            CharacterInputHandler.m_menuOpens += PauseAndTimeScale;
            m_playerInputManager.onPlayerJoined += OnPlayerJoined;
        }

        #region Custom-Methods
        private void PrepareMatchStart()
        {
            m_matchUIStates.MaxRounds = m_setMaxRounds;
            m_matchUIStates.MaxPoints = m_setMaxPoints;

            m_gameIsPaused = false;

            if (m_matchValues != null)
            {
                m_matchValues.CurrentRoundNr = m_startRound;
                m_matchValues.WinPointDifference = m_winPointDifference;
                m_matchValues.MaxPushDistance = m_maxPushDistance;

                m_matchValues.XPaddleScale = m_defaultPaddleScale.x;
                m_matchValues.YPaddleScale = m_defaultPaddleScale.y;
                m_matchValues.ZPaddleScale = m_defaultPaddleScale.z;
            }

            m_ballYPos = m_ballPrefab.GetComponent<SphereCollider>().radius;
            m_ballPopPos = new Vector3(0.0f, m_ballYPos, 0.0f);
        }

        private void LoadMatchSettings()
        {
            BasicFieldSetup basicFieldSetup = m_persistentData.LoadData<BasicFieldSetup>(m_fieldSettingsPath, m_matchFileName, m_fileFormat, m_encryptionEnabled);

            m_basicFieldValues.SetGroundWidth = basicFieldSetup.SetGroundWidth;
            m_basicFieldValues.SetGroundLength = basicFieldSetup.SetGroundLength;
            m_basicFieldValues.MinBackLineDistance = basicFieldSetup.MinBackLineDistance;
            m_basicFieldValues.MinFrontLineDistance = basicFieldSetup.MinFrontLineDistance;
            m_basicFieldValues.BacklineAdjustment = basicFieldSetup.BackLineAdjustment;
            m_basicFieldValues.FrontlineAdjustment = basicFieldSetup.FrontLineAdjustment;
        }

        private void MatchStartValues()
        {
            m_matchHasStarted = true;
            m_matchStartTime = Time.time;
            m_matchValues.StartTime = Time.time;    //OR m_matchValues.StartDateTime = DateTime.Now.Ticks;
        }

        private void UpdateTPOnePoints()
        {
            if (m_matchValues.PlayerData == null || m_matchValues == null)
            {
#if UNITY_EDITOR
                Debug.Log("MatchManager: Forgot to add a Scriptable Object in the Editor!");
#endif
                return;
            }

            //Reset each Round.
            ++m_matchValues.MatchPointsTPOne;
            //Reset only on Disable.
            ++m_matchValues.TotalPointsTPOne;

            switch ((int)m_matchUIStates.EPlayerAmount)
            {
                //TODO: Include AI NPC + Multiplayer.
                case 1:
                {
                    if (m_scoredPlayer == null)
                        m_scoredPlayer = $"{m_NPCName} from Team 1";
                    else
                        m_scoredPlayer = m_matchValues.PlayerData[0].PlayerName;
                    break;
                }
                //Solo
                case 2:
                {
                    if (m_scoredPlayer != null || m_scoredPlayer != string.Empty)
                        m_scoredPlayer = m_matchValues.PlayerData[0].PlayerName;
                    break;
                }
                //Team
                case 4:
                {
                    if (m_scoredPlayer != null || m_scoredPlayer != string.Empty)
                        m_scoredPlayer = $"{m_matchValues.PlayerData[0].PlayerName} & {m_matchValues.PlayerData[2].PlayerName}";
                    break;
                }
                default:
                    break;
            }

            CheckMatchConditions(m_scoredPlayer, m_matchValues.TotalPointsTPOne);
        }

        private void UpdateTPTwoPoints()
        {
            if (m_matchValues.PlayerData == null || m_matchValues == null)
            {
#if UNITY_EDITOR
                Debug.Log("MatchManager: Forgot to add a Scriptable Object in the Editor!");
#endif
                return;
            }

            //Reset each Round.
            ++m_matchValues.MatchPointsTPTwo;
            //Reset only on Disable.
            ++m_matchValues.TotalPointsTPTwo;

            switch ((int)m_matchUIStates.EPlayerAmount)
            {
                //TODO: Include AI NPC + Multiplayer.
                case 1:
                {
                    if (m_scoredPlayer == null)
                        m_scoredPlayer = $"{m_NPCName} from Team 2";
                    else
                        m_scoredPlayer = m_matchValues.PlayerData[1].PlayerName;
                    break;
                }
                //Solo
                case 2:
                {
                    if (m_scoredPlayer != null || m_scoredPlayer != string.Empty)
                        m_scoredPlayer = m_matchValues.PlayerData[1].PlayerName;
                    break;
                }
                //Team
                case 4:
                {
                    if (m_scoredPlayer != null || m_scoredPlayer != string.Empty)
                        m_scoredPlayer = $"{m_matchValues.PlayerData[1].PlayerName} & {m_matchValues.PlayerData[3].PlayerName}";
                    break;
                }
                default:
                    break;
            }

            CheckMatchConditions(m_scoredPlayer, m_matchValues.TotalPointsTPTwo);
        }

        private void ReSetMatch()
        {
            if (m_matchValues == null)
            {
#if UNITY_EDITOR
                Debug.Log("MatchManager: Forgot to add a Scriptable Object in the Editor!");
#endif
                DefaultPlayfield();
                SetPauseAndTimeScale(m_gameIsPaused);
                return;
            }

            ReSetPlayfield();
            ResetRoundValues();
            SetPauseAndTimeScale(m_gameIsPaused);

            InputManager.ToggleActionMaps(InputManager.m_PlayerInputActions.PlayerActions);
        }

        private void DefaultPlayfield()
        {
            m_playGround.GetComponent<Transform>();
            m_playGround.transform.localScale = new Vector3(m_playGroundWidth * m_playGroundWidthScale, m_playGround.transform.localScale.y, m_playGroundLength * m_playGroundLengthScale);
        }

        private void ResetRoundValues()
        {
            if (m_matchValues != null)
            {
                m_matchValues.MatchPointsTPOne = 0;
                m_matchValues.MatchPointsTPTwo = 0;
            }
        }

        #region Match-Presets
        private void ReSetPlayfield()
        {
            m_playGround.GetComponent<Transform>();
            m_playGround.transform.localScale = new Vector3(m_basicFieldValues.SetGroundWidth * m_playGroundWidthScale, m_playGround.transform.localScale.y, m_basicFieldValues.SetGroundLength * m_playGroundLengthScale);

            Instantiate(m_playGround, Vector3.zero, Quaternion.Euler(0, 0, 0), m_prefabParent);
            Instantiate(m_ballPrefab, m_ballPopPos, Quaternion.Euler(0, 0, 0), m_prefabParent);
        }
        #endregion

        private void CheckMatchConditions(string _winningPlayer, double _pointCount)
        {
            if (m_matchValues == null)
                return;

            //If either no max Round or max Point amount is set, then there shall be no next Round.
            if (m_matchUIStates.InfiniteMatch)
            {
                m_nextRoundConditionIsMet = false;
                return;
            }
            else
            {
                m_nextRoundConditionIsMet =
                m_matchValues.MatchPointsTPOne >= m_matchUIStates.LastMaxPointDdIndex &&
                m_matchValues.MatchPointsTPOne >= m_matchValues.MatchPointsTPTwo + m_matchValues.WinPointDifference
                ||
                m_matchValues.MatchPointsTPTwo >= m_matchUIStates.LastMaxPointDdIndex &&
                m_matchValues.MatchPointsTPTwo >= m_matchValues.MatchPointsTPOne + m_matchValues.WinPointDifference;
            }

            //WinCondition is true, when the current RoundNumber equals the max set roundAmount AND the winpointDifference (PlayerCharacter 1 <-> PlayerCharacter 2) triggers a new round.
            bool winConditionIsMet = m_matchValues.CurrentRoundNr == m_matchUIStates.LastRoundDdIndex && m_nextRoundConditionIsMet;

            if (winConditionIsMet)
            {
                if (m_matchUIStates.EGameConnectModi == EGameModi.LocalPC)
                    SaveMatchDetails(_winningPlayer, _pointCount);

                StartWinProcedure?.Invoke();
                return;
            };

            switch (m_nextRoundConditionIsMet)
            {
                case true:
                {
                    StartNextRound?.Invoke();
                    break;
                }
                case false:
                {
#if UNITY_EDITOR
                    Debug.Log($"MatchManager: {m_scoredPlayer} scored. Congratulations!");
#endif
                    break;
                }
            }
        }

        private void SaveMatchDetails(string _winningPlayer, double _winPlayerPoints)
        {
            m_matchValues.WinningPlayer = _winningPlayer;
            m_matchValues.TotalPoints = _winPlayerPoints;
            m_matchValues.MatchWinDate = $"{DateTime.Today.ToShortDateString()}\n" + string.Format("{0:00}:{1:00}:{2:00}", DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
            TimeSpan timespan = TimeSpan.FromSeconds(Time.time - m_matchValues.StartTime);
            m_matchValues.TotalPlaytime = (float)timespan.TotalSeconds;

            LoadUpHighscores?.Invoke();
        }

        /// <summary>
        /// Method called by a Button that is hidden in the 'Pause Menu', until the match is in "Infinity-Mode".
        /// </summary>
        private void LetsEndInfiniteMatch()
        {
            if (m_matchValues.TotalPointsTPOne == 0 && m_matchValues.TotalPointsTPTwo == 0)
            {
                //TODO: PopUp-Window: "No points gained, yet.
#if UNITY_EDITOR
                Debug.Log("Noone gained any Points, yet!");
#endif
                return;
            }

            //m_matchValues.WinningPlayer gets set while checking for GetHigherPlayerScore.
            m_matchValues.TotalPoints = GetHigherPlayerScore(m_matchValues.TotalPointsTPOne, m_matchValues.TotalPointsTPTwo);
            m_matchValues.MatchWinDate = $"{DateTime.Today.ToShortDateString()}\n" + string.Format("{0:00}:{1:00}:{2:00}", DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
            TimeSpan timespan = TimeSpan.FromSeconds(Time.time - m_matchValues.StartTime);
            m_matchValues.TotalPlaytime = (float)timespan.TotalSeconds;

            LoadUpHighscores?.Invoke();
        }

        /// <summary>
        /// Method to get the Higher Value for Infinite Matches.
        /// </summary>
        /// <param name="_infinitePointsTPOne"></param>
        /// <param name="_infinitePointsTPTwo"></param>
        /// <returns></returns>
        private double GetHigherPlayerScore(double _infinitePointsTPOne, double _infinitePointsTPTwo)
        {
            if (_infinitePointsTPOne != _infinitePointsTPTwo)
            {
                bool higherPoints = _infinitePointsTPOne > _infinitePointsTPTwo;
                switch (higherPoints)
                {
                    case true:
                    {
                        //Team One.
                        m_matchValues.WinningPlayer = $"{m_matchValues.PlayerData[0].PlayerName} & {m_matchValues.PlayerData[2].PlayerName}";
                        return _infinitePointsTPOne;
                    }
                    case false:
                    {
                        //Team Two.
                        m_matchValues.WinningPlayer = $"{m_matchValues.PlayerData[1].PlayerName} & {m_matchValues.PlayerData[3].PlayerName}";
                        return _infinitePointsTPTwo;
                    }
                }
            }
            else
            {
                m_matchValues.WinningPlayer = $"{m_matchValues.PlayerData[0].PlayerName} & {m_matchValues.PlayerData[2].PlayerName} draw \nto {m_matchValues.PlayerData[1].PlayerName} & {m_matchValues.PlayerData[3].PlayerName}";
                return _infinitePointsTPOne;
            }
        }

        private void LetsStartNextRound()
        {
            if (m_matchValues == null)
                return;
#if UNITY_EDITOR
            Debug.Log("Next Round starts!");
#endif
            //TODO: May implement a procedure to transition into the next set Round.

            //Increase the RoundNr by 1.
            m_matchValues.CurrentRoundNr++;
            ResetRoundValues();
        }

        private void LetsStartWinProcedure()
        {
#if UNITY_EDITOR
            Debug.Log("Won!");
#endif
            //TODO: Start WinProcedure.
        }

        private void ResetPauseAndTimescale()
        {
            SetPauseAndTimeScale(false);
            //foreach (var player in FindObjectsOfType<PlayerInput>())
            //{
            //    player.SwitchCurrentActionMap("UserInterface");
            //    Debug.Log($"Resetting PlayerInput for Player {player.playerIndex}");
            //}
        }

        private void ResumeGame()
        {
            SetPauseAndTimeScale(false);
        }

        private void SetPauseAndTimeScale(bool _isPaused)
        {
            switch (_isPaused)
            {
                case false:
                {
                    Time.timeScale = 1f;
                    m_gameIsPaused = false;
                    break;
                }
                case true:
                {
                    Time.timeScale = 0f;
                    m_gameIsPaused = true;
                    break;
                }
            }
        }
        #endregion

        #region Spawn_and_set_up_Player
        private void InstantiatePlayer()
        {
            uint playerCount = (uint)m_matchUIStates.EPlayerAmount;

            for (int i = 0; i < playerCount; i++)
            {
                GameObject playerAvatar = Instantiate(m_matchValues.PlayerData[i].Prefab, m_prefabParent);
                PlayerInput playerInput = playerAvatar.GetComponent<PlayerInput>();

                if (playerInput == null)
                    playerInput = playerAvatar.AddComponent<PlayerInput>();

                ConfigurePlayerInput(playerInput, i);
            }
        }

        private void OnPlayerJoined(PlayerInput _playerInput)
        {
            if (_playerInput == null)
            {
                Debug.LogError("PlayerInput is null!");
                return;
            }

            //Configure PlayerInput only, if the controlScheme isn't set, yet.
            if (_playerInput.currentControlScheme == null)
            {
                InputUser.PerformPairingWithDevice(null);
                ConfigurePlayerInput(_playerInput, _playerInput.playerIndex);
            }

            Debug.Log($"Player {_playerInput.playerIndex + 1} joined with ControlScheme: {_playerInput.currentControlScheme}");
        }

        private void ConfigurePlayerInput(PlayerInput playerInput, int playerIndex)
        {
            //Load and set the InputActionAsset.
            if (playerInput.actions == null)
                playerInput.actions = Resources.Load<InputActionAsset>("InputActions/PlayerInputActions");
            else
            {
                Debug.LogError("PlayerInput actions are null!");
                return;
            }

            //Set default ControlDevice by connected device.
            InputDevice device = Gamepad.all.Count > playerIndex ? Gamepad.all[playerIndex] : (InputDevice)Keyboard.current;
            InputDevice additionalDevice = device is Keyboard ? Mouse.current : null;
            playerInput.defaultControlScheme = device is Gamepad ? "Gamepad" : "KeyboardMouse";

            if (device == null || (device is Keyboard && additionalDevice == null))
            {
                Debug.LogError($"Invalid device setup for Player {playerIndex + 1}. Device: {device}, AdditionalDevice: {additionalDevice}");
                return;
            }

            string controlScheme = device is Gamepad ? "Gamepad" : "KeyboardMouse";

            if (device != null)
            {
                if (additionalDevice != null)
                {
                    playerInput.SwitchCurrentControlScheme(controlScheme, additionalDevice != null ? new[] { device, additionalDevice } : new[] { device });
                }
                else
                {
                    playerInput.SwitchCurrentControlScheme(controlScheme, device);
                }

                //Enable active ControlScheme switch.
                playerInput.neverAutoSwitchControlSchemes = false;
                //Set the notificationBehavior of the PlayerInput component.
                playerInput.notificationBehavior = PlayerNotifications.InvokeCSharpEvents;
                //Set Default Map in the PlayerInput component.
                playerInput.SwitchCurrentActionMap("PlayerActions");
                Debug.Log($"Player {playerInput.playerIndex + 1}, InputUser: {playerInput.user} initialized with ControlScheme: {controlScheme}, Device: {device}, NotificationBehavior: {playerInput.notificationBehavior}.");
            }
            else
            {
                Debug.LogError($"No valid device found for Player {playerInput.playerIndex + 1}.");
            }

            CheckForPlayerInput?.Invoke(playerIndex);
        }
        #endregion

        #region CallbackContexts
        private void PauseAndTimeScale()
        {
            SetPauseAndTimeScale(true);
        }
        #endregion
    }
}