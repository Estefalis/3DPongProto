using UnityEngine;

namespace ThreeDeePongProto.Shared.Player
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] internal Transform m_inputAndCamComponent;
        [SerializeField] internal PlayerInputReceiver m_playerInputReceiver;
        [SerializeField] internal PlayerMovement m_playerMovement;
        [SerializeField] internal PlayerInteractions m_playerInteractions;
        [SerializeField] internal PlayerHealth m_playerHealth;
        [SerializeField] internal PlayerCameraController m_playerCameraControl;
    }
}