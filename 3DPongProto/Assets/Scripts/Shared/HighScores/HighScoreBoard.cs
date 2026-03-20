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
        private enum EFilterMode { InfiniteMatch, SuddenDeath, Normal }
        private enum ESortColumn { Rounds, Points, TotalPoints, Playtime, WinDate }

        // [SerializeField] private MenuManager m_menuManager;
        [SerializeField] private MenuNavigation m_menuNavigation;

        [Header("Hierarchy References")]
        [SerializeField] private Transform m_listFrame;                 //Disable Transform.
        [SerializeField] private Transform m_contentParent;
        [Header("Entry Prefabs")]
        [SerializeField] private GameObject m_entryPrefab;
        [SerializeField] private GameObject m_noDataPrefab;
        [Header("Sort Buttons")]
        [SerializeField] private Button m_roundButton;
        [SerializeField] private Button m_maxPointButton;
        [SerializeField] private Button m_totalPointButton;
        [SerializeField] private Button m_playtimeButton;
        [SerializeField] private Button m_winDateButton;
        [Header("Filter Dropdowns")]
        [SerializeField] private TMP_Dropdown m_roundsDropdown;
        [SerializeField] private TMP_Dropdown m_maxPointsDropdown;
        [SerializeField] private TMP_Dropdown m_filterModeDropdown;

        //Filtering
        private EFilterMode m_currentFilterMode = EFilterMode.Normal;
        private int m_filterRoundsValue = 0;
        private int m_filterPointsValue = 0;
        private bool m_sortDropdownsActive = true;

        //Sorting
        private ESortColumn m_currentSortColumn = ESortColumn.WinDate;
        private bool m_isSortAscending = false; //sort Low to High

        private bool m_isMatchResult = false;   //isPostMatch Result

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
            if (!m_isMatchResult)
            {
                //Get settings from the last Match.
                var lastMatchConfig = SettingsManager.Instance.CurrentSettings.Match;

                //Set Filter-State.
                m_currentFilterMode = (EFilterMode)lastMatchConfig.EGameMode;
                m_filterRoundsValue = lastMatchConfig.RoundsToWin;  //m_lastGameRounds?
                m_filterPointsValue = lastMatchConfig.RoundPoints;  //m_lastGamePoints?
            }

            RefreshDisplay();
            AddListeners();
        }

        private void OnDisable()
        {
            if (LocalMatchManager.Instance != null)
                LocalMatchManager.Instance.OnLocalMatchEnd -= OnMatchEnded;

            RemoveListeners();
        }

        private void Start()
        {
            //Subscription to display HighScore on Game end.
            if (LocalMatchManager.Instance != null)
                LocalMatchManager.Instance.OnLocalMatchEnd += OnMatchEnded;
        }

        private void AddListeners()
        {
            m_roundButton.onClick.AddListener(() => OnSortButtonClicked(ESortColumn.Rounds));
            m_maxPointButton.onClick.AddListener(() => OnSortButtonClicked(ESortColumn.Points));
            m_totalPointButton.onClick.AddListener(() => OnSortButtonClicked(ESortColumn.TotalPoints));
            m_playtimeButton.onClick.AddListener(() => OnSortButtonClicked(ESortColumn.Playtime));
            m_winDateButton.onClick.AddListener(() => OnSortButtonClicked(ESortColumn.WinDate));

            m_roundsDropdown.onValueChanged.AddListener(OnRoundDropdownChanged);
            m_maxPointsDropdown.onValueChanged.AddListener(OnMaxPointDropdownChanged);
            m_filterModeDropdown.onValueChanged.AddListener(OnFilterModeChanged);
        }

        private void RemoveListeners()
        {
            m_roundButton.onClick.RemoveListener(() => OnSortButtonClicked(ESortColumn.Rounds));
            m_maxPointButton.onClick.RemoveListener(() => OnSortButtonClicked(ESortColumn.Points));
            m_totalPointButton.onClick.RemoveListener(() => OnSortButtonClicked(ESortColumn.TotalPoints));
            m_playtimeButton.onClick.RemoveListener(() => OnSortButtonClicked(ESortColumn.Playtime));
            m_winDateButton.onClick.RemoveListener(() => OnSortButtonClicked(ESortColumn.WinDate));

            m_roundsDropdown.onValueChanged.RemoveListener(OnRoundDropdownChanged);
            m_maxPointsDropdown.onValueChanged.RemoveListener(OnMaxPointDropdownChanged);
            m_filterModeDropdown.onValueChanged.RemoveListener(OnFilterModeChanged);
        }

        private void SetUIElements()
        {
            SetupFilterDropdown();

            m_sortDropdownsActive = m_currentFilterMode == EFilterMode.Normal && !m_isMatchResult;
            m_roundsDropdown.interactable = m_sortDropdownsActive;
            m_maxPointsDropdown.interactable = m_sortDropdownsActive;

            SetupRoundsDropdown();
            SetupMaxPointsDropdown();
        }

        #region Initial-Setup
        private void SetupFilterDropdown()
        {
            m_filterModeDropdown.ClearOptions();
            var filterOptionList = new List<string>();

            var enumMemberNames = Enum.GetNames(typeof(EFilterMode)); //Enum.GetNames(typeof(EFilterMode)).Length

            for (int i = 0; i < enumMemberNames.Length; i++)
                filterOptionList.Add($"{enumMemberNames[i]}");

            m_filterModeDropdown.AddOptions(filterOptionList);
            m_filterModeDropdown.SetValueWithoutNotify((int)m_currentFilterMode);
        }

        private void SetupRoundsDropdown()
        {
            m_roundsDropdown.ClearOptions();
            var roundsDdList = new List<string> { "\u221E" };

            for (int i = 1; i <= m_maxRounds; i++)
                roundsDdList.Add(i.ToString());

            m_roundsDropdown.AddOptions(roundsDdList);
            m_roundsDropdown.SetValueWithoutNotify(m_isMatchResult ? m_filterRoundsValue : (int)m_currentFilterMode);
        }

        private void SetupMaxPointsDropdown()
        {
            m_maxPointsDropdown.ClearOptions();
            var maxPointsDdList = new List<string> { "\u221E" };

            for (int i = 1; i <= m_maxPoints; i++)
                maxPointsDdList.Add(i.ToString());

            m_maxPointsDropdown.AddOptions(maxPointsDdList);
            m_maxPointsDropdown.SetValueWithoutNotify(m_isMatchResult ? m_filterPointsValue : (int)m_currentFilterMode);
        }
        #endregion

        #region Listener-Methods
        private void OnSortButtonClicked(ESortColumn _column)
        {
            if (_column == m_currentSortColumn)
                m_isSortAscending = !m_isSortAscending;     //Invert Sort-order
            else
            {
                m_currentSortColumn = _column;
                m_isSortAscending = false;                  //Standard sortOption is counting from HighToLow
            }

            RefreshDisplay();
        }

        private void OnRoundDropdownChanged(int _dropdownIndex)
        {
            m_filterRoundsValue = _dropdownIndex;
            RefreshDisplay();
        }

        private void OnMaxPointDropdownChanged(int _dropdownIndex)
        {
            m_filterPointsValue = _dropdownIndex;
            RefreshDisplay();
        }

        private void OnFilterModeChanged(int _dropdownIndex)
        {
            m_isMatchResult = false;
            m_currentFilterMode = (EFilterMode)_dropdownIndex;

            RefreshDisplay();
        }
        #endregion

        private void OnMatchEnded(MatchResult _matchResult)
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

            //Set postMatch state
            m_isMatchResult = true;
            //Set FilterOptions
            m_currentFilterMode = (EFilterMode)newEntry.GameMode;
            m_filterRoundsValue = newEntry.RoundSetting;
            m_filterPointsValue = newEntry.PointSetting;
            //Set SortOptions
            m_currentSortColumn = ESortColumn.WinDate;
            m_isSortAscending = false;

            //First use MenuManager's <Transform-key, SelectObject-value> dict to activate the HighScoreBoard (Transform).
            m_menuNavigation.NextElement(m_listFrame);     //listFrame.gO.SetActive = true.
            //Then tell the UserInputManager to switch the active InputActionMap to UI to skip the MenuManager OnOpenMenu() Method.
            UserInputManager.Instance.ToggleActionMaps(EInputActionMaps.UserInterface.ToString());

            RefreshDisplay();
        }

        /// <summary>
        /// Central Method, to filter, sort and show the list.
        /// </summary>
        public void RefreshDisplay()
        {
            //Fresh (Re-)start
            foreach (Transform child in m_contentParent)
                Destroy(child.gameObject);

            SetUIElements();

            //Filtering & Sorting
            List<HighScoreEntry> filteredScores = FilterScores();
            List<HighScoreEntry> sortedScores = SortScores(filteredScores);

            DisplayScores(sortedScores);
        }

        private void DisplayScores(List<HighScoreEntry> _scores)
        {
            //Show final sortResult
            if (_scores.Count == 0)
                Instantiate(m_noDataPrefab, m_contentParent);
            else
            {
                for (int i = 0; i < _scores.Count; i++)
                {
                    var entryObject = Instantiate(m_entryPrefab, m_contentParent);
                    entryObject.GetComponent<HighScoreEntrySlot>().Initialize(i + 1, _scores[i]);
                }
            }
        }

        #region Filtering_&_Sorting
        private List<HighScoreEntry> FilterScores()
        {
            IEnumerable<HighScoreEntry> filteredScores = m_highScoreData.highScores;
            EFilterMode selectedFilter = (EFilterMode)m_filterModeDropdown.value;

            if (m_isMatchResult)
            {
                return filteredScores.Where(s =>
                    s.GameMode == (EGameMode)m_currentFilterMode &&
                    s.RoundSetting == m_filterRoundsValue &&
                    s.PointSetting == m_filterPointsValue
                ).ToList();
            }

            filteredScores = filteredScores.Where(s => s.GameMode == (EGameMode)m_currentFilterMode);

            if (m_currentFilterMode == EFilterMode.Normal)
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
                case ESortColumn.Rounds:
                    sorted = m_isSortAscending ? _highScores.OrderBy(s => s.RoundSetting) : _highScores.OrderByDescending(s => s.RoundSetting);
                    break;
                case ESortColumn.Points:
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