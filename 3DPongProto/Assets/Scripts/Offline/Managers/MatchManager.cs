using ThreeDeePongProto.Offline.UI;
using UnityEngine;

namespace ThreeDeePongProto.Managers
{
    public class MatchManager : MonoBehaviour
    {
        [SerializeField] private GameObject m_ground;
        [SerializeField] private float m_groundWidthScale, m_groundLengthScale;
        [SerializeField] private MatchVariables m_matchVariables;

        private void Awake()
        {
            ReSetMatch();

            MenuOrganisation.RestartGameLevel += ReSetMatch;
        }

        private void OnDisable()
        {
            MenuOrganisation.RestartGameLevel -= ReSetMatch;
        }

        private void ReSetMatch()
        {
            m_ground.transform.localScale = new Vector3(m_matchVariables.SetGroundWidth * m_groundWidthScale, m_ground.transform.localScale.y, m_matchVariables.SetGroundLength * m_groundLengthScale);
            
            m_matchVariables.CurrentPointsTeamOne = 0;
            m_matchVariables.CurrentPointsTeamTwo = 0;
        }
    }
}