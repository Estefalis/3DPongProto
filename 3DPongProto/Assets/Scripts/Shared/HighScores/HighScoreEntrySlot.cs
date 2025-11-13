using System;
using TMPro;
using UnityEngine;

namespace ThreeDeePongProto.Shared.Highscores
{
    public class HighScoreEntrySlot : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI m_rankText;
        [SerializeField] private TextMeshProUGUI m_playerNameText;
        [SerializeField] private TextMeshProUGUI m_roundsText;
        [SerializeField] private TextMeshProUGUI m_maxPointsText;
        [SerializeField] private TextMeshProUGUI m_totalPointsText;
        [SerializeField] private TextMeshProUGUI m_totalPlaytimeText;
        [SerializeField] private TextMeshProUGUI m_matchWinDateText;

        public void Initialize(int _rank, HighScoreEntry entry)
        {
            //Rank (1st, 2nd, etc.)
            string rankSuffix = (_rank % 10) switch { 1 => "st", 2 => "nd", 3 => "rd", _ => "th" };
            if (_rank == 11 || _rank == 12 || _rank == 13)
                rankSuffix = "th";

            m_rankText.text = $"{_rank}{rankSuffix}";
            m_playerNameText.text = entry.WinningPlayerName;
            m_roundsText.text = $"{entry.RoundSetting}";
            m_maxPointsText.text = $"{entry.PointSetting}";
            m_totalPointsText.text = entry.TotalPoints.ToString();

            // Daten für die Anzeige formatieren
            TimeSpan timeSpan = TimeSpan.FromSeconds(entry.TotalPlaytime);
            m_totalPlaytimeText.text = timeSpan.ToString(@"hh\:mm\:ss");

            DateTime winDate = new DateTime(entry.MatchWinTimestamp);
            m_matchWinDateText.text = winDate.ToString("yyyy-MM-dd HH:mm");
        }
    }
}