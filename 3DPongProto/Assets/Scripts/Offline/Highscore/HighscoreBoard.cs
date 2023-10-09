using System.Collections.Generic;
using System.Linq;
using ThreeDeePongProto.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ThreeDeePongProto.Offline.Highscores
{
    public class HighscoreBoard : MonoBehaviour
    {
        private enum EListSortMode
        {
            None,
            Rounds,
            MaxPoints,
            TotalPoints,
            PlayerNames, //Sort- & Search-Option by PlayerNames.
            MatchWinDate,
            TotalPlaytime
        }

        [SerializeField] private Transform m_disableTransform;
        [SerializeField] private EventSystem m_eventSystem;
        [SerializeField] private Button[] m_topListButtons;
        [SerializeField] private TMP_Dropdown m_roundsDropdown;
        [SerializeField] private TMP_Dropdown m_maxPointsDropdown;
        [SerializeField] private Button m_finishButton;
        [Space]
        [SerializeField] private Transform m_contentParentTransform;
        [SerializeField] private HighscoreEntryPrefab m_highscoreEntryChildPrefab;
        [SerializeField] private GameObject m_noDataPrefab;
        //[SerializeField] private int m_maxHighscoreSlots;

        [SerializeField] private MatchUIStates m_matchUIStates;
        [SerializeField] private MatchUIValues m_matchUIValues;
        [SerializeField] private MatchValues m_matchValues;

        [SerializeField] private bool m_sortLowToHigh;

        private List<string> m_roundsDdList;
        private List<string> m_maxPointsDdList;
        private int m_firstRoundOffset = 1, m_firstPointOffset = 1;

        private HighscoreEntryData m_highscoreSlotData;
        private EListSortMode m_listSortMode;
        private int m_parentChildCount;
        //private float m_slotHeight;

        private IPersistentData m_persistentData = new SerializingData();
        [SerializeField] private bool m_encryptionEnabled = false;

        private void Awake()
        {
            SetDropdowns();
            LoadHighscoresFromFiles();

            //TODO: Action to join load these inside the game, so the highscore list can be used outside of matches.
            //m_slotHeight = m_highscoreEntryChildPrefab.GetComponent<RectTransform>().rect.height;
            m_parentChildCount = m_contentParentTransform.GetComponent<Transform>().childCount;

            MatchManager.m_StartWinProcedure += DisplayHighscoreBoard;
            m_eventSystem.SetSelectedGameObject(m_finishButton.gameObject);
            if (m_disableTransform.gameObject.activeInHierarchy)
                m_disableTransform.gameObject.SetActive(false);
        }

        private void OnDisable()
        {
            gameObject.SetActive(false);

            MatchManager.m_StartWinProcedure -= DisplayHighscoreBoard;
        }

        private void Update()
        {
            if (Keyboard.current.numpadPlusKey.wasPressedThisFrame)
            {
                CopyScriptableDetails();
                AddMatchDetailSlot();
            }
        }

        private void SetDropdowns()
        {
            m_roundsDdList = new List<string>();
            m_maxPointsDdList = new List<string>();

            m_roundsDropdown.ClearOptions();
            //m_roundsDdList.Add("?");
            m_roundsDdList.Add("\u221E");
            for (int i = m_firstRoundOffset; i < m_matchUIValues.SetMaxRounds + 1; i++)
            {
                m_roundsDdList.Add(i.ToString());
            }
            m_roundsDropdown.AddOptions(m_roundsDdList);
            //TODO: Option to set this dropdown value, when people want to check Highscores from the mainmenu.
            m_roundsDropdown.value = m_roundsDdList.Count;
            m_roundsDropdown.RefreshShownValue();

            m_maxPointsDropdown.ClearOptions();
            //m_maxPointsDdList.Add("?");
            m_maxPointsDdList.Add("\u221E");
            for (int i = m_firstPointOffset; i < m_matchUIValues.SetMaxPoints + 1; i++)
            {
                //'m_maxPointsDropdown.options.Add (new Dropdown.OptionData() { text = variable });' in foreach-loops.
                m_maxPointsDdList.Add(i.ToString());
            }
            m_maxPointsDropdown.AddOptions(m_maxPointsDdList);
            //TODO: Option to set this dropdown value, when people want to check Highscores from the mainmenu.
            m_maxPointsDropdown.value = m_maxPointsDdList.Count;
            m_roundsDropdown.RefreshShownValue();
        }

        private void LoadHighscoresFromFiles()
        {
            foreach (Transform child in m_contentParentTransform)
                Destroy(child.gameObject);

            //TODO: Aufgerufene Liste bestimmen. Eventuell Suchfunktion als Wahl vorschalten, wenn der Aufruf ausserhalb des Spiels erfolgen soll.
            HighscoreList highscoreList = m_persistentData.LoadData<HighscoreList>($"/SaveData/Highscore Lists/{m_roundsDropdown.value}/{m_maxPointsDropdown.value}", "/Highscores", ".json", m_encryptionEnabled);

            if (highscoreList == null)
            {
                _ = Instantiate(m_noDataPrefab, m_contentParentTransform);  //_ replaces GameObject noDataNotification, if the GameObject isn't used.
                //noDataNotification.gameObject.SetActive(true);
                return;
            }

            SortListByEnum(highscoreList);

            m_parentChildCount = 0;
            foreach (HighscoreEntryData highscores in highscoreList.highscores)
            {
                HighscoreEntryPrefab highscoreEntrySlot = Instantiate(m_highscoreEntryChildPrefab, m_contentParentTransform);

                int rank = +1 + m_parentChildCount++;
                string rankSuffix = rank switch
                {
                    1 => $"{rank}st",
                    2 => $"{rank}nd",
                    3 => $"{rank}rd",
                    _ => $"{rank}th",
                };

                highscoreEntrySlot.Initialize(rankSuffix, highscores.SetMaxRounds, highscores.SetMaxPoints, highscores.TotalPoints, highscores.WinningPlayer, highscores.MatchWinDate, highscores.TotalPlaytime);
            }
        }

        private HighscoreList SortListByEnum(HighscoreList _highscoreList)
        {
            switch (m_listSortMode)
            {
                case EListSortMode.None:
                    break;
                //In this setup Rounds & MaxPoints are set and used to navigate to the correct ListFolder, by using the corresponding Dropdown.Value. (LoadData Path)
                //case EListSortMode.Rounds:
                //    break;
                //case EListSortMode.MaxPoints:
                //    break;
                case EListSortMode.TotalPoints:
                {
                    m_sortLowToHigh = !m_sortLowToHigh;
                    SortListByTotalPoints(_highscoreList);
                    break;
                }
                case EListSortMode.PlayerNames:
                { break; }
                case EListSortMode.MatchWinDate:
                {
                    //The WinDate SortBehaviour was partly strange. Sending the parameter here, while don't elsewhere, currently avoid bool setting errors. 
                    m_sortLowToHigh = !m_sortLowToHigh;
                    SortListByMatchWinDate(_highscoreList, m_sortLowToHigh);
                    break;
                }
                case EListSortMode.TotalPlaytime:
                {
                    m_sortLowToHigh = !m_sortLowToHigh;
                    SortListByTotalPlaytime(_highscoreList);
                    break;
                }
            }

            return _highscoreList;
        }

        private HighscoreList SortListByTotalPoints(HighscoreList _highscoreList)
        {
            #region Linq-IfElse
            //if (m_sortLowToHigh)
            //    _highscoreList.highscores = _highscoreList.highscores.OrderBy(linqSorts => linqSorts.TotalPoints).ToList();
            //else
            //    _highscoreList.highscores = _highscoreList.highscores.OrderByDescending(linqSorts => linqSorts.TotalPoints).ToList();

            //return _highscoreList;
            #endregion

            #region Linq-Switch
            switch (m_sortLowToHigh)
            {
                case true:
                    _highscoreList.highscores = _highscoreList.highscores.OrderBy(linqSorts => linqSorts.TotalPoints).ToList();
                    break;
                case false:
                    _highscoreList.highscores = _highscoreList.highscores.OrderByDescending(linqSorts => linqSorts.TotalPoints).ToList();
                    break;
            }

            return _highscoreList;
            #endregion
        }

        private HighscoreList SortListByMatchWinDate(HighscoreList _highscoreList, bool _sortLowToHigh)
        {
            #region Linq-IfElse
            //if (m_sortLowToHigh)
            //    _highscoreList.highscores = _highscoreList.highscores.OrderBy(linqSorts => linqSorts.MatchWinDate).ToList();
            //else
            //    _highscoreList.highscores = _highscoreList.highscores.OrderByDescending(linqSorts => linqSorts.MatchWinDate).ToList();

            //return _highscoreList;
            #endregion

            #region Linq-Switch
            switch (_sortLowToHigh)
            {
                case true:
                    _highscoreList.highscores = _highscoreList.highscores.OrderBy(linqSorts => linqSorts.MatchWinDate).ToList();
                    break;
                case false:
                    _highscoreList.highscores = _highscoreList.highscores.OrderByDescending(linqSorts => linqSorts.MatchWinDate).ToList();
                    break;
            }

            return _highscoreList;
            #endregion
        }

        private HighscoreList SortListByTotalPlaytime(HighscoreList _highscoreList)
        {
            #region Linq-IfElse
            //if (m_sortLowToHigh)
            //    _highscoreList.highscores = _highscoreList.highscores.OrderBy(linqSorts => linqSorts.TotalPlaytime).ToList();
            //else
            //    _highscoreList.highscores = _highscoreList.highscores.OrderByDescending(linqSorts => linqSorts.TotalPlaytime).ToList();

            //return _highscoreList;
            #endregion

            #region Linq-Switch
            switch (m_sortLowToHigh)
            {
                case true:
                    _highscoreList.highscores = _highscoreList.highscores.OrderBy(linqSorts => linqSorts.TotalPlaytime).ToList();
                    break;
                case false:
                    _highscoreList.highscores = _highscoreList.highscores.OrderByDescending(linqSorts => linqSorts.TotalPlaytime).ToList();
                    break;
            }

            return _highscoreList;
            #endregion
        }

        private void DisplayHighscoreBoard()
        {
            m_disableTransform.gameObject.SetActive(true);
        }

        #region Unity-Button-Methods
        public void SortByRounds()
        {
            m_listSortMode = EListSortMode.Rounds;
            LoadHighscoresFromFiles();
        }

        public void SortByMaxPoints()
        {
            m_listSortMode = EListSortMode.MaxPoints;
            LoadHighscoresFromFiles();
        }

        public void SortByTotalPoints()
        {
            m_listSortMode = EListSortMode.TotalPoints;
            LoadHighscoresFromFiles();
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
            LoadHighscoresFromFiles();
        }
        public void SortByTotalPlaytime()
        {
            m_listSortMode = EListSortMode.TotalPlaytime;
            LoadHighscoresFromFiles();
        }

        public void BackToStartMenu()
        {
            SceneManager.LoadScene(0);
        }
        #endregion

        #region Delete after Development
        //TODO: Remove TestSave on temporary RankButton and implement Saving on Disable.
        public void TestSave()
        {
            HighscoreList highscoreListData = new();

            foreach (Transform child in m_contentParentTransform)
                highscoreListData.highscores.Add(m_highscoreSlotData);

            //(/Folder/SubFolder/RoundInfinityFolder on 0/MaxPointsInfinityFolder on 0, /FileName, .format)
            m_persistentData.SaveData($"/SaveData/Highscore Lists/{m_matchUIValues.SetMaxRounds}/{m_matchUIValues.SetMaxPoints}", "/Highscores", ".json", highscoreListData, m_encryptionEnabled, true);
        }

        private void CopyScriptableDetails()
        {
            m_highscoreSlotData.SetMaxRounds = m_matchUIValues.SetMaxRounds;
            m_highscoreSlotData.SetMaxPoints = m_matchUIValues.SetMaxPoints;
            m_highscoreSlotData.TotalPoints = m_matchValues.TotalPoints;
            m_highscoreSlotData.WinningPlayer = m_matchValues.WinningPlayer;
            m_highscoreSlotData.MatchWinDate = m_matchValues.MatchWinDate;
            m_highscoreSlotData.TotalPlaytime = m_matchValues.TotalPlaytime;
        }

        private void AddMatchDetailSlot()
        {
            //for (int i = 0; i < m_maxHighscoreSlots; i++)
            //{
            m_parentChildCount = m_contentParentTransform.GetComponent<Transform>().childCount;

            HighscoreEntryPrefab highscoreEntrySlot = Instantiate(m_highscoreEntryChildPrefab, m_contentParentTransform);

            int rank = /*i*/ +1 + m_parentChildCount;
            string rankSuffix = rank switch
            {
                1 => $"{rank}st",
                2 => $"{rank}nd",
                3 => $"{rank}rd",
                _ => $"{rank}th",
            };

            highscoreEntrySlot.Initialize(rankSuffix, m_matchUIValues.SetMaxRounds, m_matchUIValues.SetMaxPoints, m_matchValues.TotalPoints, m_matchValues.WinningPlayer, m_matchValues.MatchWinDate, m_matchValues.TotalPlaytime);
            //}
        }
        #endregion
    }
}