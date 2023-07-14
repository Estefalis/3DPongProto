using UnityEngine;

namespace ThreeDeePongProto.Managers
{
    public class LevelManager : MonoBehaviour
    {
        [SerializeField] private GameObject m_ground;
        [SerializeField] private float m_groundWidth, m_groundLength;
        [SerializeField] private PlayfieldVariables m_playfieldVariables;

        //[SerializeField] private List<PlayerMovement> m_playerList = new List<PlayerMovement>();

        private void Awake()
        {
            //m_ground.transform.localScale = new Vector3(GameManager.Instance.MaxFieldWidth * m_groundWidth, m_ground.transform.localScale.y, GameManager.Instance.MaxFieldLength * m_groundLength);
            
            m_ground.transform.localScale = new Vector3(m_playfieldVariables.GroundWidth * m_groundWidth, m_ground.transform.localScale.y, m_playfieldVariables.GroundLength * m_groundLength);
        }

        //public void AddPlayer(PlayerMovement _player, uint _playerId)
        //{
        //    if (_playerId == 0)
        //        return;
        //    m_playerList.Insert((int)_playerId - 1, _player);
        //}

        //public void RemovePlayer(PlayerMovement _player, uint _playerId)
        //{
        //    if (_playerId == 0)
        //        return;
        //    m_playerList.Insert((int)_playerId, _player);
        //}
    }
}