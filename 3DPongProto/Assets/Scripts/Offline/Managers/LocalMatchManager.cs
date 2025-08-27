using System;
using System.Collections.Generic;
using ThreeDeePongProto.Offline.UI.Menu;
using ThreeDeePongProto.Shared.Highscores;
using UnityEngine;
using UnityEngine.InputSystem;

public enum EGameConnectionModi
{
    LocalGame,
    LanGame,
    NetGame
}

public struct MatchResult
{
    public string WinnerName;
    public string FinalScore;
    public string TotalPlayTime;
    public string MatchWinDate;
}

namespace ThreeDeePongProto.Shared.Managers
{
    public class LocalMatchManager : MonoBehaviour
    {
        [Header("Scene & Prefab References")]
        [SerializeField] private string m_NPCName = "Some Test-NPC";    //TODO: Implement NPC for solo play!
        [SerializeField] private GameObject m_playGround;
        [SerializeField] private GameObject m_ballPrefab;
        [SerializeField] private Transform m_prefabParent;

        [SerializeField] private GameObject[] m_playerPrefabs; // NEU: Array für P1-P4 Prefabs
        [SerializeField] private HighScoreBoard m_highScoreBoard; // NEU: Referenz auf das Board

        #region Private-States
        //Match-Config
        private MatchSettingsData m_matchData;
        private List<PlayerProfileData> m_playerProfiles;
        private Vector3 m_ballPopPos;

        //Match-Runtime-Values
        private int m_currentScoreTeam1, m_totalScoreTeam1;
        private int m_currentScoreTeam2, m_totalScoreTeam2;
        private int m_currentRound;
        //private CharacterMainController m_lastTouchTeam1, m_lastTouchTeam2;
        private string m_lastTouchedPlayerT1, m_lastTouchedPlayerT2;

        private float m_matchStartTime, m_ballYPos;

        private bool m_gameSceneStarted = false;
        private bool m_gameIsPaused = false;
        private bool m_matchHasStarted = false;

        private bool m_nextRoundConditionIsMet;
        #endregion

        #region Paddle-Variables
        [Header("Paddle-Variables")]
        [SerializeField] internal float m_maxPushDistance = 2.0f;
        [SerializeField] private float m_paddleWidthAdjustStep = 0.25f;
        [SerializeField] private Vector3 m_defaultPaddleScale;
        #endregion

        #region Properties-Access
        internal Transform PrefabParent { get => m_prefabParent; }
        public float MaxPushDistance { get => m_maxPushDistance; }
        public Vector3 DefaultPaddleScale { get => m_defaultPaddleScale; }
        public bool MatchStarted { get => m_matchHasStarted; private set => m_matchHasStarted = value; }
        public float MatchStartTime { get => m_matchStartTime; private set => m_matchStartTime = value; }
        public float PaddleWidthAdjustStep { get => m_paddleWidthAdjustStep; }
        public bool GameIsPaused { get => m_gameIsPaused; }
        #endregion

        #region Game Rules & Constants
        private const float m_playGroundWidthScale = 0.1f;
        private const float m_playGroundLengthScale = 0.1f;
        public static readonly Vector3 DEFAULT_PADDLE_SCALE = new Vector3(3.5f, 1.0f, 0.5f);
        #endregion

        #region Actions
        internal static event Action AStartNextRound;                   //Notifies MatchUserInterface!
        internal static event Action AStartWinProcedure;
        internal static event Action ALoadUpHighScores;
        #endregion

        private void OnEnable()
        {
            m_gameSceneStarted = true;

            MenuManager.AResumeTheGame += OnResumeGame;
            MenuManager.AReLoadScene += OnReloadScene;                  //TODO: Reset Field, Ball, Player values on sceneReload?
            MenuManager.AEndInfiniteMatch += LetsEndInfiniteMatch;      //TODO: MenuButton with public LM-ManagerScript-Method?

            Ball.RoundCountStarts += MatchStartValues;
            Ball.OnHitGoalOne += OnBallHitsTeam1Goal;
            Ball.OnHitGoalTwo += OnBallHitsTeam2Goal;
            Ball.OnHitPlayer += SetLastTouch;

            AStartNextRound += LetsStartNextRound;
            AStartWinProcedure += LetsStartWinProcedure;

            UserInputManager.AChangeActiveActionMap += PauseAndTimeScale;
        }

