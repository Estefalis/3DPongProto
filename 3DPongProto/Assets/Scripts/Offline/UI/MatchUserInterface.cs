using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace ThreeDeePongProto.Offline.UI
{
    public class MatchUserInterface : MonoBehaviour
    {
        [SerializeField] private List<TextMeshProUGUI> m_playerNamesTMPList;
        [SerializeField] private List<TextMeshProUGUI> m_totalPointsTMPList;
        [SerializeField] private List<GameObject> m_playerAvatarList = new List<GameObject>();

        private Dictionary<List<TextMeshProUGUI>, List<TextMeshProUGUI>> m_playerPointsConnection = new Dictionary<List<TextMeshProUGUI>, List<TextMeshProUGUI>>();

        [Header("Round-Details")]
        [SerializeField] private TextMeshProUGUI m_RoundNrTMP;
        [SerializeField] private TextMeshProUGUI m_ElapsedTimeTMP;
        [SerializeField] private TextMeshProUGUI m_ZeroToZeroTMP;

        //[Header("Music-Details")]
        //[SerializeField] private TextMeshProUGUI m_artistNamesTMP;
        //[SerializeField] private TextMeshProUGUI m_songTitleTMP;
        //[SerializeField] private GameObject m_songIcon;

        [SerializeField] private MatchVariables m_matchVariables;
        [SerializeField] private PlayerData[] m_playerData;

        private void Awake()
        {
            m_playerPointsConnection.Add(m_playerNamesTMPList, m_totalPointsTMPList);
        }

        private void OnEnable()
        {
            BallMovement.m_HitGoalOne += UpdateMatchPoints;
            BallMovement.m_HitGoalTwo += UpdateMatchPoints;
        }

        private void OnDisable()
        {
            BallMovement.m_HitGoalOne -= UpdateMatchPoints;
            BallMovement.m_HitGoalTwo -= UpdateMatchPoints;
        }

        private void UpdateMatchPoints(uint _index)
        {
            if (m_matchVariables == null)
            {
#if UNITY_EDITOR
                Debug.Log("Forgot to add the Scriptable Object in the Editor!");
#endif
                return;
            }

            switch (_index)
            {
                case 1:
                {
                    ++m_matchVariables.CurrentPointsTeamOne;
                    ++m_matchVariables.TotalPointsTeamOne;
                    break;
                }
                case 2:
                {
                    ++m_matchVariables.CurrentPointsTeamTwo;
                    ++m_matchVariables.TotalPointsTeamTwo;
                    break;
                }
                default:
                    break;
            }

            m_ZeroToZeroTMP.text = $"{m_matchVariables.CurrentPointsTeamOne} : {m_matchVariables.CurrentPointsTeamTwo}";

            List<TextMeshProUGUI> playerTotalPointsTMP = m_playerPointsConnection[m_playerNamesTMPList];
            playerTotalPointsTMP[0].text = $"{m_matchVariables.TotalPointsTeamOne}";
            playerTotalPointsTMP[1].text = $"{m_matchVariables.TotalPointsTeamTwo}";
            playerTotalPointsTMP[2].text = $"{m_matchVariables.TotalPointsTeamOne}";
            playerTotalPointsTMP[3].text = $"{m_matchVariables.TotalPointsTeamTwo}";

            if (m_playerData != null)
            {
                //Player 1
                m_playerData[0].TotalPoints = m_matchVariables.TotalPointsTeamOne;
                //Player 2
                m_playerData[1].TotalPoints = m_matchVariables.TotalPointsTeamTwo;
                //Player 3
                m_playerData[2].TotalPoints = m_matchVariables.TotalPointsTeamOne;
                //Player 4
                m_playerData[3].TotalPoints = m_matchVariables.TotalPointsTeamTwo;
            }
        }
    }
}