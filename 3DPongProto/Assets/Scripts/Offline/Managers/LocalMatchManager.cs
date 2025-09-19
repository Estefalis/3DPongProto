using System;
using System.Collections.Generic;
using ThreeDeePongProto.Offline.UI.Menu;
using ThreeDeePongProto.Shared.Highscores;
using ThreeDeePongProto.Shared.PlayerCharacter;
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
    public int FinalScore;
    public float TotalPlayTime;
    public long MatchWinDate;
}

namespace ThreeDeePongProto.Shared.Managers
{
    public class LocalMatchManager : MonoBehaviour
    {
        [SerializeField] private MenuManager m_menuManager;
        public static LocalMatchManager Instance { get; private set; }

        [Header("Scene & Prefab References")]
        [SerializeField] private string m_NPCName = "Some Test-NPC";    //TODO: Implement NPC for solo play!
        [SerializeField] private Transform m_prefabParent;
        [SerializeField] private GameObject m_playGround;
        [SerializeField] private GameObject m_ballPrefab;
        [SerializeField] private HighScoreBoard m_highScoreBoard;

        #region Game Rules & Constants
        private const float m_playGroundWidthScale = 0.1f;
        private const float m_playGroundLengthScale = 0.1f;
        public static readonly Vector3 DEFAULT_PADDLE_SCALE = new Vector3(3.5f, 1.0f, 0.5f);
        #endregion

        #region Private-States
        //Match-Config
        private MatchSettingsData m_matchData;
        private List<PlayerProfileData> m_playerProfiles;
        private Vector3 m_ballPopPos;

        //Match-Runtime-Values
        private int m_currentScoreTeam1, m_totalScoreTeam1;
        private int m_currentScoreTeam2, m_totalScoreTeam2;
        private int m_currentRound;
        private string m_lastTouchedPlayerT1, m_lastTouchedPlayerT2, m_infiniteWinner;
        //private CharacterMainController m_lastTouchTeam1, m_lastTouchTeam2;

        private float m_matchStartTime, m_ballYPos;

        private bool m_gameIsPaused = false;
        private bool m_matchIsActive = false;
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
        public bool MatchIsActive { get => m_matchIsActive; private set => m_matchIsActive = value; }
        public float MatchStartTime { get => m_matchStartTime; private set => m_matchStartTime = value; }
        public float PaddleWidthAdjustStep { get => m_paddleWidthAdjustStep; }
        public bool GameIsPaused { get => m_gameIsPaused; }
        #endregion

        #region Actions
        public event Action<int, int> OnScoreChanged;               //Updates scoreTeam1, scoreTeam2 for subscribers
        public event Action<int> OnRoundChanged;                    //Updates currentRound for subscribers

        public event Action<MatchResult, bool> OnLocalMatchEnd;     //Sends: MatchResult
        #endregion

        private void Awake()
        {
            Instance = this;
        }

        private void OnEnable()
        {
            MenuManager.AEndInfiniteMatch += EndInfiniteMatch;
            UserInputManager.AChangeActiveActionMap += ToggleMatchPause;
            PlayerInputManager.instance.onPlayerJoined += OnPlayerJoined;
        }

        private void OnDisable()
        {
            MenuManager.AEndInfiniteMatch -= EndInfiniteMatch;
            UserInputManager.AChangeActiveActionMap -= ToggleMatchPause;
            if (PlayerInputManager.instance != null)
                PlayerInputManager.instance.onPlayerJoined -= OnPlayerJoined;

            Ball.OnFirstServe -= OnMatchActive;
            Ball.OnHitPlayer -= SetLastTouch;
            Ball.OnHitGoalOne -= OnBallHitsTeam1Goal;
            Ball.OnHitGoalTwo -= OnBallHitsTeam2Goal;
        }

        private void OnDestroy()
        {
            MenuManager.AEndInfiniteMatch -= EndInfiniteMatch;

            Ball.OnFirstServe -= OnMatchActive;
            Ball.OnHitPlayer -= SetLastTouch;
            Ball.OnHitGoalOne -= OnBallHitsTeam1Goal;
            Ball.OnHitGoalTwo -= OnBallHitsTeam2Goal;

            UserInputManager.AChangeActiveActionMap -= ToggleMatchPause;
        }

