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

        ////MatchManager pauses the Game. Coroutines and the Inputsystem.PlayerActions get disabled inside this class.
        //public static event Action InGameMenuOpens;

        private void OnDisable()
        {
            m_playerInputActions.PlayerActions.Disable();

            m_playerInputActions.PlayerActions.PushPaddleNegZP1.performed -= PaddlePushInputP1;
            m_playerInputActions.PlayerActions.PushPaddleNegZP1.canceled -= CanceledPaddlePushInputP1;
            m_playerInputActions.PlayerActions.PushPaddlePosZP2.performed -= PaddlePushInputP2;
            m_playerInputActions.PlayerActions.PushPaddlePosZP2.canceled -= CanceledPaddlePushInputP2;
            m_playerInputActions.PlayerActions.PushPaddleNegZP3.performed -= PaddlePushInputP3;
            m_playerInputActions.PlayerActions.PushPaddleNegZP3.canceled -= CanceledPaddlePushInputP3;
            m_playerInputActions.PlayerActions.PushPaddlePosZP4.performed -= PaddlePushInputP4;
            m_playerInputActions.PlayerActions.PushPaddlePosZP4.canceled -= CanceledPaddlePushInputP4;

            m_playerInputActions.UI.ToggleGameMenu.performed -= EnablePlayerControls;
            m_playerInputActions.PlayerActions.ToggleGameMenu.performed -= DisablePlayerControls;
        }

        /// <summary>
        /// PlayerController and UIControls need to be moved into 'Start()' and the PlayerInputActions of the InputManager into 'Awake()', to prevent Exceptions.
        /// </summary>
        private void Start()
        {
            m_playerInputActions = InputManager.m_PlayerInputActions;
            m_playerInputActions.PlayerActions.Enable();

            m_playerInputActions.PlayerActions.PushPaddleNegZP1.performed += PaddlePushInputP1;
            m_playerInputActions.PlayerActions.PushPaddleNegZP1.canceled += CanceledPaddlePushInputP1;
            m_playerInputActions.PlayerActions.PushPaddlePosZP2.performed += PaddlePushInputP2;
            m_playerInputActions.PlayerActions.PushPaddlePosZP2.canceled += CanceledPaddlePushInputP2;
            m_playerInputActions.PlayerActions.PushPaddleNegZP3.performed += PaddlePushInputP3;
            m_playerInputActions.PlayerActions.PushPaddleNegZP3.canceled += CanceledPaddlePushInputP3;
            m_playerInputActions.PlayerActions.PushPaddlePosZP4.performed += PaddlePushInputP4;
            m_playerInputActions.PlayerActions.PushPaddlePosZP4.canceled += CanceledPaddlePushInputP4;

            m_playerInputActions.UI.ToggleGameMenu.performed += EnablePlayerControls;
            m_playerInputActions.PlayerActions.ToggleGameMenu.performed += DisablePlayerControls;
        }

        private void FixedUpdate()
        {
            switch (m_playerController.m_playerIDData.PlayerId)
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

        #region CallbackContext Methods
        private void EnablePlayerControls(InputAction.CallbackContext _callbackContext)
        {
            m_playerInputActions.Enable();
            m_playerController.m_playerMovement.ReStartPushCoroutine();
        }

        private void DisablePlayerControls(InputAction.CallbackContext _callbackContext)
        {
            m_playerInputActions.Disable();
            m_playerController.m_playerMovement.StopPushCoroutine();
        }

        private void PaddlePushInputP1(InputAction.CallbackContext _callbackContext)
        {
            if (!m_playerController.m_playerMovement.m_blockPushInput)
            {
                if (!m_playerController.m_playerMovement.m_tempBlocked)
                {
                    //'ReadValueAsButton()' is only available inside these CallbackContext-Methods.
                    m_playerController.m_playerMovement.m_receivedPlayerId = m_playerController.m_playerIDData.PlayerId;
                    m_playerController.m_playerMovement.m_pushPlayer = _callbackContext.ReadValueAsButton();
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
                    m_playerController.m_playerMovement.m_receivedPlayerId = m_playerController.m_playerIDData.PlayerId;
                    m_playerController.m_playerMovement.m_pushPlayer = _callbackContext.ReadValueAsButton();
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
                    m_playerController.m_playerMovement.m_receivedPlayerId = m_playerController.m_playerIDData.PlayerId;
                    m_playerController.m_playerMovement.m_pushPlayer = _callbackContext.ReadValueAsButton();
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
                    m_playerController.m_playerMovement.m_receivedPlayerId = m_playerController.m_playerIDData.PlayerId;
                    m_playerController.m_playerMovement.m_pushPlayer = _callbackContext.ReadValueAsButton();
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