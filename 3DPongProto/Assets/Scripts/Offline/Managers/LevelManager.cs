using UnityEngine;

namespace ThreeDeePongProto.Managers
{
    public class LevelManager : MonoBehaviour
    {
        [SerializeField] private GameObject m_ground;
        [SerializeField] private float m_groundWidth, m_groundLength;
        [SerializeField] private MatchVariables m_MatchVariables;

        private void Awake()
        {
            //m_ground.transform.localScale = new Vector3(GameManager.Instance.MaxFieldWidth * m_groundWidth, m_ground.transform.localScale.y, GameManager.Instance.MaxFieldLength * m_groundLength);
            
            m_ground.transform.localScale = new Vector3(m_MatchVariables.GroundWidth * m_groundWidth, m_ground.transform.localScale.y, m_MatchVariables.GroundLength * m_groundLength);
        }
    }
}