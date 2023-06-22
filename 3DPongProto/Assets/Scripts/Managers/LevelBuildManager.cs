using UnityEngine;

namespace ThreeDeePongProto.Managers
{
    public class LevelBuildManager : MonoBehaviour
    {
        [SerializeField] private GameObject m_ground;
        [SerializeField] private float m_groundWidth, m_groundLength;

        private void Awake()
        {
            m_ground.transform.localScale = new Vector3(GameManager.Instance.MaxFieldWidth * m_groundWidth, m_ground.transform.localScale.y, GameManager.Instance.MaxFieldLength * m_groundLength);
        }
    }
}