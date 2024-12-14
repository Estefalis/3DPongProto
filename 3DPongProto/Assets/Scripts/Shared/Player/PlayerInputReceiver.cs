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

        private Vector2 m_sideMoveVectorP1, m_sideMoveVectorP2, m_sideMoveVectorP3, m_sideMoveVectorP4;
        private Vector2 m_axisRotation1, m_axisRotation2, m_axisRotation3, m_axisRotation4;

        private void OnDisable()
        {
            SetActionMaps(m_playerController.m_playerId, false);

            MenuManager.BackToGame -= OnMenuClosing;
            MenuManager.RestartGame -= OnMenuClosing;
        }

        /// <summary>
        /// PlayerController and UIControls need to be moved into 'Start()' and the PlayerInputActions of the InputManager into 'Awake()', to prevent Exceptions.
        /// </summary>
        private void Start()
        {
            m_playerInputActions = InputManager.m_PlayerInputActions;
            SetActionMaps(m_playerController.m_playerId, true);

            MenuManager.BackToGame += OnMenuClosing;
            MenuManager.RestartGame += OnMenuClosing;
        }

        private void FixedUpdate()
        {
            switch (m_playerController.m_playerId)
            {
                case 0:
                {
                    m_playerController.m_playerMovement.m_sideMoveVector = m_sideMoveVectorP1;
                    m_playerController.m_playerMovement.m_axisRotation = m_axisRotation1;
                    break;
                }
                case 1:
                {
                    m_playerController.m_playerMovement.m_sideMoveVector = m_sideMoveVectorP2;
                    m_playerController.m_playerMovement.m_axisRotation = m_axisRotation2;
                    break;
                }
                case 2:
                {
                    m_playerController.m_playerMovement.m_sideMoveVector = m_sideMoveVectorP3;
                    m_playerController.m_playerMovement.m_axisRotation = m_axisRotation3;
                    break;
                }
                case 3:
                {
                    m_playerController.m_playerMovement.m_sideMoveVector = m_sideMoveVectorP4;
                    m_playerController.m_playerMovement.m_axisRotation = m_axisRotation4;
                    break;
                }
                default:
                    break;
            }
        }

        #region Custom-Methods
        private void SetActionMaps(int _playerID, bool _onOff)
        {
            switch (_onOff)
            {
                case true:
                {
                    m_playerInputActions.Enable();
                    //InputManager.ToggleActionMaps(InputManager.m_PlayerInputActions.PlayerActions);
                    //m_playerInputActions.PlayerActions.ToggleGameMenu.performed += OnMenuOpening;

                    switch (_playerID)
                    {
                        case 0:
                        {
                            m_playerInputActions.PlayfieldActionsP1.PushPaddleNegZP1.performed += PaddlePushInputP1;
                            m_playerInputActions.PlayfieldActionsP1.PushPaddleNegZP1.canceled += CancelPaddlePushInputP1;
                            m_playerInputActions.PlayfieldActionsP1.SideMovementNegZP1.performed += MoveSidewardsP1;
                            m_playerInputActions.PlayfieldActionsP1.SideMovementNegZP1.canceled += CancelMoveSidewardsP1;
                            m_playerInputActions.PlayfieldActionsP1.RotatePaddleNegZP1.performed += RotatePaddleP1;
                            m_playerInputActions.PlayfieldActionsP1.RotatePaddleNegZP1.canceled += CancelRotatePaddleP1;
                            InputManager.ToggleActionMaps(InputManager.m_PlayerInputActions.PlayfieldActionsP1);
                            break;
                        }
                        case 1:
                        {
                            m_playerInputActions.PlayfieldActionsP2.PushPaddlePosZP2.performed += PaddlePushInputP2;
                            m_playerInputActions.PlayfieldActionsP2.PushPaddlePosZP2.canceled += CancelPaddlePushInputP2;
                            m_playerInputActions.PlayfieldActionsP2.SideMovementPosZP2.performed += MoveSidewardsP2;
                            m_playerInputActions.PlayfieldActionsP2.SideMovementPosZP2.canceled += CancelMoveSidewardsP2;
                            m_playerInputActions.PlayfieldActionsP2.RotatePaddlePosZP2.performed += RotatePaddleP2;
                            m_playerInputActions.PlayfieldActionsP2.RotatePaddlePosZP2.canceled += CancelRotatePaddleP2;
                            InputManager.ToggleActionMaps(InputManager.m_PlayerInputActions.PlayfieldActionsP2);
                            break;
                        }
                        case 2:
                        {
                            m_playerInputActions.PlayfieldActionsP3.PushPaddleNegZP3.performed += PaddlePushInputP3;
                            m_playerInputActions.PlayfieldActionsP3.PushPaddleNegZP3.canceled += CancelPaddlePushInputP3;
                            m_playerInputActions.PlayfieldActionsP3.SideMovementNegZP3.performed += MoveSidewardsP3;
                            m_playerInputActions.PlayfieldActionsP3.SideMovementNegZP3.canceled += CancelMoveSidewardsP3;
                            m_playerInputActions.PlayfieldActionsP3.RotatePaddleNegZP3.performed += RotatePaddleP3;
                            m_playerInputActions.PlayfieldActionsP3.RotatePaddleNegZP3.canceled += CancelRotatePaddleP3;
                            InputManager.ToggleActionMaps(InputManager.m_PlayerInputActions.PlayfieldActionsP3);
                            break;
                        }
                        case 3:
                        {
                            m_playerInputActions.PlayfieldActionsP4.PushPaddlePosZP4.performed += PaddlePushInputP4;
                            m_playerInputActions.PlayfieldActionsP4.PushPaddlePosZP4.canceled += CancelPaddlePushInputP4;
                            m_playerInputActions.PlayfieldActionsP4.SideMovementPosZP4.performed += MoveSidewardsP4;
                            m_playerInputActions.PlayfieldActionsP4.SideMovementPosZP4.canceled += CancelMoveSidewardsP4;
                            m_playerInputActions.PlayfieldActionsP4.RotatePaddlePosZP4.performed += RotatePaddleP4;
                            m_playerInputActions.PlayfieldActionsP4.RotatePaddlePosZP4.canceled += CancelRotatePaddleP4;
                            InputManager.ToggleActionMaps(InputManager.m_PlayerInputActions.PlayfieldActionsP4);
                            break;
                        }
                    }

                    m_playerInputActions.Playfield.ToggleGameMenu.performed += OnMenuOpening;

                    break;
                }
                case false:
                {
                    m_playerInputActions.Disable();
                    //m_playerInputActions.PlayerActions.ToggleGameMenu.performed -= OnMenuOpening;

                    switch (_playerID)
                    {
                        case 0:
                        {
                            m_playerInputActions.PlayfieldActionsP1.PushPaddleNegZP1.performed -= PaddlePushInputP1;
                            m_playerInputActions.PlayfieldActionsP1.PushPaddleNegZP1.canceled -= CancelPaddlePushInputP1;
                            m_playerInputActions.PlayfieldActionsP1.SideMovementNegZP1.performed -= MoveSidewardsP1;
                            m_playerInputActions.PlayfieldActionsP1.SideMovementNegZP1.canceled -= CancelMoveSidewardsP1;
                            m_playerInputActions.PlayfieldActionsP1.RotatePaddleNegZP1.performed -= RotatePaddleP1;
                            m_playerInputActions.PlayfieldActionsP1.RotatePaddleNegZP1.canceled -= CancelRotatePaddleP1;
                            break;
                        }
                        case 1:
                        {
                            m_playerInputActions.PlayfieldActionsP2.PushPaddlePosZP2.performed -= PaddlePushInputP2;
                            m_playerInputActions.PlayfieldActionsP2.PushPaddlePosZP2.canceled -= CancelPaddlePushInputP2;
                            m_playerInputActions.PlayfieldActionsP2.SideMovementPosZP2.performed -= MoveSidewardsP2;
                            m_playerInputActions.PlayfieldActionsP2.SideMovementPosZP2.canceled -= CancelMoveSidewardsP2;
                            m_playerInputActions.PlayfieldActionsP2.RotatePaddlePosZP2.performed -= RotatePaddleP2;
                            m_playerInputActions.PlayfieldActionsP2.RotatePaddlePosZP2.canceled -= CancelRotatePaddleP2;
                            break;
                        }
                        case 2:
                        {
                            m_playerInputActions.PlayfieldActionsP3.PushPaddleNegZP3.canceled -= CancelPaddlePushInputP3;
                            m_playerInputActions.PlayfieldActionsP3.PushPaddleNegZP3.performed -= PaddlePushInputP3;
                            m_playerInputActions.PlayfieldActionsP3.SideMovementNegZP3.performed -= MoveSidewardsP3;
                            m_playerInputActions.PlayfieldActionsP3.SideMovementNegZP3.canceled -= CancelMoveSidewardsP3;
                            m_playerInputActions.PlayfieldActionsP3.RotatePaddleNegZP3.performed -= RotatePaddleP3;
                            m_playerInputActions.PlayfieldActionsP3.RotatePaddleNegZP3.canceled -= CancelRotatePaddleP3;
                            break;
                        }
                        case 3:
                        {
                            m_playerInputActions.PlayfieldActionsP4.PushPaddlePosZP4.performed -= PaddlePushInputP4;
                            m_playerInputActions.PlayfieldActionsP4.PushPaddlePosZP4.canceled -= CancelPaddlePushInputP4;
                            m_playerInputActions.PlayfieldActionsP4.SideMovementPosZP4.performed -= MoveSidewardsP4;
                            m_playerInputActions.PlayfieldActionsP4.SideMovementPosZP4.canceled -= CancelMoveSidewardsP4;
                            m_playerInputActions.PlayfieldActionsP4.RotatePaddlePosZP4.performed -= RotatePaddleP4;
                            m_playerInputActions.PlayfieldActionsP4.RotatePaddlePosZP4.canceled -= CancelRotatePaddleP4;
                            break;
                        }
                    }

                    m_playerInputActions.Playfield.ToggleGameMenu.performed -= OnMenuOpening;

                    break;
                }
            }
        }

        private void OnMenuClosing()
        {
            m_playerInputActions.Enable();
        }
        #endregion

        #region CallbackContext-Methods
        private void OnMenuOpening(InputAction.CallbackContext _callbackContext)
        {
            m_playerInputActions.Disable();  //Paddles can still be moved, if 'm_playerInputActions' here isn't disabled.
            if (m_playerController.m_playerId == 0)
                InputManager.ToggleActionMaps(InputManager.m_PlayerInputActions.UIActions);
            m_playerController.m_playerMovement.StopPushCoroutine();
        }

        #region Player1
        private void PaddlePushInputP1(InputAction.CallbackContext _callbackContext)
        {
            if (!m_playerController.m_playerMovement.m_blockPushInput)
            {
                if (!m_playerController.m_playerMovement.m_tempBlocked)
                {
                    //'ReadValueAsButton()' is only available inside these CallbackContext-Methods.
                    m_playerController.m_playerMovement.m_receivedPlayerId = m_playerController.m_playerId;
                    m_playerController.m_playerMovement.m_pushPlayer = _callbackContext.ReadValueAsButton();
                }
            }
        }

        private void CancelPaddlePushInputP1(InputAction.CallbackContext _callbackContext)
        {
            m_playerController.m_playerMovement.m_pushPlayer = false;
        }

        private void MoveSidewardsP1(InputAction.CallbackContext _callbackContext)
        {
            m_playerController.m_playerMovement.m_receivedPlayerId = m_playerController.m_playerId;
            m_sideMoveVectorP1 = _callbackContext.ReadValue<Vector2>();
        }

        private void CancelMoveSidewardsP1(InputAction.CallbackContext _callbackContext)
        {
            m_sideMoveVectorP1 = Vector2.zero;
        }

        private void RotatePaddleP1(InputAction.CallbackContext _callbackContext)
        {
            m_playerController.m_playerMovement.m_receivedPlayerId = m_playerController.m_playerId;
            m_axisRotation1 = _callbackContext.ReadValue<Vector2>();
        }

        private void CancelRotatePaddleP1(InputAction.CallbackContext __callbackContext)
        {
            m_axisRotation1 = Vector2.zero;
        }
        #endregion

        #region Player2
        private void PaddlePushInputP2(InputAction.CallbackContext _callbackContext)
        {
            if (!m_playerController.m_playerMovement.m_blockPushInput)
            {
                if (!m_playerController.m_playerMovement.m_tempBlocked)
                {
                    //'ReadValueAsButton()' is only available inside these CallbackContext-Methods.
                    m_playerController.m_playerMovement.m_receivedPlayerId = m_playerController.m_playerId;
                    m_playerController.m_playerMovement.m_pushPlayer = _callbackContext.ReadValueAsButton();
                }
            }
        }

        private void CancelPaddlePushInputP2(InputAction.CallbackContext _callbackContext)
        {
            m_playerController.m_playerMovement.m_pushPlayer = false;
        }

        private void MoveSidewardsP2(InputAction.CallbackContext _callbackContext)
        {
            m_playerController.m_playerMovement.m_receivedPlayerId = m_playerController.m_playerId;
            m_sideMoveVectorP2 = _callbackContext.ReadValue<Vector2>();
        }

        private void CancelMoveSidewardsP2(InputAction.CallbackContext _callbackContext)
        {
            m_sideMoveVectorP2 = Vector2.zero;
        }

        private void RotatePaddleP2(InputAction.CallbackContext _callbackContext)
        {
            m_playerController.m_playerMovement.m_receivedPlayerId = m_playerController.m_playerId;
            m_axisRotation2 = _callbackContext.ReadValue<Vector2>();
        }

        private void CancelRotatePaddleP2(InputAction.CallbackContext _callbackContext)
        {
            m_axisRotation2 = Vector2.zero;
        }
        #endregion

        #region Player3
        private void PaddlePushInputP3(InputAction.CallbackContext _callbackContext)
        {
            if (!m_playerController.m_playerMovement.m_blockPushInput)
            {
                if (!m_playerController.m_playerMovement.m_tempBlocked)
                {
                    //'ReadValueAsButton()' is only available inside these CallbackContext-Methods.
                    m_playerController.m_playerMovement.m_receivedPlayerId = m_playerController.m_playerId;
                    m_playerController.m_playerMovement.m_pushPlayer = _callbackContext.ReadValueAsButton();
                }
            }
        }

        private void CancelPaddlePushInputP3(InputAction.CallbackContext _callbackContext)
        {
            m_playerController.m_playerMovement.m_pushPlayer = false;
        }

        private void MoveSidewardsP3(InputAction.CallbackContext _callbackContext)
        {
            m_playerController.m_playerMovement.m_receivedPlayerId = m_playerController.m_playerId;
            m_sideMoveVectorP3 = _callbackContext.ReadValue<Vector2>();
        }

        private void CancelMoveSidewardsP3(InputAction.CallbackContext _callbackContext)
        {
            m_sideMoveVectorP3 = Vector2.zero;
        }

        private void RotatePaddleP3(InputAction.CallbackContext _callbackContext)
        {
            m_playerController.m_playerMovement.m_receivedPlayerId = m_playerController.m_playerId;
            m_axisRotation3 = _callbackContext.ReadValue<Vector2>();
        }

        private void CancelRotatePaddleP3(InputAction.CallbackContext _callbackContext)
        {
            m_axisRotation3 = Vector2.zero;
        }
        #endregion

        #region Player4
        private void PaddlePushInputP4(InputAction.CallbackContext _callbackContext)
        {
            if (!m_playerController.m_playerMovement.m_blockPushInput)
            {
                if (!m_playerController.m_playerMovement.m_tempBlocked)
                {
                    //'ReadValueAsButton()' is only available inside these CallbackContext-Methods.
                    m_playerController.m_playerMovement.m_receivedPlayerId = m_playerController.m_playerId;
                    m_playerController.m_playerMovement.m_pushPlayer = _callbackContext.ReadValueAsButton();
                }
            }
        }

        private void CancelPaddlePushInputP4(InputAction.CallbackContext _callbackContext)
        {
            m_playerController.m_playerMovement.m_pushPlayer = false;
        }

        private void MoveSidewardsP4(InputAction.CallbackContext _callbackContext)
        {
            m_playerController.m_playerMovement.m_receivedPlayerId = m_playerController.m_playerId;
            m_sideMoveVectorP4 = _callbackContext.ReadValue<Vector2>();
        }

        private void CancelMoveSidewardsP4(InputAction.CallbackContext _callbackContext)
        {
            m_sideMoveVectorP4 = Vector2.zero;
        }

        private void RotatePaddleP4(InputAction.CallbackContext _callbackContext)
        {
            m_playerController.m_playerMovement.m_receivedPlayerId = m_playerController.m_playerId;
            m_axisRotation4 = _callbackContext.ReadValue<Vector2>();
        }

        private void CancelRotatePaddleP4(InputAction.CallbackContext _callbackContext)
        {
            m_axisRotation4 = Vector2.zero;
        }
        #endregion
        #endregion
    }
}