        private void Start()
        {
            var settingsManager = SettingsManager.Instance;
            var profileManager = PlayerProfileManager.Instance;

            //Manager-Security-Checks
            if (settingsManager == null || profileManager == null)
            {
                Debug.LogError("One or more required Manager were not found!");
                //Optional: Load StartMenu-Scene.
                return;
            }

            m_matchData = settingsManager.CurrentSettings.Match;
            m_playerProfiles = profileManager.PlayerProfiles;

            Ball.OnFirstServe += OnMatchActive;
            Ball.OnHitPlayer += SetLastTouch;
            Ball.OnHitGoalOne += OnBallHitsTeam1Goal;
            Ball.OnHitGoalTwo += OnBallHitsTeam2Goal;

            SetupMatch();
        }

        private void OnPlayerJoined(PlayerInput _playerInput)
        {
            int playerIndex = _playerInput.playerIndex;

            EPlayerLine line = m_matchData.PlayerPositions[playerIndex];
            float zSide = (playerIndex % 2 == 0) ? -1f : 1f;
            float halfFieldLength = m_matchData.FieldLength / 2f;
            float lineDistance = (line == EPlayerLine.Backline) ? m_matchData.BacklineDistance : m_matchData.FrontlineDistance;
            float zPos = zSide * (halfFieldLength - lineDistance); //Sets distance of players X-MoveLine.

            Vector3 spawnPosition = new(0, _playerInput.transform.position.y + 0.5f, zPos);
            //Quaternion spawnRotation = Quaternion.LookRotation(new Vector3(0, 0, -zSide));
            Quaternion spawnRotation = Quaternion.Euler(0, zSide > 0 ? 180f : 0f, 0);

            //PlayerBase position and rotation
            _playerInput.transform.SetPositionAndRotation(spawnPosition, spawnRotation);
            
            //var avatar = _playerInput.GetComponentInChildren<CharacterMovement>();
            //avatar.transform.localRotation = spawnRotation;
            var rb = _playerInput.GetComponentInChildren<Rigidbody>();
            rb.transform.localRotation = spawnRotation;
        }

