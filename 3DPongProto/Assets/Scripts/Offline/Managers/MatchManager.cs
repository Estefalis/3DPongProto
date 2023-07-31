using ThreeDeePongProto.Offline.UI;
using UnityEngine;

namespace ThreeDeePongProto.Managers
{
    public class MatchManager : MonoBehaviour
    {
        [SerializeField] private GameObject m_ground;
        [SerializeField] private float m_groundWidthScale, m_groundLengthScale;
        [SerializeField] private MatchVariables m_MatchVariables;

        private void Awake()
        {
            ReSetMatchObjects();
            MenuOrganisation.RestartGameLevel += ReSetMatchObjects;
        }

        private void OnDisable()
        {
            MenuOrganisation.RestartGameLevel -= ReSetMatchObjects;
        }

        private void ReSetMatchObjects()
        {
            m_ground.transform.localScale = new Vector3(m_MatchVariables.SetGroundWidth * m_groundWidthScale, m_ground.transform.localScale.y, m_MatchVariables.SetGroundLength * m_groundLengthScale);
        }
    }
}