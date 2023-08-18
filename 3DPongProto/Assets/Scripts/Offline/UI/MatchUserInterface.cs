using System.Collections.Generic;
using ThreeDeePongProto.Managers;
using TMPro;
using UnityEngine;

namespace ThreeDeePongProto.Offline.UI
{
    public class MatchUserInterface : MonoBehaviour
    {
        #region SerializeField-Member-Variables
        [SerializeField] private List<TextMeshProUGUI> m_playerNamesTMPList;
        [SerializeField] private List<TextMeshProUGUI> m_totalPointsTMPList;
        [SerializeField] private List<GameObject> m_playerAvatarList = new List<GameObject>();

        [Header("Round-Details")]
        [SerializeField] private TextMeshProUGUI m_RoundNrTMP;
        [SerializeField] private TextMeshProUGUI m_ElapsedTimeTMP;
        [SerializeField] private TextMeshProUGUI m_ZeroToZeroTMP;

        //[Header("Music-Details")]
        //[SerializeField] private TextMeshProUGUI m_artistNamesTMP;
        //[SerializeField] private TextMeshProUGUI m_songTitleTMP;
        //[SerializeField] private GameObject m_songIcon;

        [SerializeField] private MatchVariables m_matchVariables;
        #endregion

        private Dictionary<List<TextMeshProUGUI>, List<TextMeshProUGUI>> m_playerPointsConnection = new Dictionary<List<TextMeshProUGUI>, List<TextMeshProUGUI>>();

        private void OnEnable()
        {
            m_playerPointsConnection.Add(m_playerNamesTMPList, m_totalPointsTMPList);
            
            BallMovement.m_HitGoalOne += UpdateUserInterface;
            BallMovement.m_HitGoalTwo += UpdateUserInterface;
            MatchManager.m_StartNextRound += UpdateUserInterface;

            if (m_matchVariables == null)
                return;

            UpdateRoundTMPs();
            UpdatePlayerTMPs();
        }

        private void OnDisable()
        {
            BallMovement.m_HitGoalOne -= UpdateUserInterface;
            BallMovement.m_HitGoalTwo -= UpdateUserInterface;
            MatchManager.m_StartNextRound -= UpdateUserInterface;
        }

        private void UpdateUserInterface()
        {
            if (m_matchVariables == null)
            {
#if UNITY_EDITOR
                Debug.Log("Forgot to add the Scriptable Object in the Editor!");
#endif
                return;
            }

            UpdateRoundTMPs();
            UpdatePlayerTMPs();
        }

        private void UpdateRoundTMPs()
        {
            m_RoundNrTMP.text = $"Round {m_matchVariables.CurrentRoundNr}";
            m_ZeroToZeroTMP.text = $"{m_matchVariables.CurrentPointsTPOne} : {m_matchVariables.CurrentPointsTPTwo}";
        }

        private void UpdatePlayerTMPs()
        {
            List<TextMeshProUGUI> playerTotalPointsTMP = m_playerPointsConnection[m_playerNamesTMPList];

            playerTotalPointsTMP[0].text = $"Total: {m_matchVariables.TotalPointsTPOne}";
            playerTotalPointsTMP[1].text = $"Total: {m_matchVariables.TotalPointsTPTwo}";
            playerTotalPointsTMP[2].text = $"Total: {m_matchVariables.TotalPointsTPOne}";
            playerTotalPointsTMP[3].text = $"Total: {m_matchVariables.TotalPointsTPTwo}";
        }
    }
}