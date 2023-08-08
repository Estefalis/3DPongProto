using ThreeDeePongProto.Offline.UI;
using UnityEngine;

namespace ThreeDeePongProto.Managers
{
    public class MatchManager : MonoBehaviour
    {
        [SerializeField] private GameObject m_ground;
        [SerializeField] private float m_groundWidthScale, m_groundLengthScale;
        [SerializeField] private MatchVariables m_matchVariables;
        [SerializeField] private PlayerData[] m_playerData;

        private void Awake()
        {
            ReSetMatch();

            MenuOrganisation.RestartGameLevel += ReSetMatch;
            BallMovement.m_HitGoalOne += UpdateMatchPoints;
            BallMovement.m_HitGoalTwo += UpdateMatchPoints;
        }

        private void OnDisable()
        {
            MenuOrganisation.RestartGameLevel -= ReSetMatch;
            BallMovement.m_HitGoalOne -= UpdateMatchPoints;
            BallMovement.m_HitGoalTwo -= UpdateMatchPoints;
        }

        private void ReSetMatch()
        {
            m_ground.transform.localScale = new Vector3(m_matchVariables.SetGroundWidth * m_groundWidthScale, m_ground.transform.localScale.y, m_matchVariables.SetGroundLength * m_groundLengthScale);

            m_matchVariables.CurrentPointsTPOne = 0;
            m_matchVariables.CurrentPointsTPTwo = 0;
        }

        private void UpdateMatchPoints(uint _index)
        {
            if (m_playerData == null || m_matchVariables == null)
            {
#if UNITY_EDITOR
                Debug.Log("MatchManager: Forgot to add a Scriptable Object in the Editor!");
#endif
                return;
            }

            switch (_index)
            {
                case 1:
                {
                    ++m_matchVariables.CurrentPointsTPOne;
                    ++m_matchVariables.TotalPointsTPOne;

                    //Player 1
                    m_playerData[0].TotalPoints = m_matchVariables.TotalPointsTPOne;
                    //Player 3
                    m_playerData[2].TotalPoints = m_matchVariables.TotalPointsTPOne;
                    break;
                }
                case 2:
                {
                    ++m_matchVariables.CurrentPointsTPTwo;
                    ++m_matchVariables.TotalPointsTPTwo;
                    //Player 2
                    m_playerData[1].TotalPoints = m_matchVariables.TotalPointsTPTwo;
                    //Player 4
                    m_playerData[3].TotalPoints = m_matchVariables.TotalPointsTPTwo;
                    break;
                }
                default:
                    break;
            }
        }
    }
}