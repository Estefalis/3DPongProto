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

        public void Initialize(int _rank, HighScoreEntry entry /*, string _winningPlayer, int _rounds, int _maxPoints, double _totalPoints, string _winDate, float _totalPlaytime*/)
        {
            //Rank (1st, 2nd, etc.)
            string rankSuffix = (_rank % 10) switch { 1 => "st", 2 => "nd", 3 => "rd", _ => "th" };
            if (_rank == 11 || _rank == 12 || _rank == 13)
                rankSuffix = "th";
            m_rankText.text = $"{_rank}{rankSuffix}";

            m_playerNameText.text = entry.WinningPlayerName;
            m_totalPointsText.text = entry.TotalPoints.ToString();

            // Daten für die Anzeige formatieren
            DateTime winDate = new DateTime(entry.MatchWinTimestamp);
            m_matchWinDateText.text = winDate.ToString("yyyy-MM-dd HH:mm");

            TimeSpan timeSpan = TimeSpan.FromSeconds(entry.TotalPlaytimeSeconds);
            m_totalPlaytimeText.text = timeSpan.ToString(@"hh\:mm\:ss");

            //m_rankText.text = _rank;
            //m_playerNameText.text = _winningPlayer;
            //m_roundsText.text = $"{_rounds}";
            //m_maxPointsText.text = $"{_maxPoints}";
            //m_totalPointsText.text = $"{_totalPoints}";
            //m_matchWinDateText.text = _winDate;
            //m_totalPlaytimeText.text = $"{_totalPlaytime / 3600:N0}h {_totalPlaytime / 60 % 60:N0}m {_totalPlaytime % 60:N0}s";
        }
    }
}