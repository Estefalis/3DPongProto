using System.Collections.Generic;
using System.Linq;
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
            None = default,
            TotalPoints,
            PlayerNames, //Sort- & Search-Option by PlayerNames?
            MatchWinDate,
            TotalPlaytime
        }

        [SerializeField] private Transform m_disableTransform;
        [SerializeField] private EventSystem m_eventSystem;
        [SerializeField] private TMP_Dropdown m_roundsDropdown;
        [SerializeField] private TMP_Dropdown m_maxPointsDropdown;
        [SerializeField] private Button m_finishButton;
        [Space]
        [SerializeField] private Transform m_contentParentTransform;
        [SerializeField] private HighscoreEntryPrefab m_highScoreEntryChildPrefab;
        [SerializeField] private GameObject m_noDataPrefab;

        [SerializeField] private MatchUIStates m_matchUIStates;
        [SerializeField] private MatchValues m_matchValues;

        [SerializeField] private bool m_sortLowToHigh;

        [SerializeField] private EListSortMode m_listSortMode;

        private List<string> m_roundsDdList;
        private List<string> m_maxPointsDdList;
        private readonly int m_firstRoundOffset = 1;
        private readonly int m_firstPointOffset = 1;
        private int m_parentChildCount;

        #region Serialization
        //private readonly string m_highScoreListFolderPath = "/SaveData/HighScore Lists";
        //private readonly string m_highscoresFileName = "/Highscores";
        //private readonly string m_fileFormat = ".json";

        //private readonly IPersistentData m_persistentData = new SerializingData();
        //[SerializeField] private bool m_encryptionEnabled = false;
        #endregion

        private void Awake()
        {
            SetupDropdowns();

            //TODO: Action to join load these inside the game, so the highScore list can be used outside of matches.
            //m_slotHeight = m_highScoreEntryChildPrefab.GetComponent<RectTransform>().rect.height;
            m_parentChildCount = m_contentParentTransform.GetComponent<Transform>().childCount;

            m_eventSystem.SetSelectedGameObject(m_finishButton.gameObject);

            if (m_disableTransform.gameObject.activeInHierarchy)
                m_disableTransform.gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            LocalMatchManager.AStartWinProcedure += DisplayHighScoreBoard;
            LocalMatchManager.ALoadUpHighScores += LoadHighScoresOnGameEnd;

            //m_roundsDropdown.onValueChanged.AddListeners(OnRoundDropdownChanges);
            //m_maxPointsDropdown.onValueChanged.AddListeners(OnMaxPointDropdownChanges);
        }

        private void OnDisable()
        {
            gameObject.SetActive(false);

            LocalMatchManager.AStartWinProcedure -= DisplayHighScoreBoard;
            LocalMatchManager.ALoadUpHighScores -= LoadHighScoresOnGameEnd;
        }

        private void SetupDropdowns()
        {
            //m_roundsDdList = new List<string>();
            //m_maxPointsDdList = new List<string>();

            //m_roundsDropdown.ClearOptions();
            ////m_roundsDdList.Add("?");
            //m_roundsDdList.Add("\u221E");
            //for (int i = m_firstRoundOffset; i < m_matchUIStates.RoundsToWin + 1; i++)
            //{
            //    m_roundsDdList.Add(i.ToString());
            //}
            //m_roundsDropdown.AddOptions(m_roundsDdList);
            ////TODO: Option to set this dropdown value, when people want to check Highscores from the mainMenu.
            //m_roundsDropdown.value = m_matchUIStates.LastRoundDdIndex;
            //m_roundsDropdown.RefreshShownValue();

            //m_maxPointsDropdown.ClearOptions();
            ////m_maxPointsDdList.Add("?");
            //m_maxPointsDdList.Add("\u221E");
            //for (int i = m_firstPointOffset; i < m_matchUIStates.PointsEachRound + 1; i++)
            //{
            //    //'m_maxPointsDropdown.options.Add (new Dropdown.OptionData() { text = variable });' in foreach-loops.
            //    m_maxPointsDdList.Add(i.ToString());
            //}
            //m_maxPointsDropdown.AddOptions(m_maxPointsDdList);
            ////TODO: Option to set this dropdown value, when people want to check Highscores from the mainMenu.
            //m_maxPointsDropdown.value = m_matchUIStates.LastMaxPointDdIndex;
            //m_maxPointsDropdown.RefreshShownValue();
        }

        /// <summary>
        /// Loads the HighScoreList with the current WinValue after the game ended.
        /// </summary>
        private void LoadHighScoresOnGameEnd()
        {
            //m_disableTransform.gameObject.SetActive(true);

            //foreach (Transform child in m_contentParentTransform)
            //    Destroy(child.gameObject);

            //HighscoreList highScoreList = m_persistentData.LoadData<HighscoreList>($"{m_highScoreListFolderPath}/{m_matchUIStates.LastRoundDdIndex}/{m_matchUIStates.LastMaxPointDdIndex}", m_highscoresFileName, m_fileFormat, m_encryptionEnabled);

            //if (highScoreList == null)
            //{
            //    highScoreList = new HighscoreList();
            //    AddNewEntryDataSlot(highScoreList);
            //    return;
            //}
            //else
            //{
            //    //SortMode None on the first time. (Loaded as saved.)
            //    SortListByEnum(highScoreList);

            //    m_parentChildCount = 0;
            //    foreach (HighscoreEntryData highscores in highScoreList.highscores)
            //    {
            //        HighscoreEntryPrefab highScoreEntrySlot = Instantiate(m_highScoreEntryChildPrefab, m_contentParentTransform);

            //        int rank = +1 + m_parentChildCount++;
            //        string rankSuffix = rank switch
            //        {
            //            1 => $"{rank}st",
            //            2 => $"{rank}nd",
            //            3 => $"{rank}rd",
            //            _ => $"{rank}th",
            //        };

            //        highScoreEntrySlot.Initialize(rankSuffix, highscores.SetMaxRounds, highscores.SetMaxPoints, highscores.TotalPoints, highscores.WinningPlayer, highscores.MatchWinDate, highscores.TotalPlaytime);
            //    }

            //    AddNewEntryDataSlot(highScoreList);
            //}
        }

        private void OnRoundDropdownChanges(int _roundValue)
        {
            LoadHighscoresByDropdowns(_roundValue, m_maxPointsDropdown.value);
        }

        private void OnMaxPointDropdownChanges(int _maxPointValue)
        {
            LoadHighscoresByDropdowns(m_roundsDropdown.value, _maxPointValue);
        }

        /// <summary>
        /// Manual HighScoreLists-Switch by DropdownChanges, after the Game ended, to see other HighScoreLists.
        /// </summary>
        /// <param name="_roundValue"></param>
        /// <param name="_maxPointValue"></param>
        private void LoadHighscoresByDropdowns(int _roundValue, int _maxPointValue)
        {
            //foreach (Transform child in m_contentParentTransform)
            //    Destroy(child.gameObject);

            //HighscoreList highScoreList = m_persistentData.LoadData<HighscoreList>($"{m_highScoreListFolderPath}/{_roundValue}/{_maxPointValue}", m_highscoresFileName, m_fileFormat, m_encryptionEnabled);

            //if (highScoreList == null)
            //{
            //    _ = Instantiate(m_noDataPrefab, m_contentParentTransform);  //_ replaces GameObject noDataNotification, if the GameObject isn't used.
            //    //noDataNotification.gameObject.SetActive(true);
            //    return;
            //}

            ////SortMode None on the first time. (Loaded as saved.)
            //SortListByEnum(highScoreList);

            //m_parentChildCount = 0;
            //foreach (HighscoreEntryData highscores in highScoreList.highscores)
            //{
            //    HighscoreEntryPrefab highScoreEntrySlot = Instantiate(m_highScoreEntryChildPrefab, m_contentParentTransform);

            //    int rank = +1 + m_parentChildCount++;
            //    string rankSuffix = rank switch
            //    {
            //        1 => $"{rank}st",
            //        2 => $"{rank}nd",
            //        3 => $"{rank}rd",
            //        _ => $"{rank}th",
            //    };

            //    highScoreEntrySlot.Initialize(rankSuffix, highscores.SetMaxRounds, highscores.SetMaxPoints, highscores.TotalPoints, highscores.WinningPlayer, highscores.MatchWinDate, highscores.TotalPlaytime);
            //}
        }

        private HighscoreList SortListByEnum(HighscoreList _highScoreList)
        {
            switch (m_listSortMode)
            {
                case EListSortMode.None:
                    break;
                case EListSortMode.TotalPoints:
                {
                    m_sortLowToHigh = !m_sortLowToHigh;
                    SortListByTotalPoints(_highScoreList);
                    break;
                }
                case EListSortMode.PlayerNames:
                { break; }
                case EListSortMode.MatchWinDate:
                {
                    //The WinDate SortBehavior was partly strange. Sending the parameter here, while don't elsewhere, currently avoids bool setting errors. 
                    m_sortLowToHigh = !m_sortLowToHigh;
                    SortListByMatchWinDate(_highScoreList, m_sortLowToHigh);
                    break;
                }
                case EListSortMode.TotalPlaytime:
                {
                    m_sortLowToHigh = !m_sortLowToHigh;
                    SortListByTotalPlaytime(_highScoreList);
                    break;
                }
            }

            return _highScoreList;
        }

        private HighscoreList SortListByTotalPoints(HighscoreList _highScoreList)
        {
            #region Linq-IfElse
            //if (m_sortLowToHigh)
            //    _highScoreList.highscores = _highScoreList.highscores.OrderBy(linqSorts => linqSorts.TotalPoints).ToList();
            //else
            //    _highScoreList.highscores = _highScoreList.highscores.OrderByDescending(linqSorts => linqSorts.TotalPoints).ToList();

            //return _highScoreList;
            #endregion

            #region Linq-Switch
            switch (m_sortLowToHigh)
            {
                case true:
                    _highScoreList.highscores = _highScoreList.highscores.OrderBy(linqSorts => linqSorts.TotalPoints).ToList();
                    break;
                case false:
                    _highScoreList.highscores = _highScoreList.highscores.OrderByDescending(linqSorts => linqSorts.TotalPoints).ToList();
                    break;
            }

            return _highScoreList;
            #endregion
        }

        private HighscoreList SortListByMatchWinDate(HighscoreList _highScoreList, bool _sortLowToHigh)
        {
            #region Linq-IfElse
            //if (m_sortLowToHigh)
            //    _highScoreList.highscores = _highScoreList.highscores.OrderBy(linqSorts => linqSorts.MatchWinDate).ToList();
            //else
            //    _highScoreList.highscores = _highScoreList.highscores.OrderByDescending(linqSorts => linqSorts.MatchWinDate).ToList();

            //return _highScoreList;
            #endregion

            #region Linq-Switch
            switch (_sortLowToHigh)
            {
                case true:
                    _highScoreList.highscores = _highScoreList.highscores.OrderBy(linqSorts => linqSorts.MatchWinDate).ToList();
                    break;
                case false:
                    _highScoreList.highscores = _highScoreList.highscores.OrderByDescending(linqSorts => linqSorts.MatchWinDate).ToList();
                    break;
            }

            return _highScoreList;
            #endregion
        }

        private HighscoreList SortListByTotalPlaytime(HighscoreList _highScoreList)
        {
            #region Linq-IfElse
            //if (m_sortLowToHigh)
            //    _highScoreList.highscores = _highScoreList.highscores.OrderBy(linqSorts => linqSorts.TotalPlaytime).ToList();
            //else
            //    _highScoreList.highscores = _highScoreList.highscores.OrderByDescending(linqSorts => linqSorts.TotalPlaytime).ToList();

            //return _highScoreList;
            #endregion

            #region Linq-Switch
            switch (m_sortLowToHigh)
            {
                case true:
                    _highScoreList.highscores = _highScoreList.highscores.OrderBy(linqSorts => linqSorts.TotalPlaytime).ToList();
                    break;
                case false:
                    _highScoreList.highscores = _highScoreList.highscores.OrderByDescending(linqSorts => linqSorts.TotalPlaytime).ToList();
                    break;
            }

            return _highScoreList;
            #endregion
        }

        private void DisplayHighScoreBoard()
        {
            m_disableTransform.gameObject.SetActive(true);
        }

        #region Unity-Button-Methods
        #region Currently disabled in the current save structure
        //public void SortByRounds()
        //{
        //    m_listSortMode = EListSortMode.Rounds;
        //    LoadHighscoresByDropdowns();
        //}

        //public void SortByMaxPoints()
        //{
        //    m_listSortMode = EListSortMode.PointsEachRound;
        //    LoadHighscoresByDropdowns();
        //}
        #endregion

        public void SortByTotalPoints()
        {
            m_listSortMode = EListSortMode.TotalPoints;
            LoadHighscoresByDropdowns(m_roundsDropdown.value, m_maxPointsDropdown.value);
        }

        public void SortByPlayerNames()
        {
            m_listSortMode = EListSortMode.PlayerNames;
#if UNITY_EDITOR
            Debug.Log(m_listSortMode);
#endif
        }

        public void SortByMatchWinDate()
        {
            m_listSortMode = EListSortMode.MatchWinDate;
            LoadHighscoresByDropdowns(m_roundsDropdown.value, m_maxPointsDropdown.value);
        }
        public void SortByTotalPlaytime()
        {
            m_listSortMode = EListSortMode.TotalPlaytime;
            LoadHighscoresByDropdowns(m_roundsDropdown.value, m_maxPointsDropdown.value);
        }

        public void BackToStartMenu()
        {
            SceneManager.LoadScene((int)ESceneNames.StartMenu);
        }
        #endregion

        private void AddNewEntryDataSlot(HighscoreList _highScoreList)
        {
            int rank = +1 + m_parentChildCount++;
            string rankSuffix = rank switch
            {
                1 => $"{rank}st",
                2 => $"{rank}nd",
                3 => $"{rank}rd",
                _ => $"{rank}th",
            };

            _highScoreList.highscores.Add(new HighscoreEntryData(m_matchUIStates.LastRoundDdIndex, m_matchUIStates.LastMaxPointDdIndex, m_matchValues.TotalPoints, m_matchValues.WinningPlayer, m_matchValues.MatchWinDate, m_matchValues.TotalPlaytime));

            HighscoreEntryPrefab highScoreEntrySlot = Instantiate(m_highScoreEntryChildPrefab, m_contentParentTransform);
            highScoreEntrySlot.Initialize(rankSuffix, m_matchUIStates.LastRoundDdIndex, m_matchUIStates.LastMaxPointDdIndex, m_matchValues.TotalPoints, m_matchValues.WinningPlayer, m_matchValues.MatchWinDate, m_matchValues.TotalPlaytime);

            SortListByTotalPoints(_highScoreList);

            //(/Folder/SubFolder/RoundInfinityFolder on 0/MaxPointsInfinityFolder on 0, /FileName, .format)
            
            //m_persistentData.SaveData($"{m_highScoreListFolderPath}/{m_roundsDropdown.value}/{m_maxPointsDropdown.value}", m_highscoresFileName, m_fileFormat, _highScoreList, m_encryptionEnabled, true);
        }
    }
}