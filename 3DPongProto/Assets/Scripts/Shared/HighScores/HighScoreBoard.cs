using System;
using System.Collections.Generic;
using System.Linq;
using ThreeDeePongProto.Offline.UI.Menu;
using ThreeDeePongProto.Shared.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ThreeDeePongProto.Shared.Highscores
{
    public class HighScoreBoard : MonoBehaviour
    {
        private enum EListSortMode
        {
            None,
            SetRounds,
            SetPoints,
            TotalPoints,
            MatchWinDate,
            TotalPlaytime
        }

        [SerializeField] private MenuManager m_menuManager;

        [Header("UI References")]
        [SerializeField] private Transform m_listFrame;     //Disable Transform.
        [SerializeField] private Transform m_contentParent;
        [SerializeField] private GameObject m_entryPrefab;
        [SerializeField] private GameObject m_noDataPrefab;
        [SerializeField] private Button[] m_sortButtons;
        [SerializeField] private TMP_Dropdown[] m_sortDropdowns;

        //[SerializeField] private Transform m_disableTransform;
        //[SerializeField] private EventSystem m_eventSystem;
        //[SerializeField] private TMP_Dropdown m_roundsDropdown;
        //[SerializeField] private TMP_Dropdown m_maxPointsDropdown;
        //[SerializeField] private Button m_finishButton;
        //[Space]

        //Sorting
        /*[SerializeField] */private EListSortMode _currentSortMode = EListSortMode.MatchWinDate;
        private bool _isSortAscending = false;              //m_sortLowToHigh

        private IPersistentData<HighScoreData> _saveSystem;
        private HighScoreData _highScoreData;

        private const string m_fileName = "highscores.json";
        private const string m_subFolder = "/SaveData/";

        //[SerializeField] private HighScoreEntrySlot m_highScoreEntryChildPrefab;
        //[SerializeField] private GameObject m_noDataPrefab;

        //[SerializeField] private MatchUIStates m_matchUIStates;
        //[SerializeField] private MatchValues m_matchValues;

        //[SerializeField] private bool m_sortLowToHigh;

        //[SerializeField] private EListSortMode m_listSortMode = EListSortMode.MatchWinDate;

        //private List<string> m_roundsDdList;
        //private List<string> m_maxPointsDdList;
        //private readonly int m_firstRoundOffset = 1;
        //private readonly int m_firstPointOffset = 1;
        //private int m_parentChildCount;

        #region Serialization
        //private readonly string m_highScoreListFolderPath = "/SaveData/HighScore Lists";
        //private readonly string m_highscoresFileName = "/Highscores";
        //private readonly string m_fileFormat = ".json";

        //private readonly IPersistentData m_persistentData = new SerializingData();
        //[SerializeField] private bool m_encryptionEnabled = false;
        #endregion

        private void Awake()
        {
            _saveSystem = new SerializingData<HighScoreData>(m_fileName, m_subFolder);
            _highScoreData = _saveSystem.Load();

            // Listener für die Sortier-Buttons hinzufügen
            for (int i = 0; i < m_sortButtons.Length; i++)
            {
                int sortModeIndex = i + 1; // +1, da 'None' an Position 0 der Enum ist
                m_sortButtons[i].onClick.AddListener(() => OnSortButtonClicked(sortModeIndex));
            }

            //SetupDropdowns();

            ////TODO: Action to join load these inside the game, so the highScore list can be used outside of matches.
            ////m_slotHeight = m_highScoreEntryChildPrefab.GetComponent<RectTransform>().rect.height;
            //m_parentChildCount = m_listFrame.GetComponent<Transform>().childCount;

            //m_eventSystem.SetSelectedGameObject(m_finishButton.gameObject);

            //if (m_disableTransform.gameObject.activeInHierarchy)
            //    m_disableTransform.gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            // Event vom LocalMatchManager abonnieren, um zu wissen, wann wir erscheinen sollen
            LocalMatchManager.Instance.OnMatchEnded += DisplayBoard;

            //LocalMatchManager.ALoadUpHighScores += LoadHighScoresOnGameEnd;
            //m_roundsDropdown.onValueChanged.AddListener(OnRoundDropdownChanges);
            //m_maxPointsDropdown.onValueChanged.AddListener(OnMaxPointDropdownChanges);
        }

        private void OnDisable()
        {
            gameObject.SetActive(false);    //TODO: Remove?

            if (LocalMatchManager.Instance != null)
                LocalMatchManager.Instance.OnMatchEnded -= DisplayBoard;

            //LocalMatchManager.ALoadUpHighScores -= LoadHighScoresOnGameEnd;
            //m_roundsDropdown.onValueChanged.RemoveListener(OnRoundDropdownChanges);
            //m_maxPointsDropdown.onValueChanged.RemoveListener(OnMaxPointDropdownChanges);
        }

        //private void SetupDropdowns()
        //{
        //    m_roundsDdList = new List<string>();
        //    m_maxPointsDdList = new List<string>();

        //    m_roundsDropdown.ClearOptions();
        //    //m_roundsDdList.Add("?");
        //    m_roundsDdList.Add("\u221E");
        //    for (int i = m_firstRoundOffset; i < m_matchUIStates.RoundsToWin + 1; i++)
        //    {
        //        m_roundsDdList.Add(i.ToString());
        //    }
        //    m_roundsDropdown.AddOptions(m_roundsDdList);
        //    //TODO: Option to set this dropdown value, when people want to check Highscores from the mainMenu.
        //    m_roundsDropdown.value = m_matchUIStates.LastRoundDdIndex;
        //    m_roundsDropdown.RefreshShownValue();

        //    m_maxPointsDropdown.ClearOptions();
        //    //m_maxPointsDdList.Add("?");
        //    m_maxPointsDdList.Add("\u221E");
        //    for (int i = m_firstPointOffset; i < m_matchUIStates.PointsEachRound + 1; i++)
        //    {
        //        //'m_maxPointsDropdown.options.Add (new Dropdown.OptionData() { text = variable });' in foreach-loops.
        //        m_maxPointsDdList.Add(i.ToString());
        //    }
        //    m_maxPointsDropdown.AddOptions(m_maxPointsDdList);
        //    //TODO: Option to set this dropdown value, when people want to check Highscores from the mainMenu.
        //    m_maxPointsDropdown.value = m_matchUIStates.LastMaxPointDdIndex;
        //    m_maxPointsDropdown.RefreshShownValue();
        //}

        ///// <summary>
        ///// Loads the HighScoreData with the current WinValue after the game ended.
        ///// </summary>
        //private void LoadHighScoresOnGameEnd()
        //{
        //    m_disableTransform.gameObject.SetActive(true);

        //    foreach (Transform child in m_listFrame)
        //        Destroy(child.gameObject);

        //    HighScoreData highScoreList = m_persistentData.LoadData<HighScoreData>($"{m_highScoreListFolderPath}/{m_matchUIStates.LastRoundDdIndex}/{m_matchUIStates.LastMaxPointDdIndex}", m_highscoresFileName, m_fileFormat, m_encryptionEnabled);

        //    if (highScoreList == null)
        //    {
        //        highScoreList = new HighScoreData();
        //        AddNewEntryDataSlot(highScoreList);
        //        return;
        //    }
        //    else
        //    {
        //        //SortMode None on the first time. (Loaded as saved.)
        //        SortListByEnum(highScoreList);

        //        m_parentChildCount = 0;
        //        foreach (HighScoreEntry highscores in highScoreList.highScores)
        //        {
        //            HighScoreEntrySlot highScoreEntrySlot = Instantiate(m_highScoreEntryChildPrefab, m_listFrame);

        //            int rank = +1 + m_parentChildCount++;
        //            string rankSuffix = rank switch
        //            {
        //                1 => $"{rank}st",
        //                2 => $"{rank}nd",
        //                3 => $"{rank}rd",
        //                _ => $"{rank}th",
        //            };

        //            highScoreEntrySlot.Initialize(rankSuffix, highscores.WinningPlayer, highscores.SetMaxRounds, highscores.SetMaxPoints, highscores.TotalPoints, highscores.MatchWinDate, highscores.TotalPlaytime);
        //        }

        //        AddNewEntryDataSlot(highScoreList);
        //    }
        //}

        //private void OnRoundDropdownChanges(int _roundValue)
        //{
        //    LoadHighscoresByDropdowns(_roundValue, m_maxPointsDropdown.value);
        //}

        //private void OnMaxPointDropdownChanges(int _maxPointValue)
        //{
        //    LoadHighscoresByDropdowns(m_roundsDropdown.value, _maxPointValue);
        //}

        ///// <summary>
        ///// Manual HighScoreLists-Switch by DropdownChanges, after the Game ended, to see other HighScoreLists.
        ///// </summary>
        ///// <param name="_roundValue"></param>
        ///// <param name="_maxPointValue"></param>
        //private void LoadHighscoresByDropdowns(int _roundValue, int _maxPointValue)
        //{
        //    foreach (Transform child in m_listFrame)
        //        Destroy(child.gameObject);

        //    HighScoreData highScoreList = m_persistentData.LoadData<HighScoreData>($"{m_highScoreListFolderPath}/{_roundValue}/{_maxPointValue}", m_highscoresFileName, m_fileFormat, m_encryptionEnabled);

        //    if (highScoreList == null)
        //    {
        //        _ = Instantiate(m_noDataPrefab, m_listFrame);  //_ replaces GameObject noDataNotification, if the GameObject isn't used.
        //        //noDataNotification.gameObject.SetActive(true);
        //        return;
        //    }

        //    //SortMode None on the first time. (Loaded as saved.)
        //    SortListByEnum(highScoreList);

        //    m_parentChildCount = 0;
        //    foreach (HighScoreEntry highscores in highScoreList.highScores)
        //    {
        //        HighScoreEntrySlot highScoreEntrySlot = Instantiate(m_highScoreEntryChildPrefab, m_listFrame);

        //        int rank = +1 + m_parentChildCount++;
        //        string rankSuffix = rank switch
        //        {
        //            1 => $"{rank}st",
        //            2 => $"{rank}nd",
        //            3 => $"{rank}rd",
        //            _ => $"{rank}th",
        //        };

        //        highScoreEntrySlot.Initialize(rankSuffix, highscores.WinningPlayer, highscores.SetMaxRounds, highscores.SetMaxPoints, highscores.TotalPoints, highscores.MatchWinDate, highscores.TotalPlaytime);
        //    }
        //}

        //private HighScoreData SortListByEnum(HighScoreData _highScoreList)
        //{
        //    switch (m_listSortMode)
        //    {
        //        case EListSortMode.None:
        //            break;
        //        case EListSortMode.SetRounds:
        //        { break; }
        //        case EListSortMode.SetPoints:
        //        { break; }
        //        case EListSortMode.TotalPoints:
        //        {
        //            m_sortLowToHigh = !m_sortLowToHigh;
        //            SortListByTotalPoints(_highScoreList);
        //            break;
        //        }
        //        case EListSortMode.MatchWinDate:
        //        {
        //            //The WinDate SortBehavior was partly strange. Sending the parameter here, while don't elsewhere, currently avoids bool setting errors. 
        //            m_sortLowToHigh = !m_sortLowToHigh;
        //            SortListByMatchWinDate(_highScoreList, m_sortLowToHigh);
        //            break;
        //        }
        //        case EListSortMode.TotalPlaytime:
        //        {
        //            m_sortLowToHigh = !m_sortLowToHigh;
        //            SortListByTotalPlaytime(_highScoreList);
        //            break;
        //        }
        //    }

        //    return _highScoreList;
        //}

        //private HighScoreData SortListByTotalPoints(HighScoreData _highScoreList)
        //{
        //    #region Linq-IfElse
        //    //if (m_sortLowToHigh)
        //    //    _highScoreList.highScores = _highScoreList.highScores.OrderBy(linqSorts => linqSorts.TotalPoints).ToList();
        //    //else
        //    //    _highScoreList.highScores = _highScoreList.highScores.OrderByDescending(linqSorts => linqSorts.TotalPoints).ToList();

        //    //return _highScoreList;
        //    #endregion

        //    #region Linq-Switch
        //    switch (m_sortLowToHigh)
        //    {
        //        case true:
        //            _highScoreList.highScores = _highScoreList.highScores.OrderBy(linqSorts => linqSorts.TotalPoints).ToList();
        //            break;
        //        case false:
        //            _highScoreList.highScores = _highScoreList.highScores.OrderByDescending(linqSorts => linqSorts.TotalPoints).ToList();
        //            break;
        //    }

        //    return _highScoreList;
        //    #endregion
        //}

        //private HighScoreData SortListByMatchWinDate(HighScoreData _highScoreList, bool _sortLowToHigh)
        //{
        //    #region Linq-IfElse
        //    //if (m_sortLowToHigh)
        //    //    _highScoreList.highScores = _highScoreList.highScores.OrderBy(linqSorts => linqSorts.MatchWinDate).ToList();
        //    //else
        //    //    _highScoreList.highScores = _highScoreList.highScores.OrderByDescending(linqSorts => linqSorts.MatchWinDate).ToList();

        //    //return _highScoreList;
        //    #endregion

        //    #region Linq-Switch
        //    switch (_sortLowToHigh)
        //    {
        //        case true:
        //            _highScoreList.highScores = _highScoreList.highScores.OrderBy(linqSorts => linqSorts.MatchWinDate).ToList();
        //            break;
        //        case false:
        //            _highScoreList.highScores = _highScoreList.highScores.OrderByDescending(linqSorts => linqSorts.MatchWinDate).ToList();
        //            break;
        //    }

        //    return _highScoreList;
        //    #endregion
        //}

        //private HighScoreData SortListByTotalPlaytime(HighScoreData _highScoreList)
        //{
        //    #region Linq-IfElse
        //    //if (m_sortLowToHigh)
        //    //    _highScoreList.highScores = _highScoreList.highScores.OrderBy(linqSorts => linqSorts.TotalPlaytime).ToList();
        //    //else
        //    //    _highScoreList.highScores = _highScoreList.highScores.OrderByDescending(linqSorts => linqSorts.TotalPlaytime).ToList();

        //    //return _highScoreList;
        //    #endregion

        //    #region Linq-Switch
        //    switch (m_sortLowToHigh)
        //    {
        //        case true:
        //            _highScoreList.highScores = _highScoreList.highScores.OrderBy(linqSorts => linqSorts.TotalPlaytime).ToList();
        //            break;
        //        case false:
        //            _highScoreList.highScores = _highScoreList.highScores.OrderByDescending(linqSorts => linqSorts.TotalPlaytime).ToList();
        //            break;
        //    }

        //    return _highScoreList;
        //    #endregion
        //}

        //New from here downwards!!!
        private void DisplayBoard(MatchResult _matchResult)
        {
            //Create entries out of the matchResult
            var matchSettings = SettingsManager.Instance.CurrentSettings.Match;
            HighScoreEntry newEntry = new HighScoreEntry
            {
                WinningPlayerName = _matchResult.WinnerName,
                TotalPoints = _matchResult.FinalScore,
                TotalPlaytimeSeconds = _matchResult.TotalPlayTime,
                MatchWinTimestamp = DateTime.UtcNow.Ticks,
                GameMode = matchSettings.EGameMode,
                RoundsSetting = matchSettings.RoundsToWin,
                PointsSetting = matchSettings.PointsEachRound
            };

            //Add Entry to the list
            _highScoreData.highScores.Add(newEntry);
            _saveSystem.Save(_highScoreData);

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
            { Destroy(child.gameObject); }

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

        private void OnSortButtonClicked(int sortModeIndex)
        {
            var newSortMode = (EListSortMode)sortModeIndex;
            if (newSortMode == _currentSortMode)
            {
                _isSortAscending = !_isSortAscending; //Invert Sort-order
            }
            else
            {
                _currentSortMode = newSortMode;
                _isSortAscending = false; //Standard sortOption is counting from HighToLow
            }

            RefreshDisplay();
        }

        private List<HighScoreEntry> SortScores(List<HighScoreEntry> scores)
        {
            IOrderedEnumerable<HighScoreEntry> sorted;
            switch (_currentSortMode)
            {
                case EListSortMode.TotalPoints:
                    sorted = _isSortAscending ? scores.OrderBy(s => s.TotalPoints) : scores.OrderByDescending(s => s.TotalPoints);
                    break;
                case EListSortMode.MatchWinDate:
                    sorted = _isSortAscending ? scores.OrderBy(s => s.MatchWinTimestamp) : scores.OrderByDescending(s => s.MatchWinTimestamp);
                    break;
                // ... weitere Fälle für andere Sortierungen ...
                default:
                    return scores;
            }
            return sorted.ToList();
        }

        private List<HighScoreEntry> FilterScores(List<HighScoreEntry> scores)
        {
            // TODO: Hier kommt später die Filter-Logik basierend auf den Dropdowns rein.
            // Beispiel: return scores.Where(s => s.GameMode == EGameMode.Infinite).ToList();
            return scores; // Vorerst keine Filterung
        }

        #region Unity-Button-Methods
        #region Currently disabled in the current save structure
        //public void SortByRounds()
        //{
        //    m_listSortMode = EListSortMode.SetRounds;
        //    LoadHighscoresByDropdowns();
        //}

        //public void SortByMaxPoints()
        //{
        //    m_listSortMode = EListSortMode.SetPoints;
        //    LoadHighscoresByDropdowns();
        //}
        #endregion

        //public void SortByTotalPoints()
        //{
        //    m_listSortMode = EListSortMode.TotalPoints;
        //    LoadHighscoresByDropdowns(m_roundsDropdown.value, m_maxPointsDropdown.value);
        //}

        //public void SortByMatchWinDate()
        //{
        //    m_listSortMode = EListSortMode.MatchWinDate;
        //    LoadHighscoresByDropdowns(m_roundsDropdown.value, m_maxPointsDropdown.value);
        //}
        //public void SortByTotalPlaytime()
        //{
        //    m_listSortMode = EListSortMode.TotalPlaytime;
        //    LoadHighscoresByDropdowns(m_roundsDropdown.value, m_maxPointsDropdown.value);
        //}

        //public void BackToStartMenu()
        //{
        //    SceneManager.LoadScene((int)ESceneNames.StartMenu);
        //}
        #endregion

        //private void AddNewEntryDataSlot(HighScoreData _highScoreList)
        //{
        //    int rank = +1 + m_parentChildCount++;
        //    string rankSuffix = rank switch
        //    {
        //        1 => $"{rank}st",
        //        2 => $"{rank}nd",
        //        3 => $"{rank}rd",
        //        _ => $"{rank}th",
        //    };

        //    _highScoreList.highScores.Add(new HighScoreEntry(m_matchValues.WinningPlayer, m_matchUIStates.LastRoundDdIndex, m_matchUIStates.LastMaxPointDdIndex, m_matchValues.TotalPoints, m_matchValues.MatchWinDate, m_matchValues.TotalPlaytime));

        //    HighScoreEntrySlot highScoreEntrySlot = Instantiate(m_highScoreEntryChildPrefab, m_listFrame);
        //    highScoreEntrySlot.Initialize(rankSuffix, m_matchValues.WinningPlayer, m_matchUIStates.LastRoundDdIndex, m_matchUIStates.LastMaxPointDdIndex, m_matchValues.TotalPoints, m_matchValues.MatchWinDate, m_matchValues.TotalPlaytime);

        //    SortListByTotalPoints(_highScoreList);

        //    //(/Folder/SubFolder/RoundInfinityFolder on 0/MaxPointsInfinityFolder on 0, /FileName, .format)

        //    m_persistentData.SaveData($"{m_highScoreListFolderPath}/{m_roundsDropdown.value}/{m_maxPointsDropdown.value}", m_highscoresFileName, m_fileFormat, _highScoreList, m_encryptionEnabled, true);
        //}
    }
}