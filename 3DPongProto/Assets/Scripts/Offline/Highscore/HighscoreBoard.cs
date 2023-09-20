using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace ThreeDeePongProto.Offline.Highscores
{
    public class HighscoreBoard : MonoBehaviour
    {
        private enum EListSortMode
        {
            Rounds,
            InfiniteRounds,
            MaxPoints,
            InfinitePoints,
            PlayerName, //Sort- & Search-Option by PlayerName.
            WinDate,
            TotalPlaytime
        }

        [SerializeField] private Button[] m_topListButtons;
        [SerializeField] private TMP_Dropdown m_roundsDropdown;
        [SerializeField] private TMP_Dropdown m_maxPointsDropdown;
        [Space]
        [SerializeField] private Transform m_contentParentTransform;
        [SerializeField] private HighscoreEntryPrefab m_highscoreEntryChildPrefab;
        [SerializeField] private int m_maxHighscoreSlots;

        [SerializeField] private HighscoreSlotData m_highscoreSlotData;
        [SerializeField] private MatchUIStates m_matchUIStates;
        [SerializeField] private MatchValues m_matchValues;

        private List<string> m_roundsDdList;
        private List<string> m_maxPointsDdList;
        private List<string>[] m_highscoreLists;
        private int m_firstRoundOffset = 1, m_firstPointOffset = 1;

        private EListSortMode m_listSortMode;
        private int m_parentChildCount = 0;
        //private float m_slotHeight;

        private IPersistentData m_persistentData = new SerializingData();
        [SerializeField] private bool m_encryptionEnabled = false;

        private void Awake()
        {
            SetDropdowns();

            //m_slotHeight = m_highscoreEntryChildPrefab.GetComponent<RectTransform>().rect.height;
            m_parentChildCount = m_contentParentTransform.GetComponent<Transform>().childCount;
        }

        private void Update()
        {
            if (Keyboard.current.numpadPlusKey.wasPressedThisFrame)
            {
                AddMatchDetailSlot();
            }
        }

        private void OnDisable()
        {
            //(/Folder/Folder, /FileName, .format)
            m_persistentData.SaveData("/SaveData/Highscore List", "/List Entries", ".json", m_highscoreSlotData, m_encryptionEnabled, true);
        }

        private void SetDropdowns()
        {
            m_roundsDropdown.ClearOptions();
            m_maxPointsDropdown.ClearOptions();

            m_roundsDdList = new List<string>();
            m_maxPointsDdList = new List<string>();

            m_roundsDdList.Add("?");
            for (int i = m_firstRoundOffset; i < m_matchValues.SetMaxRounds + 1; i++)
            {
                m_roundsDdList.Add(i.ToString());
            }
            m_roundsDdList.Add("\u221E");
            //m_roundsDropdown.value = m_matchUIStates.LastRoundDdIndex;
            m_roundsDropdown.AddOptions(m_roundsDdList);

            m_maxPointsDdList.Add("?");
            for (int i = m_firstPointOffset; i < m_matchValues.SetMaxPoints + 1; i++)
            {
                //'m_maxPointsDropdown.options.Add (new Dropdown.OptionData() { text = variable });' in foreach-loops.
                m_maxPointsDdList.Add(i.ToString());
            }
            m_maxPointsDdList.Add("\u221E");

            m_maxPointsDropdown.AddOptions(m_maxPointsDdList);
            m_roundsDropdown.RefreshShownValue();

            //m_maxPointsDropdown.value = m_matchUIStates.LastMaxPointDdIndex;
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

            highscoreEntrySlot.Initialization(rankSuffix, m_highscoreSlotData.SetMaxRounds, m_highscoreSlotData.SetMaxPoints, m_highscoreSlotData.WinningPlayer, m_highscoreSlotData.MatchWinDate, m_highscoreSlotData.TotalPlaytime);
            //}
        }

        public void SortListBySetRounds(TMP_Dropdown _roundsDropdown)
        {
            m_listSortMode = EListSortMode.Rounds;
            Debug.Log(_roundsDropdown.value);
        }

        public void ShowInfiniteRoundEntries()
        {
            m_listSortMode = EListSortMode.InfiniteRounds;
            Debug.Log(m_listSortMode);
        }

        public void SortListBySetMaxPoints(TMP_Dropdown _maxPointsDropdown)
        {
            m_listSortMode = EListSortMode.MaxPoints;
            Debug.Log(_maxPointsDropdown.value);
        }

        public void ShowInfinitePointsEntries()
        {
            m_listSortMode = EListSortMode.InfinitePoints;
            Debug.Log(m_listSortMode);
        }

        public void SortListByPlayer()
        {
            m_listSortMode = EListSortMode.PlayerName;
            Debug.Log(m_listSortMode);
        }

        public void SortListByDate()
        {
            m_listSortMode = EListSortMode.WinDate;
            Debug.Log(m_listSortMode);
        }

        public void SortListByTotalTime()
        {
            m_listSortMode = EListSortMode.TotalPlaytime;
            Debug.Log(m_listSortMode);
        }
    }
}