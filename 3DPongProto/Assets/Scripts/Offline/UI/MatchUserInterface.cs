using System.Collections.Generic;
using ThreeDeePongProto.Managers;
using TMPro;
using UnityEngine;

namespace ThreeDeePongProto.Offline.UI
{
    public class MatchUserInterface : MonoBehaviour
    {
        #region Script-References
        [SerializeField] private MatchManager m_matchManager;
        #endregion

        #region SerializeField-Member-Variables
        [SerializeField] private List<TextMeshProUGUI> m_playerNamesTMPList;
        [SerializeField] private List<TextMeshProUGUI> m_totalPointsTMPList;
        [SerializeField] private List<GameObject> m_playerAvatarList = new List<GameObject>();

        [Header("Round-Details")]
        [SerializeField] private TextMeshProUGUI m_roundNrTMP;
        [SerializeField] private TextMeshProUGUI m_elapsedTimeTMP;
        [SerializeField] private TextMeshProUGUI m_zeroToZeroTMP;

        //[SerializeField] private bool m_timerShallCountUp;
        [SerializeField] private bool m_showMilliseconds = false;

        //[Header("Music-Details")]
        //[SerializeField] private TextMeshProUGUI m_artistNamesTMP;
        //[SerializeField] private TextMeshProUGUI m_songTitleTMP;
        //[SerializeField] private GameObject m_songIcon;

        #region Scriptable Variables
        [SerializeField] private MatchValues m_matchVariables;
        #endregion
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

        private void Update()
        {
            if (m_matchManager.MatchStarted)
                DisplayTime(Time.time - m_matchManager.MatchStartTime);
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
            m_roundNrTMP.text = $"Round {m_matchVariables.CurrentRoundNr}";
            m_zeroToZeroTMP.text = $"{m_matchVariables.CurrentPointsTPOne} : {m_matchVariables.CurrentPointsTPTwo}";
        }

        private void UpdatePlayerTMPs()
        {
            List<TextMeshProUGUI> playerTotalPointsTMP = m_playerPointsConnection[m_playerNamesTMPList];

            playerTotalPointsTMP[0].text = $"Total: {m_matchVariables.TotalPointsTPOne}";
            playerTotalPointsTMP[1].text = $"Total: {m_matchVariables.TotalPointsTPTwo}";
            playerTotalPointsTMP[2].text = $"Total: {m_matchVariables.TotalPointsTPOne}";
            playerTotalPointsTMP[3].text = $"Total: {m_matchVariables.TotalPointsTPTwo}";
        }

        /// <summary>
        /// Source: https://www.youtube.com/watch?v=HmHPJL-OcQE to display the round time counter.
        /// </summary>
        /// <param name="_timeToDisplay"></param>
        private void DisplayTime(float _timeToDisplay)
        {
            #region Optional CountDown with an adjusted Start- and RoundTime.
            //if (!m_timerShallCountUp)
            //{
            //    if (_timeToDisplay < 0)
            //        _timeToDisplay = 0;
            //    else if (!m_showMilliseconds)
            //    {
            //        _timeToDisplay += 1;
            //    }

            //    //Calculating Minutes.
            //    float minutes = Mathf.FloorToInt(_timeToDisplay / 60);
            //    //Calculating Seconds.
            //    float seconds = Mathf.FloorToInt(_timeToDisplay % 60);
            //    //Calculating Milliseconds.
            //    if (m_showMilliseconds)
            //    {
            //        float milliseconds = _timeToDisplay % 1 * 1000;
            //        m_elapsedTimeTMP.text = string.Format("{0:00}:{1:00}:{2:000}", minutes, seconds, milliseconds);
            //    }
            //    else
            //    {
            //        m_elapsedTimeTMP.text = string.Format("{0:00}:{1:00}", minutes, seconds);
            //    }
            //}
            //else if (m_timerShallCountUp)
            #endregion
            //{
            //Mathf.FloorToInt(days: '_timeToDisplay / 86400', hours: '_timeToDisplay / 3600' minutes: '_timeToDisplay / 60', seconds: '_timeToDisplay % 60'.
            //Calculating Hours.
            float hours = Mathf.FloorToInt(_timeToDisplay / 3600);
            //Calculating Minutes.
            float minutes = Mathf.FloorToInt(_timeToDisplay / 60 % 60);
            //Calculating Seconds.
            float seconds = Mathf.FloorToInt(_timeToDisplay % 60);
            //Calculating Milliseconds.
            if (m_showMilliseconds)
            {
                float milliseconds = _timeToDisplay % 1 * 1000;
                //m_elapsedTimeTMP.text = string.Format("{0:00}:{1:00}:{2:000}", minutes, seconds, milliseconds);
                m_elapsedTimeTMP.text = string.Format("{0:00}:{1:00}:{2:00}:{3:000}", hours, minutes, seconds, milliseconds);
            }
            else
            {
                //m_elapsedTimeTMP.text = string.Format("{0:00}:{1:00}", minutes, seconds);
                m_elapsedTimeTMP.text = string.Format("{0:00}:{1:00}:{2:00}", hours, minutes, seconds);
            }
            //}
        }
    }
}