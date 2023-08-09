using System;
using ThreeDeePongProto.Offline.UI;
using UnityEngine;

namespace ThreeDeePongProto.Managers
{
    public class MatchManager : MonoBehaviour
    {
        //TODO: MatchUI RoundTextfield
        [SerializeField] private GameObject m_ground;
        [SerializeField] private float m_groundWidthScale, m_groundLengthScale;
        [SerializeField] private MatchVariables m_matchVariables;
        [SerializeField] private PlayerData[] m_playerData;

        //[SerializeField] private int m_winPointDifference;

        public static event Action m_StartNextRound;
        public static event Action m_StartWinProcedure;

        private void Awake()
        {
            m_matchVariables.CurrentRoundNr = 1;
            ReSetMatch();
        }

        private void OnEnable()
        {
            MenuOrganisation.RestartGameLevel += ReSetMatch;
            BallMovement.m_HitGoalOne += UpdateTPOnePoints;
            BallMovement.m_HitGoalTwo += UpdateTPTwoPoints;
            m_StartNextRound += StartNextRound;
            m_StartWinProcedure += StartWinProcedure;
        }

        private void OnDisable()
        {
            MenuOrganisation.RestartGameLevel -= ReSetMatch;
            BallMovement.m_HitGoalOne -= UpdateTPOnePoints;
            BallMovement.m_HitGoalTwo -= UpdateTPTwoPoints;
            m_StartNextRound -= StartNextRound;
            m_StartWinProcedure -= StartWinProcedure;
        }

        private void ReSetMatch()
        {
            if (m_matchVariables == null)
            {
#if UNITY_EDITOR
                Debug.Log("MatchManager: Forgot to add a Scriptable Object in the Editor!");
#endif
                return;
            }

            ResetPlayfield();
            ResetRoundValues();
        }

        private void UpdateTPOnePoints()
        {
            if (m_playerData == null || m_matchVariables == null)
            {
#if UNITY_EDITOR
                Debug.Log("MatchManager: Forgot to add a Scriptable Object in the Editor!");
#endif
                return;
            }

            ++m_matchVariables.CurrentPointsTPTwo;
            ++m_matchVariables.TotalPointsTPTwo;
            //Player 2
            m_playerData[1].TotalPoints = m_matchVariables.TotalPointsTPTwo;
            //Player 4
            m_playerData[3].TotalPoints = m_matchVariables.TotalPointsTPTwo;

            CheckMatchConditions();
        }

        private void UpdateTPTwoPoints()
        {
            if (m_playerData == null || m_matchVariables == null)
            {
#if UNITY_EDITOR
                Debug.Log("MatchManager: Forgot to add a Scriptable Object in the Editor!");
#endif
                return;
            }

            ++m_matchVariables.CurrentPointsTPOne;
            ++m_matchVariables.TotalPointsTPOne;

            //Player 1
            m_playerData[0].TotalPoints = m_matchVariables.TotalPointsTPOne;
            //Player 3
            m_playerData[2].TotalPoints = m_matchVariables.TotalPointsTPOne;

            CheckMatchConditions();
        }

        #region Match-Presets
        private void ResetPlayfield()
        {
            m_ground.transform.localScale = new Vector3(m_matchVariables.SetGroundWidth * m_groundWidthScale, m_ground.transform.localScale.y, m_matchVariables.SetGroundLength * m_groundLengthScale);
        }

        private void ResetRoundValues()
        {
            m_matchVariables.CurrentPointsTPOne = 0;
            m_matchVariables.CurrentPointsTPTwo = 0;
        }
        #endregion

        private void CheckMatchConditions()
        {
            bool nextRoundConditionIsMet = 
                m_matchVariables.CurrentPointsTPOne >= m_matchVariables.SetMaxPoints &&
                m_matchVariables.CurrentPointsTPOne >= m_matchVariables.CurrentPointsTPTwo + m_matchVariables.WinPointDifference
                ||
                m_matchVariables.CurrentPointsTPTwo >= m_matchVariables.SetMaxPoints &&
                m_matchVariables.CurrentPointsTPTwo >= m_matchVariables.CurrentPointsTPOne + m_matchVariables.WinPointDifference;

            bool winConditionIsMet = m_matchVariables.CurrentRoundNr == m_matchVariables.SetMaxRounds && nextRoundConditionIsMet;

            if(winConditionIsMet)
            {
                m_StartWinProcedure.Invoke();
                return;
            };

            switch (nextRoundConditionIsMet)
            {
                case true:
                {
                    m_StartNextRound?.Invoke();
                    break; 
                }
                case false:
                {
#if UNITY_EDITOR
                    Debug.Log("Requirements for the next round not met, yet.");
#endif
                    break; 
                }
            }
        }

        private void StartNextRound()
        {
            //reuse of 'MatchVariables/MatchSettings public bool InfiniteRounds'.
            if (m_matchVariables == null || m_matchVariables.InfiniteRounds)
                return;
#if UNITY_EDITOR
            Debug.Log("Next Round starts!");
#endif
            //May save RoundPoints of Players/Teams.
            //TODO: May implement a procedure to transition into the next set Round.

            //Increase the RoundNr by 1.
            m_matchVariables.CurrentRoundNr++;
            ResetRoundValues();
        }

        private void StartWinProcedure()
        {
#if UNITY_EDITOR
            Debug.Log("Won!");
#endif
            //TODO: Start WinProcedure.
        }
    }
}