        #region Custom-Methods        
        private void SetupMatch()
        {
            m_currentScoreTeam1 = 0;
            m_totalScoreTeam1 = 0;
            m_currentScoreTeam2 = 0;
            m_totalScoreTeam2 = 0;
            m_currentRound = 1;

            m_matchIsActive = false;
            //Time.timeScale & m_gameIsPaused gets set by UserInputManager & ToggleMatchPause on ActionMap switch!

            BuildPlayField();
            SpawnBall();
            UserInputManager.Instance.SpawnPlayersForMatch(m_matchData.PlayerCount);

            OnScoreChanged?.Invoke(m_totalScoreTeam1, m_totalScoreTeam2);
            OnRoundChanged?.Invoke(m_currentRound);
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

        private void OnMatchActive()
        {
            m_matchIsActive = true;
            m_matchStartTime = Time.time;   //OR StartDateTime = DateTime.Now.Ticks;
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

        private void OnBallHitsTeam1Goal()
        {
            //Reset each Round.
            ++m_currentScoreTeam2;
            //Reset only on Disable.
            ++m_totalScoreTeam2;

            OnScoreChanged?.Invoke(m_totalScoreTeam1, m_totalScoreTeam2);
            CheckMatchConditions(m_lastTouchedPlayerT2, m_currentScoreTeam2);
        }

        private void OnBallHitsTeam2Goal()
        {
            //Reset each Round.
            ++m_currentScoreTeam1;
            //Reset only on Disable.
            ++m_totalScoreTeam1;

            OnScoreChanged?.Invoke(m_totalScoreTeam1, m_totalScoreTeam2);
            CheckMatchConditions(m_lastTouchedPlayerT1, m_currentScoreTeam1);
        }

        private void CheckMatchConditions(string _winningPlayer, int _pointCount)
        {
            Debug.Log($"{_winningPlayer} scored. Congratulations!");

            bool roundIsOver = false;
            if (m_matchData.EGameMode == EGameMode.Infinite)
                return;

            if (m_matchData.EGameMode == EGameMode.SuddenDeath)
            {
                roundIsOver = m_currentScoreTeam1 >= 1 || m_currentScoreTeam2 >= 1;
            }
            else
            {
                //Normal Mode
                roundIsOver = (m_currentScoreTeam1 >= m_matchData.RoundPoints && m_currentScoreTeam1 >= m_currentScoreTeam2 + m_matchData.WinPointDifference) || (m_currentScoreTeam2 >= m_matchData.RoundPoints && m_currentScoreTeam2 >= m_currentScoreTeam1 + m_matchData.WinPointDifference);
            }

            if (roundIsOver)
            {
                if (m_currentRound >= m_matchData.RoundsToWin)
                    ProcessMatchResult(_winningPlayer, _pointCount);
                else
                    StartNextRound();
            }
        }

        private void StartNextRound()
        {
            //Optional: Transition into the next set Round.
            //Increase the RoundNr by 1 and reset current round's player Scores.
            m_currentRound++;
            m_currentScoreTeam1 = 0;
            m_currentScoreTeam2 = 0;

            OnRoundChanged?.Invoke(m_currentRound); //Time to reset Ball and Player.
        }

        private void ProcessMatchResult(string _winningPlayer, int _finalScore)
        {
            PauseMatch(false);

            var matchResult = new MatchResult();
            matchResult.WinnerName = _winningPlayer;
            matchResult.FinalScore = _finalScore;
            //TimeSpan playTimeSpan = TimeSpan.FromSeconds(Time.time - m_matchStartTime);
            matchResult.TotalPlayTime = Time.time - m_matchStartTime;
            //matchResult.MatchWinDate = $"{DateTime.Today.ToShortDateString()}\n" + string.Format("{0:00}:{1:00}:{2:00}", DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
            matchResult.MatchWinDate = DateTime.UtcNow.Ticks;

            OnLocalMatchEnd?.Invoke(matchResult, true);    //isMatchResult
        }

        /// <summary>
        /// Method called by a Button that is hidden in the 'Pause Menu', until the match is in "Infinity-Mode".
        /// </summary>
        private void EndInfiniteMatch()
        {
            if (m_totalScoreTeam1 <= 0 || m_totalScoreTeam2 <= 0)
            {
                Debug.Log("No points gained, yet Loading StartMenu.");    //Optional: PopUp-Window, to ask for playerResponse.
                m_menuManager.ReturnToMainMenu();
                return;
            }

            PauseMatch(true);

            var matchResult = new MatchResult();
            matchResult.FinalScore = GetHigherPlayerScore(m_totalScoreTeam1, m_totalScoreTeam2);
            matchResult.WinnerName = m_infiniteWinner;  //WinnerName AFTER FinalScore, because atm it gets set in GetHigherPlayerScore.
            matchResult.TotalPlayTime = Time.time - m_matchStartTime;
            matchResult.MatchWinDate = DateTime.UtcNow.Ticks;

            OnLocalMatchEnd?.Invoke(matchResult, true);    //isMatchResult
        }

        /// <summary>
        /// Method to get the Higher Value for Infinite Matches.
        /// </summary>
        /// <param name="_scoreTeam1"></param>
        /// <param name="_scoreTeam2"></param>
        /// <returns></returns>
        private int GetHigherPlayerScore(int _scoreTeam1, int _scoreTeam2)
        {
            if (_scoreTeam1 > _scoreTeam2)
            {
                //Team 1 wins
                m_infiniteWinner = (m_matchData.PlayerCount == 4)
                    ? $"{m_playerProfiles[0].PlayerName} & {m_playerProfiles[2].PlayerName}"
                    : m_playerProfiles[0].PlayerName;
            }
            else if (_scoreTeam2 > _scoreTeam1)
            {
                //Team 2 wins
                m_infiniteWinner = (m_matchData.PlayerCount == 4)
                    ? $"{m_playerProfiles[1].PlayerName} & {m_playerProfiles[3].PlayerName}"
                    : m_playerProfiles[1].PlayerName;
            }
            else
            {
                //Draw
                string team1Name = (m_matchData.PlayerCount == 4) ? $"{m_playerProfiles[0].PlayerName} & {m_playerProfiles[2].PlayerName}" : m_playerProfiles[0].PlayerName;
                string team2Name = (m_matchData.PlayerCount == 4) ? $"{m_playerProfiles[1].PlayerName} & {m_playerProfiles[3].PlayerName}" : m_playerProfiles[1].PlayerName;
                m_infiniteWinner = $"{team1Name} draw \nto {team2Name}";
            }

            //Uses Mathf.Max to get the highest Score.
            return Mathf.Max(_scoreTeam1, _scoreTeam2);
        }

        private void ToggleMatchPause(string _actionMap)
        {
            m_gameIsPaused = _actionMap == EInputActionMaps.UserInterface.ToString();
            PauseMatch(m_gameIsPaused);
        }

        private void PauseMatch(bool _isPaused)
        {
            Time.timeScale = _isPaused ? 0f : 1f;
            m_gameIsPaused = _isPaused;
        }
        #endregion
    }
}