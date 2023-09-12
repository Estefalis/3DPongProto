using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ThreeDeePongProto.Offline.Highscores
{
    public class HighscoreList : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI m_rankText;
        [SerializeField] private TextMeshProUGUI m_roundsText;
        [SerializeField] private TextMeshProUGUI m_maxPointsText;
        [SerializeField] private TextMeshProUGUI m_playerNameText;
        [SerializeField] private TextMeshProUGUI m_matchDateText;
        [SerializeField] private TextMeshProUGUI m_totalMatchTimeText;

        [SerializeField] private MatchValues m_matchValues;

        //private IPersistentData m_persistentData = new SerializingData();
        //[SerializeField] private bool m_encryptionEnabled = false;

#if UNITY_EDITOR
        private void Update()
        {
            if (Keyboard.current.tKey.wasPressedThisFrame)
                Time.timeScale = 60.0f;

            if (Keyboard.current.fKey.wasPressedThisFrame)
            {
                m_rankText.text = "1st";
                m_roundsText.text = $"{m_matchValues.SetMaxRounds}";
                m_maxPointsText.text = $"{m_matchValues.SetMaxPoints}";
                m_playerNameText.text = $"{m_matchValues.WinningPlayer}";
                m_matchDateText.text = $"{DateTime.Today.ToShortDateString()}\n" + string.Format("{0:00}:{1:00}:{2:00}", DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
                m_totalMatchTimeText.text = $"{m_matchValues.TotalPlaytime / 3600:N0}h {m_matchValues.TotalPlaytime / 60 % 60:N0}m {m_matchValues.TotalPlaytime % 60:N0}s";
            }
        }
#endif
        public void SortListBySetRounds()
        {
            Debug.Log("Kommt noch!");
        }

        public void SortListBySetMaxPoints()
        {
            Debug.Log("Kommt noch!");
        }

        public void SearchForPlayer()
        {
            Debug.Log("Kommt noch!");
        }

        public void SortListByDate()
        {
            Debug.Log("Kommt noch!");
        }

        public void SortByTotalTime()
        {
            Debug.Log("Kommt noch!");
        }
    }
}
//m_totalMatchTimeText.text = string.Format("{0:00}:{1:00}:{2:00}:{3:000}", hours, minutes, seconds, milliseconds);
//m_matchDateText.text = $"{DateTime.Now.Day}d {DateTime.Now.Hour}h {DateTime.Now.Minute}m {DateTime.Now.Second}s";
//m_matchDateText.text = $"{DateTime.Today.TimeOfDay}";