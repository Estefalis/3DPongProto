using UnityEngine;

namespace ThreeDeePongProto.Managers
{
    public class LevelManager : MonoBehaviour
    {
        [SerializeField] private GameObject m_ground;
        [SerializeField] private readonly float m_groundWidthScale, m_groundLengthScale;
        [SerializeField] private MatchVariables m_MatchVariables;

        private void Awake()
        {
            m_ground.transform.localScale = new Vector3(m_MatchVariables.GroundWidth * m_groundWidthScale, m_ground.transform.localScale.y, m_MatchVariables.GroundLength * m_groundLengthScale);
        }
    }
}