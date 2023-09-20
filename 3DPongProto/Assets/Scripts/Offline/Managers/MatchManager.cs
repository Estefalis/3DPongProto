using System;
using ThreeDeePongProto.Offline.Player.Inputs;
using ThreeDeePongProto.Offline.UI;
using UnityEngine;

public enum EGameModi
{
    LocalPC,
    LAN,
    Internet
}

public enum EMatchCountOptions
{
    NoChanges,
    InfiniteRounds,
    InfinitePoints,
    InfiniteMatch
}

namespace ThreeDeePongProto.Managers
{
    public class MatchManager : MonoBehaviour
    {
        //TODO EMatchCountOptions implementieren.

        #region SerializeField-Member-Variables
        [SerializeField] private GameObject m_playGround;

        [Header("Defaults")]
        [SerializeField] private float m_playGroundWidthScale = 0.1f;
        [SerializeField] private float m_playGroundLengthScale = 0.1f;
        [SerializeField] private float m_playGroundWidth = 25.0f;
        [SerializeField] private float m_playGroundLength = 50.0f;
        [SerializeField] private float m_minimalFrontLineDistance = 6.0f;
        [SerializeField] private float m_minimalBackLineDistance = 1.5f;
        [SerializeField] private uint m_startRound = 1;
        [SerializeField] private int m_winPointDifference = 2;
        [SerializeField] private float m_maxPushDistance = 1.5f;
        [SerializeField] private Vector3 m_defaultPaddleScale;
        [Space]

        #region Scriptable Objects
        [SerializeField] private MatchUIStates m_matchUIStates;
        [SerializeField] private MatchValues m_matchValues;
        [SerializeField] private PlayerIDData[] m_playerIDData;
        #endregion
        #endregion

        #region Non-SerializeField-Member-Variables
        #region Properties-Access
        public float DefaultFrontLineDistance { get => m_minimalFrontLineDistance; }
        public float DefaultBackLineDistance { get => m_minimalBackLineDistance; }
        public float DefaultFieldWidth { get => m_playGroundWidth; }
        public float DefaultFieldLength { get => m_playGroundLength; }
        public float MaxPushDistance { get => m_maxPushDistance; }
        public Vector3 DefaultPaddleScale { get => m_defaultPaddleScale; }

        #endregion

        private string m_scoredPlayer;
        public bool MatchStarted { get => m_matchHasStarted; private set => m_matchHasStarted = value; }
        private bool m_matchHasStarted = false;
        public float MatchStartTime { get => m_matchStartTime; private set => m_matchStartTime = value; }
        private float m_matchStartTime;
        public bool GameIsPaused { get => m_gameIsPaused; }
        [SerializeField] private bool m_gameIsPaused;

        public static event Action m_StartNextRound;
        public static event Action m_StartWinProcedure;
        #endregion

        private void Awake()
        {
            SetScriptableDefaults();
            RegisterPlayerNames();
            ReSetMatch();
        }

        private void OnEnable()
        {
            PlayerMovement.InGameMenuOpens += PauseAndTimeScale;

            MenuOrganisation.CloseInGameMenu += ResetPauseAndTimescale;
            MenuOrganisation.LoadMainScene += MainSceneRestartActions;
            MenuOrganisation.RestartGameLevel += ReSetMatch;

            BallMovement.m_RoundCountStarts += MatchStartValues;
            BallMovement.m_HitGoalOne += UpdateTPTwoPoints;
            BallMovement.m_HitGoalTwo += UpdateTPOnePoints;

            m_StartNextRound += StartNextRound;
            m_StartWinProcedure += StartWinProcedure;
        }

        private void OnDisable()
        {
            PlayerMovement.InGameMenuOpens -= PauseAndTimeScale;

            MenuOrganisation.CloseInGameMenu -= ResetPauseAndTimescale;
            MenuOrganisation.LoadMainScene -= MainSceneRestartActions;
            MenuOrganisation.RestartGameLevel -= ReSetMatch;

            BallMovement.m_RoundCountStarts -= MatchStartValues;
            BallMovement.m_HitGoalOne -= UpdateTPTwoPoints;
            BallMovement.m_HitGoalTwo -= UpdateTPOnePoints;

            m_StartNextRound -= StartNextRound;
            m_StartWinProcedure -= StartWinProcedure;
        }

        private void UseDefaultSettings()
        {
            m_playGround.transform.localScale = new Vector3(m_playGroundWidth * m_playGroundWidthScale, m_playGround.transform.localScale.y, m_playGroundLength * m_playGroundLengthScale);
        }

        private void SetScriptableDefaults()
        {
            if (m_matchValues != null)
            {
#if UNITY_EDITOR
                m_matchValues.CurrentRoundNr = m_startRound;
                m_matchValues.WinPointDifference = m_winPointDifference;

                m_matchValues.MinFrontLineDistance = m_minimalFrontLineDistance;
                m_matchValues.MinBackLineDistance = m_minimalBackLineDistance;

                m_matchValues.MaxPushDistance = m_maxPushDistance;
                m_matchValues.XPaddleScale = m_defaultPaddleScale.x;
                m_matchValues.YPaddleScale = m_defaultPaddleScale.y;
                m_matchValues.ZPaddleScale = m_defaultPaddleScale.z;

                //Needs to be uncommented to test Rect-Change on Restart Game-Scene.
                //m_matchUIStates.SetCameraMode = m_eCameraMode;
            }

            m_gameIsPaused = false;
#endif
            //else
            //TODO: Load settings with an active Save- and Load-System.
        }

