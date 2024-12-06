using ThreeDeePongProto.Offline.UI.Menu;
using ThreeDeePongProto.Shared.InputActions;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ThreeDeePongProto.Shared.Player
{
    internal class PlayerInputReceiver : MonoBehaviour
    {
        private PlayerInputActions m_playerInputActions;
        [SerializeField] internal PlayerController m_playerController;

        private Vector3 m_sideMoveVector;

        private void OnDisable()
        {
            m_playerInputActions.PlayerActions.Disable();

            switch (m_playerController.m_playerId)
            {
                case 0:
                {
                    m_playerInputActions.PlayerActions.PushPaddleNegZP1.performed -= PaddlePushInputP1;
                    m_playerInputActions.PlayerActions.PushPaddleNegZP1.canceled -= CanceledPaddlePushInputP1;
                    break;
                }
                case 1:
                {
                    m_playerInputActions.PlayerActions.PushPaddlePosZP2.performed -= PaddlePushInputP2;
                    m_playerInputActions.PlayerActions.PushPaddlePosZP2.canceled -= CanceledPaddlePushInputP2;
                    break;
                }
                case 2:
                {
                    m_playerInputActions.PlayerActions.PushPaddleNegZP3.performed -= PaddlePushInputP3;
                    m_playerInputActions.PlayerActions.PushPaddleNegZP3.canceled -= CanceledPaddlePushInputP3;
                    break;
                }
                case 3:
                {
                    m_playerInputActions.PlayerActions.PushPaddlePosZP4.performed -= PaddlePushInputP4;
                    m_playerInputActions.PlayerActions.PushPaddlePosZP4.canceled -= CanceledPaddlePushInputP4;
                    break;
                }
            }

            m_playerInputActions.PlayerActions.ToggleGameMenu.performed -= OnMenuOpening;
            MenuManager.BackToGame -= OnMenuClosing;
            MenuManager.RestartGame -= OnMenuClosing;
        }

        /// <summary>
        /// PlayerController and UIControls need to be moved into 'Start()' and the PlayerInputActions of the InputManager into 'Awake()', to prevent Exceptions.
        /// </summary>
        private void Start()
        {
            m_playerInputActions = InputManager.m_PlayerInputActions;
            m_playerInputActions.PlayerActions.Enable();

            switch (m_playerController.m_playerId)
            {
                case 0:
                {
                    m_playerInputActions.PlayerActions.PushPaddleNegZP1.performed += PaddlePushInputP1;
                    m_playerInputActions.PlayerActions.PushPaddleNegZP1.canceled += CanceledPaddlePushInputP1;
                    break;
                }
                case 1:
                {
                    m_playerInputActions.PlayerActions.PushPaddlePosZP2.performed += PaddlePushInputP2;
                    m_playerInputActions.PlayerActions.PushPaddlePosZP2.canceled += CanceledPaddlePushInputP2;
                    break;
                }
                case 2:
                {
                    m_playerInputActions.PlayerActions.PushPaddleNegZP3.performed += PaddlePushInputP3;
                    m_playerInputActions.PlayerActions.PushPaddleNegZP3.canceled += CanceledPaddlePushInputP3;
                    break;
                }
                case 3:
                {
                    m_playerInputActions.PlayerActions.PushPaddlePosZP4.performed += PaddlePushInputP4;
                    m_playerInputActions.PlayerActions.PushPaddlePosZP4.canceled += CanceledPaddlePushInputP4;
                    break;
                }
            }

            m_playerInputActions.PlayerActions.ToggleGameMenu.performed += OnMenuOpening;
            MenuManager.BackToGame += OnMenuClosing;
            MenuManager.RestartGame += OnMenuClosing;
        }

        private void FixedUpdate()
        {
            switch (m_playerController.m_playerId)
            {
                case 0:
                {
                    m_playerController.m_playerMovement.m_sideMoveVector =
                        m_playerInputActions.PlayerActions.SideMovementNegZP1.ReadValue<Vector2>();
                    m_playerController.m_playerMovement.m_axisRotation =
                        m_playerInputActions.PlayerActions.RotatePaddleNegZP1.ReadValue<Vector2>();
                    break;
                }
                case 1:
                {
                    m_playerController.m_playerMovement.m_sideMoveVector =
                        m_playerInputActions.PlayerActions.SideMovementPosZP2.ReadValue<Vector2>();
                    m_playerController.m_playerMovement.m_axisRotation =
                        m_playerInputActions.PlayerActions.RotatePaddlePosZP2.ReadValue<Vector2>();
                    break;
                }
                case 2:
                {
                    m_playerController.m_playerMovement.m_sideMoveVector =
                        m_playerInputActions.PlayerActions.SideMovementNegZP3.ReadValue<Vector2>();
                    m_playerController.m_playerMovement.m_axisRotation =
                        m_playerInputActions.PlayerActions.RotatePaddleNegZP3.ReadValue<Vector2>();
                    break;
                }
                case 3:
                {
                    m_playerController.m_playerMovement.m_sideMoveVector =
                        m_playerInputActions.PlayerActions.SideMovementPosZP4.ReadValue<Vector2>();
                    m_playerController.m_playerMovement.m_axisRotation =
                        m_playerInputActions.PlayerActions.RotatePaddlePosZP4.ReadValue<Vector2>();
                    break;
                }
                default:
                    break;
            }
        }

        private void OnMenuClosing()
        {
            m_playerInputActions.Enable();
        }

        #region CallbackContext Methods
        private void OnMenuOpening(InputAction.CallbackContext _callbackContext)
        {
            m_playerInputActions.Disable();  //Paddles can still be moved, if 'm_playerInputActions' here isn't disabled.
            InputManager.ToggleActionMaps(InputManager.m_PlayerInputActions.UI);
            m_playerController.m_playerMovement.StopPushCoroutine();
        }

        private void PaddlePushInputP1(InputAction.CallbackContext _callbackContext)
        {
            if (!m_playerController.m_playerMovement.m_blockPushInput)
            {
                if (!m_playerController.m_playerMovement.m_tempBlocked)
                {
                    //'ReadValueAsButton()' is only available inside these CallbackContext-Methods.
                    m_playerController.m_playerMovement.m_pushPlayer = _callbackContext.ReadValueAsButton();
                    m_playerController.m_playerMovement.m_receivedPlayerId = m_playerController.m_playerId;
                }
            }
        }

        private void CanceledPaddlePushInputP1(InputAction.CallbackContext _callbackContext)
        {
            m_playerController.m_playerMovement.m_pushPlayer = false;
        }

        private void PaddlePushInputP2(InputAction.CallbackContext _callbackContext)
        {
            if (!m_playerController.m_playerMovement.m_blockPushInput)
            {
                if (!m_playerController.m_playerMovement.m_tempBlocked)
                {
                    //'ReadValueAsButton()' is only available inside these CallbackContext-Methods.
                    m_playerController.m_playerMovement.m_pushPlayer = _callbackContext.ReadValueAsButton();
                    m_playerController.m_playerMovement.m_receivedPlayerId = m_playerController.m_playerId;
                }
            }
        }

        private void CanceledPaddlePushInputP2(InputAction.CallbackContext _callbackContext)
        {
            m_playerController.m_playerMovement.m_pushPlayer = false;
        }

        private void PaddlePushInputP3(InputAction.CallbackContext _callbackContext)
        {
            if (!m_playerController.m_playerMovement.m_blockPushInput)
            {
                if (!m_playerController.m_playerMovement.m_tempBlocked)
                {
                    //'ReadValueAsButton()' is only available inside these CallbackContext-Methods.
                    m_playerController.m_playerMovement.m_pushPlayer = _callbackContext.ReadValueAsButton();
                    m_playerController.m_playerMovement.m_receivedPlayerId = m_playerController.m_playerId;
                }
            }
        }

        private void CanceledPaddlePushInputP3(InputAction.CallbackContext _callbackContext)
        {
            m_playerController.m_playerMovement.m_pushPlayer = false;
        }

        private void PaddlePushInputP4(InputAction.CallbackContext _callbackContext)
        {
            if (!m_playerController.m_playerMovement.m_blockPushInput)
            {
                if (!m_playerController.m_playerMovement.m_tempBlocked)
                {
                    //'ReadValueAsButton()' is only available inside these CallbackContext-Methods.
                    m_playerController.m_playerMovement.m_pushPlayer = _callbackContext.ReadValueAsButton();
                    m_playerController.m_playerMovement.m_receivedPlayerId = m_playerController.m_playerId;
                }
            }
        }

        private void CanceledPaddlePushInputP4(InputAction.CallbackContext _callbackContext)
        {
            m_playerController.m_playerMovement.m_pushPlayer = false;
        }
        #endregion
    }
}