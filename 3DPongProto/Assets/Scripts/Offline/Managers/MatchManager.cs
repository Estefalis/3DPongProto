using System;
using ThreeDeePongProto.Offline.UI;
using UnityEngine;

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
        [SerializeField] private MatchVariables m_matchVariables;
        [SerializeField] private PlayerData[] m_playerData;
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

        public static event Action m_StartNextRound;
        public static event Action m_StartWinProcedure;
        #endregion

        private void Awake()
        {
            SetScriptableDefaults();

            ReSetMatch();
        }

        private void OnEnable()
        {
            MenuOrganisation.RestartGameLevel += ReSetMatch;

            BallMovement.m_RoundCountStarts += MatchStartValues;
            BallMovement.m_HitGoalOne += UpdateTPTwoPoints;
            BallMovement.m_HitGoalTwo += UpdateTPOnePoints;

            m_StartNextRound += StartNextRound;
            m_StartWinProcedure += StartWinProcedure;
        }

        private void OnDisable()
        {
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
            if (m_matchVariables != null)
            {
#if UNITY_EDITOR
                m_matchVariables.CurrentRoundNr = m_startRound;
                m_matchVariables.WinPointDifference = m_winPointDifference;

                m_matchVariables.MinFrontLineDistance = m_minimalFrontLineDistance;
                m_matchVariables.MinBackLineDistance = m_minimalBackLineDistance;

                m_matchVariables.MaxPushDistance = m_maxPushDistance;
                m_matchVariables.StartPaddleScale = m_defaultPaddleScale;

                //Needs to be uncommented to test Rect-Change on Restart Game-Scene.
                //m_matchVariables.SetCameraMode = m_eCameraMode;
            }
#endif
            //else
                //TODO: Load settings with an active Save- and Load-System.
        }

        private void MatchStartValues()
        {
            m_matchHasStarted = true;
            m_matchStartTime = Time.time;
        }

        private void UpdateTPOnePoints()
        {
            if (m_playerData == null || m_matchVariables == null)
            {
#if UNITY_EDITOR
                Debug.Log("MatchManager: Forgot to add a Scriptable Object in the Editor!");
#endif
                return;
            }

            if (m_scoredPlayer != null || m_scoredPlayer != string.Empty)
                m_scoredPlayer = m_playerData[0].PlayerName;

            ++m_matchVariables.CurrentPointsTPOne;
            ++m_matchVariables.TotalPointsTPOne;
            //Player 1
            m_playerData[0].TotalPoints = m_matchVariables.TotalPointsTPOne;
            //Player 3
            m_playerData[2].TotalPoints = m_matchVariables.TotalPointsTPOne;

            CheckMatchConditions();
        }

        private void UpdateTPTwoPoints()
        {
            if (m_playerData == null || m_matchVariables == null)
            {
#if UNITY_EDITOR
                Debug.Log("MatchManager: Forgot to add a Scriptable Object in the Editor!");
#endif
                return;
            }

            if (m_scoredPlayer != null || m_scoredPlayer != string.Empty)
                m_scoredPlayer = m_playerData[1].PlayerName;

            ++m_matchVariables.CurrentPointsTPTwo;
            ++m_matchVariables.TotalPointsTPTwo;

            //Player 2
            m_playerData[1].TotalPoints = m_matchVariables.TotalPointsTPTwo;
            //Player 4
            m_playerData[3].TotalPoints = m_matchVariables.TotalPointsTPTwo;

            CheckMatchConditions();
        }

        private void ReSetMatch()
        {
            if (m_matchVariables == null)
            {
#if UNITY_EDITOR
                Debug.Log("MatchManager: Forgot to add a Scriptable Object in the Editor!");
#endif
                UseDefaultSettings();
                return;
            }

            ResetPlayfield();
            ResetRoundValues();
        }

        #region Match-Presets
        private void ResetPlayfield()
        {
            m_playGround.transform.localScale = new Vector3(m_matchVariables.SetGroundWidth * m_playGroundWidthScale, m_playGround.transform.localScale.y, m_matchVariables.SetGroundLength * m_playGroundLengthScale);
        }

        private void ResetRoundValues()
        {
            if (m_matchVariables != null)
            {
                m_matchVariables.CurrentPointsTPOne = 0;
                m_matchVariables.CurrentPointsTPTwo = 0;
            }
        }
        #endregion

        private void CheckMatchConditions()
        {
            if (m_matchVariables == null)
                return;

            bool nextRoundConditionIsMet =
                m_matchVariables.CurrentPointsTPOne >= m_matchVariables.SetMaxPoints &&
                m_matchVariables.CurrentPointsTPOne >= m_matchVariables.CurrentPointsTPTwo + m_matchVariables.WinPointDifference
                ||
                m_matchVariables.CurrentPointsTPTwo >= m_matchVariables.SetMaxPoints &&
                m_matchVariables.CurrentPointsTPTwo >= m_matchVariables.CurrentPointsTPOne + m_matchVariables.WinPointDifference;

            bool winConditionIsMet = m_matchVariables.CurrentRoundNr == m_matchVariables.SetMaxRounds && nextRoundConditionIsMet;

            if (winConditionIsMet)
            {
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

        private void StartNextRound()
        {
            //reuse of 'MatchVariables/MatchSettings public bool InfiniteRounds'.
            if (m_matchVariables == null || m_matchVariables.InfiniteRounds)
                return;
#if UNITY_EDITOR
            Debug.Log("Next Round starts!");
#endif
            //TODO: May implement a procedure to transition into the next set Round.

            //Increase the RoundNr by 1.
            m_matchVariables.CurrentRoundNr++;
            ResetRoundValues();
        }

        private void StartWinProcedure()
        {
#if UNITY_EDITOR
            Debug.Log("Won!");
#endif
            //TODO: Start WinProcedure.
        }
    }
}