        private void MatchStartValues()
        {
            m_matchHasStarted = true;
            m_matchStartTime = Time.time;
        }

        private void RegisterPlayerNames()
        {
            m_matchValues.PlayerInGame.Clear();

            foreach (PlayerIDData player in m_playerIDData)
            {
                if (player.PlayerName != null || player.PlayerName != string.Empty)
                    m_matchValues.PlayerInGame.Add(player.PlayerName);
            }
        }

        private void UpdateTPOnePoints()
        {
            if (m_playerIDData == null || m_matchValues == null)
            {
#if UNITY_EDITOR
                Debug.Log("MatchManager: Forgot to add a Scriptable Object in the Editor!");
#endif
                return;
            }

            if (m_scoredPlayer != null || m_scoredPlayer != string.Empty)
                m_scoredPlayer = m_playerIDData[0].PlayerName;

            ++m_matchValues.CurrentPointsTPOne;
            ++m_matchValues.TotalPointsTPOne;

            CheckMatchConditions(m_scoredPlayer);
        }

        private void UpdateTPTwoPoints()
        {
            if (m_playerIDData == null || m_matchValues == null)
            {
#if UNITY_EDITOR
                Debug.Log("MatchManager: Forgot to add a Scriptable Object in the Editor!");
#endif
                return;
            }

            if (m_scoredPlayer != null || m_scoredPlayer != string.Empty)
                m_scoredPlayer = m_playerIDData[1].PlayerName;

            ++m_matchValues.CurrentPointsTPTwo;
            ++m_matchValues.TotalPointsTPTwo;

            CheckMatchConditions(m_scoredPlayer);
        }

        private void ReSetMatch()
        {
            if (m_matchValues == null)
            {
#if UNITY_EDITOR
                Debug.Log("MatchManager: Forgot to add a Scriptable Object in the Editor!");
#endif
                UseDefaultSettings();
                return;
            }

            ResetPlayfield();
            ResetRoundValues();
            ResetPauseAndTimescale();
        }

        #region Match-Presets
        private void ResetPlayfield()
        {
            m_playGround.transform.localScale = new Vector3(m_matchValues.SetGroundWidth * m_playGroundWidthScale, m_playGround.transform.localScale.y, m_matchValues.SetGroundLength * m_playGroundLengthScale);
        }

        private void ResetRoundValues()
        {
            if (m_matchValues != null)
            {
                m_matchValues.CurrentPointsTPOne = 0;
                m_matchValues.CurrentPointsTPTwo = 0;
            }
        }
        #endregion

        private void CheckMatchConditions(string _winningPlayer)
        {
            if (m_matchValues == null)
                return;

            bool nextRoundConditionIsMet =
                m_matchValues.CurrentPointsTPOne >= m_matchValues.SetMaxPoints &&
                m_matchValues.CurrentPointsTPOne >= m_matchValues.CurrentPointsTPTwo + m_matchValues.WinPointDifference
                ||
                m_matchValues.CurrentPointsTPTwo >= m_matchValues.SetMaxPoints &&
                m_matchValues.CurrentPointsTPTwo >= m_matchValues.CurrentPointsTPOne + m_matchValues.WinPointDifference;

            bool winConditionIsMet = m_matchValues.CurrentRoundNr == m_matchValues.SetMaxRounds && nextRoundConditionIsMet;

            if (winConditionIsMet)
            {
                FormatTextStrings(_winningPlayer);

                m_StartWinProcedure.Invoke();
                return;
            };

            switch (nextRoundConditionIsMet)
            {
                case true:
                {
                    m_StartNextRound?.Invoke();
                    break;
                }
                case false:
                {
#if UNITY_EDITOR
                    Debug.Log($"{m_scoredPlayer} scored. Congratulations!");
#endif
                    break;
                }
            }
        }

        private void FormatTextStrings(string _winningPlayer)
        {
            m_matchValues.WinningPlayer = _winningPlayer;
            m_matchValues.MatchWinDate = $"{DateTime.Today.ToShortDateString()}\n" + string.Format("{0:00}:{1:00}:{2:00}", DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
            TimeSpan timespan = TimeSpan.FromSeconds(Time.time - m_matchValues.StartTime);
            m_matchValues.TotalPlaytime = (float)timespan.TotalSeconds;
        }

        private void StartNextRound()
        {
            if (m_matchValues == null || m_matchUIStates.InfiniteRounds || m_matchUIStates.InfinitePoints)
                return;
#if UNITY_EDITOR
            Debug.Log("Next Round starts!");
#endif
            //TODO: May implement a procedure to transition into the next set Round.

            //Increase the RoundNr by 1.
            m_matchValues.CurrentRoundNr++;
            ResetRoundValues();
        }

        private void StartWinProcedure()
        {
#if UNITY_EDITOR
            Debug.Log("Won!");
#endif
            //TODO: Start WinProcedure.
        }

        private void PauseAndTimeScale()
        {
            Time.timeScale = 0f;
            m_gameIsPaused = true;
        }

        private void ResetPauseAndTimescale()
        {
            Time.timeScale = 1f;
            m_gameIsPaused = false;
        }

        private void MainSceneRestartActions()
        {
            ResetPauseAndTimescale();
        }
    }
}