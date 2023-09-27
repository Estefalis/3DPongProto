using TMPro;
using UnityEngine;

namespace ThreeDeePongProto.Offline.Highscores
{
    public class HighscoreEntryPrefab : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI m_rankText;
        [SerializeField] private TextMeshProUGUI m_roundsText;
        [SerializeField] private TextMeshProUGUI m_maxPointsText;
        [SerializeField] private TextMeshProUGUI m_totalPointsText;
        [SerializeField] private TextMeshProUGUI m_playerNameText;
        [SerializeField] private TextMeshProUGUI m_matchWinDateText;
        [SerializeField] private TextMeshProUGUI m_totalPlaytimeText;

        public void Initialize(string _rank, int _rounds, int _maxPoints, double _totalPoints, string _winningPlayer, string _winDate, float _totalPlaytime)
        {
            m_rankText.text = _rank;
            m_roundsText.text = $"{_rounds}";
            m_maxPointsText.text = $"{_maxPoints}";
            m_totalPointsText.text = $"{_totalPoints}";
            m_playerNameText.text = _winningPlayer;
            m_matchWinDateText.text = _winDate;
            m_totalPlaytimeText.text = $"{_totalPlaytime / 3600:N0}h {_totalPlaytime / 60 % 60:N0}m {_totalPlaytime % 60:N0}s";
        }
    }
}