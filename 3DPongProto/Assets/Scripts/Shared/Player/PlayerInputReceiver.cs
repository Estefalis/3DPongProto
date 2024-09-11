using ThreeDeePongProto.Shared.InputActions;
using UnityEngine;

namespace ThreeDeePongProto.Shared.Player
{
    public class PlayerInputReceiver : MonoBehaviour
    {
        private PlayerInputActions m_playerInputActions;
        [SerializeField] internal PlayerController m_playerController;

        private void OnDisable()
        {
            m_playerInputActions.PlayerActions.Disable();
        }

        /// <summary>
        /// PlayerController and UIControls need to be moved into 'Start()' and the PlayerInputActions of the InputManager into 'Awake()', to prevent Exceptions.
        /// </summary>
        private void Start()
        {
            m_playerInputActions = InputManager.m_playerInputActions;
            m_playerInputActions.PlayerActions.Enable();
        }
    }
}