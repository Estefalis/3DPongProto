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
        private enum EFilterMode
        {
            InfiniteMatch = 0,
            SuddenDeath = 1,
            SetRounds,
            SetPoints,
            TotalPoints,
            TotalPlaytime,
            MatchWinDate,
            ShowAll
        }

        [SerializeField] private MenuManager m_menuManager;

        [Header("UI References")]
        [SerializeField] private Transform m_listFrame;     //Disable Transform.
        [SerializeField] private Transform m_contentParent;
        [SerializeField] private GameObject m_entryPrefab;
        [SerializeField] private GameObject m_noDataPrefab;
        [SerializeField] private Button[] m_sortButtons;
        [SerializeField] private TMP_Dropdown m_roundsDropdown;
        [SerializeField] private TMP_Dropdown m_maxPointsDropdown;
        [SerializeField] private TMP_Dropdown m_filterModeDropdown;

        //Sorting
        private EFilterMode m_currentFilterMode = EFilterMode.SetRounds;
        private bool _isSortAscending = false;              //sort Low to High
        private bool m_noFiltering = true;

        //LastMatchConfig
        private EGameMode m_lastGameMode;
        private int m_lastGameRounds;
        private int m_lastGamePoints;
        private bool m_isMatchResult = false;

        private readonly int m_maxRounds = 5;
        private readonly int m_maxPoints = 25;
        private int m_lastRound;
        private int m_lastPoints;

        private HighScoreData _highScoreData;
        private IPersistentData<HighScoreData> _saveSystem;

        private const string m_fileName = "highscores.json";
        private const string m_subFolder = "/SaveData/";

        private void Awake()
        {
            _saveSystem = new SerializingData<HighScoreData>(m_fileName, m_subFolder);
            _highScoreData = _saveSystem.Load();
        }

        private void OnEnable()
        {
            //Subscription to display HighScore on Game end.
            LocalMatchManager.Instance.OnLocalMatchEnd += DisplayResult;

            SetUIElements();
        }

        private void OnDisable()
        {
            if (LocalMatchManager.Instance != null)
                LocalMatchManager.Instance.OnLocalMatchEnd -= DisplayResult;

            RemoveListeners();
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

            m_noFiltering = m_currentFilterMode == EFilterMode.ShowAll;
            m_roundsDropdown.interactable = m_noFiltering;
            m_maxPointsDropdown.interactable = m_noFiltering;

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
            m_filterModeDropdown.SetValueWithoutNotify((int)m_currentFilterMode);
        }

        private void SetupRoundsDropdown()
        {
            m_roundsDropdown.ClearOptions();
            var roundsDdList = new List<string> { "\u221E" };

            for (int i = 1; i <= m_maxRounds; i++)
                roundsDdList.Add(i.ToString());

            m_roundsDropdown.AddOptions(roundsDdList);
            m_roundsDropdown.SetValueWithoutNotify(m_isMatchResult ? m_lastGameRounds : m_maxRounds);
            m_lastRound = m_roundsDropdown.value;
        }

        private void SetupMaxPointsDropdown()
        {
            m_maxPointsDropdown.ClearOptions();
            var maxPointsDdList = new List<string> { "\u221E" };

            for (int i = 1; i <= m_maxPoints; i++)
                maxPointsDdList.Add(i.ToString());

            m_maxPointsDropdown.AddOptions(maxPointsDdList);
            m_maxPointsDropdown.SetValueWithoutNotify(m_isMatchResult ? m_lastGamePoints : m_maxPoints);
            m_lastPoints = m_maxPointsDropdown.value;
        }
        #endregion

        #region Listener-Methods
        private void OnSortButtonClicked(int _sortModeIndex)
        {
            var newSortMode = (EFilterMode)_sortModeIndex;
            if (newSortMode == m_currentFilterMode)
            {
                _isSortAscending = !_isSortAscending;   //Invert Sort-order
            }
            else
            {
                m_currentFilterMode = newSortMode;
                _isSortAscending = false;               //Standard sortOption is counting from HighToLow
            }

            RefreshDisplay();
        }

        private void OnFilterModeChanged(int _dropdownIndex)          //<-----------------------
        {
            m_currentFilterMode = (EFilterMode)_dropdownIndex;

            if (m_currentFilterMode == EFilterMode.ShowAll)
                m_isMatchResult = false;

            //TODO: Listener Logic

            RefreshDisplay();
        }

        private void OnRoundDropdownChanged(int _dropdownIndex)     //<-----------------------
        {
            if (m_isMatchResult)
                return;

            m_lastRound = _dropdownIndex;
            RefreshDisplay();
        }

        private void OnMaxPointDropdownChanged(int _dropdownIndex)  //<-----------------------
        {
            if (m_isMatchResult)
                return;

            m_lastPoints = _dropdownIndex;
            RefreshDisplay();
        }
        #endregion

        /// <summary>
        /// MatchResult true after Matches. Else false.
        /// </summary>
        /// <param name="_matchResult"></param>
        /// <param name="_isMatchResult"></param>
        private void DisplayResult(MatchResult _matchResult, bool _isMatchResult)
        {
            if (_isMatchResult == true)
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
                _highScoreData.highScores.Add(newEntry);
                _saveSystem.Save(_highScoreData);

                m_lastGameMode = matchSettings.EGameMode;
                m_lastGameRounds = matchSettings.RoundsToWin;
                m_lastGamePoints = matchSettings.RoundPoints;
            }

            m_isMatchResult = _isMatchResult;

            //First use MenuManager's <Transform-key, SelectObject-value> dict to activate the HighScoreBoard (Transform).
            m_menuManager.NextElement(m_listFrame);
            //Then tell the UserInputManager to switch the active InputActionMap to UI to skip the MenuManager OnOpenMenu() Method.
            UserInputManager.ToggleActionMaps(EInputActionMaps.UserInterface.ToString());

            RefreshDisplay();
        }

        /// <summary>
        /// Central Method, to filter, sort and show the list.
        /// </summary>
        public void RefreshDisplay()
        {
            //Delete all entries at start
            foreach (Transform child in m_contentParent)
                Destroy(child.gameObject);

            //Filter (Later!)
            List<HighScoreEntry> filteredScores = FilterScores(_highScoreData.highScores);

            //Sort by option
            List<HighScoreEntry> sortedScores = SortScores(filteredScores);

            //Show final sortResult
            if (sortedScores.Count == 0)
            {
                Instantiate(m_noDataPrefab, m_contentParent);
            }
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
            if (m_isMatchResult == true)
            {
                //SortOption accessable right after Matches. To filter and display HighScore with these identical settings.
                return _allScores.Where(s =>
                    s.GameMode == m_lastGameMode &&
                    s.RoundSetting == m_lastGameRounds &&
                    s.PointSetting == m_lastGamePoints
                ).ToList();
            }
            else
            {
                //SortOptions accessable outside of matches. To filter and display all HighScores.
                IEnumerable<HighScoreEntry> filteredScores = _allScores;
                EFilterMode selectedFilter = (EFilterMode)m_filterModeDropdown.value;

                switch (selectedFilter)
                {
                    case EFilterMode.InfiniteMatch:
                        filteredScores = filteredScores.Where(s => s.GameMode == EGameMode.Infinite);
                        break;
                    case EFilterMode.SuddenDeath:
                        filteredScores = filteredScores.Where(s => s.GameMode == EGameMode.SuddenDeath);
                        break;
                    case EFilterMode.SetRounds:
                        filteredScores = filteredScores.Where(s => s.RoundSetting == m_lastRound && s.GameMode != EGameMode.Infinite);
                        break;
                    case EFilterMode.SetPoints:
                        filteredScores = filteredScores.Where(s => s.PointSetting == m_lastPoints && s.GameMode != EGameMode.Infinite);
                        break;
                    case EFilterMode.TotalPoints:
                    case EFilterMode.TotalPlaytime:
                    case EFilterMode.MatchWinDate:
                    {
                        Debug.Log("Code for these FilterOptions still has to be implemented. Or SortButton-Components removed.");
                        filteredScores = _allScores;
                        break;
                    }
                    default:
                        break;
                }

                return filteredScores.ToList();
            }
        }

        private List<HighScoreEntry> SortScores(List<HighScoreEntry> _highScores)
        {
            IOrderedEnumerable<HighScoreEntry> sorted;
            switch (m_currentFilterMode)
            {
                case EFilterMode.SetRounds:
                    sorted = _isSortAscending ? _highScores.OrderBy(s => s.RoundSetting) : _highScores.OrderByDescending(s => s.RoundSetting);
                    break;
                case EFilterMode.SetPoints:
                    sorted = _isSortAscending ? _highScores.OrderBy(s => s.PointSetting) : _highScores.OrderByDescending(s => s.PointSetting);
                    break;
                case EFilterMode.TotalPoints:
                    sorted = _isSortAscending ? _highScores.OrderBy(s => s.TotalPoints) : _highScores.OrderByDescending(s => s.TotalPoints);
                    break;
                case EFilterMode.TotalPlaytime:
                    sorted = _isSortAscending ? _highScores.OrderBy(s => s.TotalPlaytime) : _highScores.OrderByDescending(s => s.TotalPlaytime);
                    break;
                case EFilterMode.MatchWinDate:
                {
                    sorted = _isSortAscending ? _highScores.OrderBy(s => s.MatchWinTimestamp) : _highScores.OrderByDescending(s => s.MatchWinTimestamp);
                    #region Idea
                    // sorted = _isSortAscending
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