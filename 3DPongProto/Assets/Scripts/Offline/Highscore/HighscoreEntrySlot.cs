using TMPro;
using UnityEngine;

namespace ThreeDeePongProto.Offline.Highscores
{
    public class HighscoreEntrySlot : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI m_rankText;
        [SerializeField] private TextMeshProUGUI m_roundsText;
        [SerializeField] private TextMeshProUGUI m_maxPointsText;
        [SerializeField] private TextMeshProUGUI m_playerNameText;
        [SerializeField] private TextMeshProUGUI m_matchDateText;
        [SerializeField] private TextMeshProUGUI m_totalMatchTimeText;

        [SerializeField] private HighscoreList m_highscoreList;

        public void Initialise(MatchValues _matchValues)
        {
            int rank = _matchValues.ListIndex;
            string rankSuffix = rank switch
            {
                1 => $"{rank}st",
                2 => $"{rank}nd",
                3 => $"{rank}rd",
                _ => $"{rank}th",
            };

            m_rankText.text = rankSuffix;
            m_roundsText.text = $"{_matchValues.SetMaxRounds}";
            m_maxPointsText.text = $"{_matchValues.SetMaxPoints}";
            m_playerNameText.text = $"{_matchValues.WinningPlayer}";
            m_matchDateText.text = $"{_matchValues.MatchWinDate}";
            m_totalMatchTimeText.text = $"{_matchValues.TotalPlaytime / 3600:N0}h {_matchValues.TotalPlaytime / 60 % 60:N0}m {_matchValues.TotalPlaytime % 60:N0}s";
        }
    }
}