        private void OnDisable()
        {
            m_gameSceneStarted = false;

            MenuManager.AResumeTheGame -= OnResumeGame;
            MenuManager.AReLoadScene -= OnReloadScene;
            MenuManager.AEndInfiniteMatch -= LetsEndInfiniteMatch;

            Ball.RoundCountStarts -= MatchStartValues;
            Ball.OnHitGoalOne -= OnBallHitsTeam1Goal;
            Ball.OnHitGoalTwo -= OnBallHitsTeam2Goal;
            Ball.OnHitPlayer -= SetLastTouch;

            AStartNextRound -= LetsStartNextRound;
            AStartWinProcedure -= LetsStartWinProcedure;

            UserInputManager.AChangeActiveActionMap -= PauseAndTimeScale;
        }

        private void Start()
        {
            var settingsManager = SettingsManager.Instance;
            var profileManager = PlayerProfileManager.Instance;
            var playerInputManager = PlayerInputManager.instance;

            //Manager-Security-Checks
            if (settingsManager == null || profileManager == null || playerInputManager == null)
            {
                Debug.LogError("One or more required Manager were not found!");
                //Optional: Load StartMenu-Scene.
                return;
            }

            playerInputManager.onPlayerJoined += OnPlayerJoined;

            m_matchData = settingsManager.CurrentSettings.Match;
            m_playerProfiles = profileManager.PlayerProfiles;

            SetupMatch();
        }

        #region Player Setup
        private void OnPlayerJoined(PlayerInput playerInput)
        {
            //int playerIndex = playerInput.playerIndex;
            //GameObject playerObject = playerInput.gameObject;

            //PlayerProfileData profile = m_playerProfiles[playerIndex];
            //EPlayerLine line = m_matchData.PlayerPositions[playerIndex];

            //float zSide = (playerIndex % 2 == 0) ? -1f : 1f;
            //float halfFieldLength = m_matchData.FieldLength / 2f;
            //float lineDistance = (line == EPlayerLine.Backline) ? m_matchData.BacklineDistance : m_matchData.FrontlineDistance;
            //float zPos = zSide * (halfFieldLength - lineDistance); // Anpassung für Distanz von der Endlinie

            //Vector3 spawnPosition = new Vector3(0, 0.5f, zPos);
            //Quaternion spawnRotation = Quaternion.LookRotation(new Vector3(0, 0, -zSide));

            //playerObject.transform.SetPositionAndRotation(spawnPosition, spawnRotation);
            //playerObject.name = profile.PlayerName;

            //// Skalierung und andere Werte an das Spieler-Skript übergeben
            //if (playerObject.TryGetComponent<CharacterMainController>(out var controller))
            //{
            //    controller.Initialize(profile, m_matchData);
            //}
        }
        #endregion

        #region Custom-Methods
        private void MatchStartValues()
        {
            m_matchHasStarted = true;
            m_matchStartTime = Time.time;   //OR StartDateTime = DateTime.Now.Ticks;
        }

        private void SetupMatch()
        {
            m_currentScoreTeam1 = 0;
            m_totalScoreTeam1 = 0;
            m_currentScoreTeam2 = 0;
            m_totalScoreTeam2 = 0;
            m_currentRound = 1;
            m_matchHasStarted = false;

            if (m_gameIsPaused)
                m_gameIsPaused = false;

            BuildPlayField();
            SpawnBall();

            //TODO: Replace and save maximal pushDistance 'm_maxPushDistance' elsewhere?/.
        }

