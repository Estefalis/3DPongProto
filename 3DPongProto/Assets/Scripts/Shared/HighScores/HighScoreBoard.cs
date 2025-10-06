using System;
using System.Collections.Generic;
using System.Linq;
using ThreeDeePongProto.Offline.UI.Menu;
using ThreeDeePongProto.Shared.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ThreeDeePongProto.Shared.Highscores
{
    public class HighScoreBoard : MonoBehaviour
    {
        private enum EFilterMode { ShowAll, InfiniteMatch, SuddenDeath, Normal }

        private enum ESortColumn { SetRounds, SetPoints, TotalPoints, Playtime, WinDate }

        [SerializeField] private MenuManager m_menuManager;

        [Header("UI References")]
        [SerializeField] private Transform m_listFrame;                 //Disable Transform.
        [SerializeField] private Transform m_contentParent;
        [SerializeField] private GameObject m_entryPrefab;
        [SerializeField] private GameObject m_noDataPrefab;
        [SerializeField] private Button[] m_sortButtons;
        [SerializeField] private TMP_Dropdown m_roundsDropdown;
        [SerializeField] private TMP_Dropdown m_maxPointsDropdown;
        [SerializeField] private TMP_Dropdown m_filterModeDropdown;

        //Filtering
        private EFilterMode m_currentFilterMode = EFilterMode.ShowAll;
        private int m_filterRoundsValue = 0; //ShowAll = 0
        private int m_filterPointsValue = 0; //ShowAll = 0
        private bool m_filterInteractable = true;

        //Sorting
        private ESortColumn m_currentSortColumn = ESortColumn.WinDate;
        private bool m_isSortAscending = false; //sort Low to High

        ////LastMatchConfig
        private bool m_isMatchResult = false;   //isPostMatch Result
        private EGameMode m_lastGameMode;
        private int m_lastGameRounds;
        private int m_lastGamePoints;

        private readonly int m_maxRounds = 5;
        private readonly int m_maxPoints = 25;

        //SaveData
        private HighScoreData m_highScoreData;
        private IPersistentData<HighScoreData> m_saveSystem;
        private const string m_fileName = "highscores.json";
        private const string m_subFolder = "/SaveData/";

        private void Awake()
        {
            m_saveSystem = new SerializingData<HighScoreData>(m_fileName, m_subFolder);
            m_highScoreData = m_saveSystem.Load();
        }

        private void OnEnable()
        {
            m_currentFilterMode = EFilterMode.Normal;

            RefreshDisplay();     //AddListeners in RefreshDisplay() SetUIElements()
        }

        private void OnDisable()
        {
            if (LocalMatchManager.Instance != null)
                LocalMatchManager.Instance.OnLocalMatchEnd -= DisplayHighScores;

            RemoveListeners();
        }

        private void Start()
        {
            //Subscription to display HighScore on Game end.
            if (LocalMatchManager.Instance != null)
                LocalMatchManager.Instance.OnLocalMatchEnd += DisplayHighScores;
        }

        private void AddListeners()
        {
            for (int i = 0; i < m_sortButtons.Length; i++)
            {
                int sortModeIndex = i + 2;  //SortButton-Array-Indices == EFilterMode-Indices + 2
                m_sortButtons[i].onClick.AddListener(() => OnSortButtonClicked(sortModeIndex));
            }

            m_roundsDropdown.onValueChanged.AddListener(OnRoundDropdownChanged);
            m_maxPointsDropdown.onValueChanged.AddListener(OnMaxPointDropdownChanged);
            m_filterModeDropdown.onValueChanged.AddListener(OnFilterModeChanged);
        }

        private void RemoveListeners()
        {
            for (int i = 0; i < m_sortButtons.Length; i++)
            {
                int sortModeIndex = i + 2;  //SortButton-Array-Indices == EFilterMode-Indices + 2
                m_sortButtons[i].onClick.RemoveListener(() => OnSortButtonClicked(sortModeIndex));
            }

            m_roundsDropdown.onValueChanged.RemoveListener(OnRoundDropdownChanged);
            m_maxPointsDropdown.onValueChanged.RemoveListener(OnMaxPointDropdownChanged);
            m_filterModeDropdown.onValueChanged.RemoveListener(OnFilterModeChanged);
        }

        private void SetUIElements()
        {
            RemoveListeners();

            SetupFilterDropdown();

            m_filterInteractable = /*!m_isMatchResult && */(m_currentFilterMode == EFilterMode.Normal || m_currentFilterMode == EFilterMode.ShowAll);
            m_roundsDropdown.interactable = m_filterInteractable;
            m_maxPointsDropdown.interactable = m_filterInteractable;

            SetupRoundsDropdown();
            SetupMaxPointsDropdown();

            AddListeners();
        }

        #region Initial-Setup
        private void SetupFilterDropdown()
        {
            m_filterModeDropdown.ClearOptions();
            var filterOptionList = new List<string>();

            var enumMemberNames = Enum.GetNames(typeof(EFilterMode)); //int enumCount = Enum.GetNames(typeof(EFilterMode)).Length

            for (int i = 0; i < enumMemberNames.Length; i++)
                filterOptionList.Add($"{enumMemberNames[i]}");

            m_filterModeDropdown.AddOptions(filterOptionList);
            //ShowAll is on Index 0. So + 1.
            m_filterModeDropdown.SetValueWithoutNotify(m_isMatchResult ? (int)m_lastGameMode + 1 : (int)m_currentFilterMode);
        }

        private void SetupRoundsDropdown()
        {
            m_roundsDropdown.ClearOptions();
            var roundsDdList = new List<string> { "\u221E" };

            for (int i = 1; i <= m_maxRounds; i++)
                roundsDdList.Add(i.ToString());

            m_roundsDropdown.AddOptions(roundsDdList);
            m_roundsDropdown.SetValueWithoutNotify(m_isMatchResult ? m_lastGameRounds : m_filterRoundsValue);
        }

        private void SetupMaxPointsDropdown()
        {
            m_maxPointsDropdown.ClearOptions();
            var maxPointsDdList = new List<string> { "\u221E" };

            for (int i = 1; i <= m_maxPoints; i++)
                maxPointsDdList.Add(i.ToString());

            m_maxPointsDropdown.AddOptions(maxPointsDdList);
            m_maxPointsDropdown.SetValueWithoutNotify(m_isMatchResult ? m_lastGamePoints : m_filterPointsValue);
        }
        #endregion

        #region Listener-Methods
        private void OnSortButtonClicked(int _sortColumnIndex)
        {
            var newSortMode = (ESortColumn)_sortColumnIndex;
            if (newSortMode == m_currentSortColumn)
            {
                m_isSortAscending = !m_isSortAscending;   //Invert Sort-order
            }
            else
            {
                m_currentSortColumn = newSortMode;
                m_isSortAscending = false;               //Standard sortOption is counting from HighToLow
            }

            RefreshDisplay();
        }

        private void OnFilterModeChanged(int _dropdownIndex)
        {
            //ShowAll = 0, InfiniteMatch = 1, SuddenDeath = 2, Normal = 3.
            m_currentFilterMode = (EFilterMode)_dropdownIndex;

            //if (m_currentFilterMode == EFilterMode.ShowAll && m_isMatchResult)
            //    m_isMatchResult = false;

            m_filterRoundsValue = 0; // Zurücksetzen bei Modus-Wechsel
            m_filterPointsValue = 0;

            RefreshDisplay();
        }

        private void OnRoundDropdownChanged(int _dropdownIndex)
        {
            //if (m_isMatchResult)
            //    return;

            m_filterRoundsValue = _dropdownIndex;
            RefreshDisplay();
        }

        private void OnMaxPointDropdownChanged(int _dropdownIndex)
        {
            //if (m_isMatchResult)
            //    return;

            m_filterPointsValue = _dropdownIndex;
            RefreshDisplay();
        }
        #endregion

        /// <summary>
        /// MatchResult true after Matches. Else false.
        /// </summary>
        /// <param name="_matchResult"></param>
        private void DisplayHighScores(MatchResult _matchResult)
        {
            //Creates a new entry out of the matchResult.
            var matchSettings = SettingsManager.Instance.CurrentSettings.Match;
            HighScoreEntry newEntry = new()
            {
                WinningPlayerName = _matchResult.WinnerName,
                TotalPoints = _matchResult.FinalScore,
                TotalPlaytime = _matchResult.TotalPlayTime,
                MatchWinTimestamp = DateTime.UtcNow.Ticks,
                GameMode = matchSettings.EGameMode,
                RoundSetting = matchSettings.RoundsToWin,
                PointSetting = matchSettings.RoundPoints
            };

            //Add Entry to the list
            m_highScoreData.highScores.Add(newEntry);
            m_saveSystem.Save(m_highScoreData);

            m_lastGameMode = matchSettings.EGameMode;
            m_lastGameRounds = matchSettings.RoundsToWin;
            m_lastGamePoints = matchSettings.RoundPoints;

            m_isMatchResult = true;

            //First use MenuManager's <Transform-key, SelectObject-value> dict to activate the HighScoreBoard (Transform).
            m_menuManager.NextElement(m_listFrame);     //listFrame.gO.SetActive = true.
            //Then tell the UserInputManager to switch the active InputActionMap to UI to skip the MenuManager OnOpenMenu() Method.
            UserInputManager.Instance.ToggleActionMaps(EInputActionMaps.UserInterface.ToString());

            RefreshDisplay();
        }

        /// <summary>
        /// Central Method, to filter, sort and show the list.
        /// </summary>
        public void RefreshDisplay()
        {
            SetUIElements();

            //Fresh (Re-)start
            foreach (Transform child in m_contentParent)
                Destroy(child.gameObject);

            //Filtering & Sorting
            List<HighScoreEntry> filteredScores = FilterScores(m_highScoreData.highScores);
            List<HighScoreEntry> sortedScores = SortScores(filteredScores);

            //Show final sortResult
            if (sortedScores.Count == 0)
                Instantiate(m_noDataPrefab, m_contentParent);
            else
            {
                for (int i = 0; i < sortedScores.Count; i++)
                {
                    var entryObject = Instantiate(m_entryPrefab, m_contentParent);
                    entryObject.GetComponent<HighScoreEntrySlot>().Initialize(i + 1, sortedScores[i]);
                }
            }
        }

        #region Filtering_&_Sorting
        private List<HighScoreEntry> FilterScores(List<HighScoreEntry> _allScores)
        {
            //SortOptions accessable outside of matches. To filter and display all HighScores.
            IEnumerable<HighScoreEntry> filteredScores = _allScores;
            EFilterMode selectedFilter = (EFilterMode)m_filterModeDropdown.value;

            if (m_currentFilterMode != EFilterMode.ShowAll)
            {
                filteredScores = filteredScores.Where(s => s.GameMode == (EGameMode)m_currentFilterMode);
            }

            if (m_currentFilterMode == EFilterMode.Normal || m_currentFilterMode == EFilterMode.ShowAll)
            {
                if (m_filterRoundsValue > 0)
                    filteredScores = filteredScores.Where(s => s.RoundSetting == m_filterRoundsValue);
                if (m_filterPointsValue > 0)
                    filteredScores = filteredScores.Where(s => s.PointSetting == m_filterPointsValue);
            }

            return filteredScores.ToList();
        }

        private List<HighScoreEntry> SortScores(List<HighScoreEntry> _highScores)
        {
            IOrderedEnumerable<HighScoreEntry> sorted;
            switch (m_currentSortColumn)
            {
                case ESortColumn.SetRounds:
                    sorted = m_isSortAscending ? _highScores.OrderBy(s => s.RoundSetting) : _highScores.OrderByDescending(s => s.RoundSetting);
                    break;
                case ESortColumn.SetPoints:
                    sorted = m_isSortAscending ? _highScores.OrderBy(s => s.PointSetting) : _highScores.OrderByDescending(s => s.PointSetting);
                    break;
                case ESortColumn.TotalPoints:
                    sorted = m_isSortAscending ? _highScores.OrderBy(s => s.TotalPoints) : _highScores.OrderByDescending(s => s.TotalPoints);
                    break;
                case ESortColumn.Playtime:
                    sorted = m_isSortAscending ? _highScores.OrderBy(s => s.TotalPlaytime) : _highScores.OrderByDescending(s => s.TotalPlaytime);
                    break;
                case ESortColumn.WinDate:
                {
                    sorted = m_isSortAscending ? _highScores.OrderBy(s => s.MatchWinTimestamp) : _highScores.OrderByDescending(s => s.MatchWinTimestamp);
                    #region Idea
                    // sorted = m_isSortAscending
                    //     ? _allScores.OrderBy(s => new DateTime(s.MatchWinTimestamp).Year)
                    //             .ThenBy(s => new DateTime(s.MatchWinTimestamp).Month)
                    //             .ThenBy(s => new DateTime(s.MatchWinTimestamp).Day)
                    //     : _allScores.OrderByDescending(s => new DateTime(s.MatchWinTimestamp).Year)
                    //             .ThenByDescending(s => new DateTime(s.MatchWinTimestamp).Month)
                    //             .ThenByDescending(s => new DateTime(s.MatchWinTimestamp).Day);
                    #endregion
                    break;
                }
                //Sorting by playerName isn't planned. But could be implemented by a InputField-PopUp search.
                default:
                    return _highScores;
            }

            return sorted.ToList();
        }
        #endregion
    }
}