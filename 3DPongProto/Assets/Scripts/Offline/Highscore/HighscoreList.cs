using TMPro;
using UnityEngine;

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

        //private IPersistentData m_persistentData = new SerializingData();
        //[SerializeField] private bool m_encryptionEnabled = false;
    }
}