        private void BuildPlayField()
        {
            m_playGround.GetComponent<Transform>();
            m_playGround.transform.localScale = new Vector3(m_matchData.FieldWidth * m_playGroundWidthScale, m_playGround.transform.localScale.y, m_matchData.FieldLength * m_playGroundLengthScale);

            Instantiate(m_playGround, Vector3.zero, Quaternion.Euler(0, 0, 0), m_prefabParent);
        }

        private void SpawnBall()
        {
            m_ballYPos = m_ballPrefab.GetComponent<SphereCollider>().radius;
            m_ballPopPos = new Vector3(0.0f, m_ballYPos, 0.0f);

            Instantiate(m_ballPrefab, m_ballPopPos, Quaternion.Euler(0, 0, 0), m_prefabParent);
        }

        private void OnBallHitsTeam1Goal()
        {
            //Reset each Round.
            ++m_currentScoreTeam2;
            //Reset only on Disable.
            ++m_totalScoreTeam2;

            CheckMatchConditions(m_lastTouchedPlayerT2, m_currentScoreTeam2);
        }

        private void OnBallHitsTeam2Goal()
        {
            //Reset each Round.
            ++m_currentScoreTeam1;
            //Reset only on Disable.
            ++m_totalScoreTeam1;

            CheckMatchConditions(m_lastTouchedPlayerT1, m_currentScoreTeam1);
        }

        public void SetLastTouch(int _playerIndex)
        {
            if (_playerIndex % 0 == 0)
            {
                //Team 1 Index 0 & Index 2.
                m_lastTouchedPlayerT1 = m_playerProfiles[_playerIndex].PlayerName;
                //m_lastTouchTeam1 = _playerIndex;
            }
            else
            {
                //Team 2 Index 1 & Index 3
                m_lastTouchedPlayerT2 = m_playerProfiles[_playerIndex].PlayerName;
                //m_lastTouchTeam2 = _playerIndex;
            }
        }

        //private void ReSetMatch()
        //{
        //    BuildPlayField();
        //    ResetRoundValues();
        //    SetPauseAndTimeScale(m_gameIsPaused);
        //}

        //private void ResetRoundValues()
        //{
        //    m_currentScoreTeam1 = 0;
        //    m_currentScoreTeam2 = 0;
        //}

        private void CheckMatchConditions(string _winningPlayer, int _pointCount)
        {
            Debug.Log($"{_winningPlayer} scored. Congratulations!");

            //If either no max Round or max Point amount is set, then there shall be no next Round.
            if (m_matchData.EGameMode == EGameMode.Infinite)
            {
                m_nextRoundConditionIsMet = false;
                return;
            }
            else
            {
                m_nextRoundConditionIsMet =
                m_currentScoreTeam1 >= m_matchData.PointsEachRound &&
                m_currentScoreTeam1 >= m_currentScoreTeam2 + m_matchData.WinPointDifference
                ||
                m_currentScoreTeam2 >= m_matchData.PointsEachRound &&
                m_currentScoreTeam2 >= m_currentScoreTeam1 + m_matchData.WinPointDifference;
            }

            //WinCondition is true, when the current RoundNumber equals the max set roundAmount AND the winPoint-Difference (PlayerCharacter 1 <-> PlayerCharacter 2) triggers a new round.
            bool winConditionIsMet = m_currentRound == m_matchData.RoundsToWin && m_nextRoundConditionIsMet;

            if (winConditionIsMet)
            {
                ProcessMatchDetails(_winningPlayer, _pointCount);
                return;
            }

            switch (m_nextRoundConditionIsMet)
            {
                case true:
                {
                    AStartNextRound?.Invoke();
                    break;
                }
                case false:
                {
                    break;
                }
            }
        }

        private void ProcessMatchDetails(string _winningPlayer, int _finalScore)
        {
            var matchResult = new MatchResult();
            matchResult.WinnerName = _winningPlayer;
            matchResult.FinalScore = _finalScore.ToString();
            TimeSpan playTimeSpan = TimeSpan.FromSeconds(Time.time - m_matchStartTime);
            matchResult.TotalPlayTime = playTimeSpan.TotalSeconds.ToString();
            matchResult.MatchWinDate = $"{DateTime.Today.ToShortDateString()}\n" + string.Format("{0:00}:{1:00}:{2:00}", DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);

            //TODO: Submit Result to HighScoreBoard.cs.

            AStartWinProcedure?.Invoke();
            //ALoadUpHighScores?.Invoke();
        }

        /// <summary>
        /// Method called by a Button that is hidden in the 'Pause Menu', until the match is in "Infinity-Mode".
        /// </summary>
        private void LetsEndInfiniteMatch()
        {
            if (m_totalScoreTeam1 <= 0 || m_totalScoreTeam2 <= 0)
            {
                Debug.Log("No points gained, yet!");
                //TODO: PopUp-Window: "No points gained, yet.
                return;
            }

            var matchResult = new MatchResult();
            //TODO: Return WinnerName & FinalScore! Per MemberVariable?
            matchResult.WinnerName = "";
            matchResult.FinalScore = GetHigherPlayerScore(m_totalScoreTeam1, m_totalScoreTeam2).ToString();
            TimeSpan playTimeSpan = TimeSpan.FromSeconds(Time.time - m_matchStartTime);
            matchResult.TotalPlayTime = playTimeSpan.TotalSeconds.ToString();
            matchResult.MatchWinDate = $"{DateTime.Today.ToShortDateString()}\n" + string.Format("{0:00}:{1:00}:{2:00}", DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);

            //ALoadUpHighScores?.Invoke();
        }

        /// <summary>
        /// Method to get the Higher Value for Infinite Matches.
        /// </summary>
        /// <param name="_infinitePointsTPOne"></param>
        /// <param name="_infinitePointsTPTwo"></param>
        /// <returns></returns>
        private double GetHigherPlayerScore(double _infinitePointsTPOne, double _infinitePointsTPTwo)
        {
            //if (_infinitePointsTPOne != _infinitePointsTPTwo)
            //{
            //    bool higherPoints = _infinitePointsTPOne > _infinitePointsTPTwo;
            //    switch (higherPoints)
            //    {
            //        case true:
            //        {
            //            //Team One.
            //            m_matchValues.WinningPlayer = $"{m_matchValues.PlayerSOData[0].PlayerName} & {m_matchValues.PlayerSOData[2].PlayerName}";
            //            return _infinitePointsTPOne;
            //        }
            //        case false:
            //        {
            //            //Team Two.
            //            m_matchValues.WinningPlayer = $"{m_matchValues.PlayerSOData[1].PlayerName} & {m_matchValues.PlayerSOData[3].PlayerName}";
            //            return _infinitePointsTPTwo;
            //        }
            //    }
            //}
            //else
            //{
            //    m_matchValues.WinningPlayer = $"{m_matchValues.PlayerSOData[0].PlayerName} & {m_matchValues.PlayerSOData[2].PlayerName} draw \nto {m_matchValues.PlayerSOData[1].PlayerName} & {m_matchValues.PlayerSOData[3].PlayerName}";
            return _infinitePointsTPOne;
            //}
        }

        private void LetsStartNextRound()
        {
            //TODO: May implement a procedure to transition into the next set Round, if desired.

            //Increase the RoundNr by 1.
            m_currentRound++;
            m_currentScoreTeam1 = 0;
            m_currentScoreTeam2 = 0;
            //ResetRoundValues();
        }

        private void LetsStartWinProcedure()
        {
#if UNITY_EDITOR
            Debug.Log("Won!");
#endif
            //TODO: Start the WinProcedure.
        }

        private void OnReloadScene(int _sceneIndex)
        {
            SetPauseAndTimeScale(false);
        }

        private void OnResumeGame()
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

        private void PauseAndTimeScale(string _actionMap)
        {
            if (_actionMap == EInputActionMaps.UserInterface.ToString())
                SetPauseAndTimeScale(true);
        }
        #endregion
    }
}