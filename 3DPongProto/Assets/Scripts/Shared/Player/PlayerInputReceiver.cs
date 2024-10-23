using System;
using ThreeDeePongProto.Offline.Managers;
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

        //MatchManager pauses the Game. Coroutines and the Inputsystem.PlayerActions get disabled inside this class.
        public static event Action InGameMenuOpens;

        private void OnDisable()
        {
            m_playerInputActions.PlayerActions.Disable();

            InGameMenuOpens -= DisablePlayerActions;
            InGameMenuActions.CloseInGameMenu -= ReEnablePlayerActions;
            DisablePlayerActions();

            m_playerInputActions.PlayerActions.PushPaddleNegZP1.performed -= PaddlePushInputP1;
            m_playerInputActions.PlayerActions.PushPaddleNegZP1.canceled -= CanceledPaddlePushInputP1;
            m_playerInputActions.PlayerActions.PushPaddlePosZP2.performed -= PaddlePushInputP2;
            m_playerInputActions.PlayerActions.PushPaddlePosZP2.canceled -= CanceledPaddlePushInputP2;
            m_playerInputActions.PlayerActions.PushPaddleNegZP3.performed -= PaddlePushInputP3;
            m_playerInputActions.PlayerActions.PushPaddleNegZP3.canceled -= CanceledPaddlePushInputP3;
            m_playerInputActions.PlayerActions.PushPaddlePosZP4.performed -= PaddlePushInputP4;
            m_playerInputActions.PlayerActions.PushPaddlePosZP4.canceled -= CanceledPaddlePushInputP4;
        }

        /// <summary>
        /// PlayerController and UIControls need to be moved into 'Start()' and the PlayerInputActions of the InputManager into 'Awake()', to prevent Exceptions.
        /// </summary>
        private void Start()
        {
            m_playerInputActions = InputManager.m_PlayerInputActions;
            m_playerInputActions.PlayerActions.Enable();

            InGameMenuOpens += DisablePlayerActions;
            InGameMenuActions.CloseInGameMenu += ReEnablePlayerActions;
            ReEnablePlayerActions();

            m_playerInputActions.PlayerActions.PushPaddleNegZP1.performed += PaddlePushInputP1;
            m_playerInputActions.PlayerActions.PushPaddleNegZP1.canceled += CanceledPaddlePushInputP1;
            m_playerInputActions.PlayerActions.PushPaddlePosZP2.performed += PaddlePushInputP2;
            m_playerInputActions.PlayerActions.PushPaddlePosZP2.canceled += CanceledPaddlePushInputP2;
            m_playerInputActions.PlayerActions.PushPaddleNegZP3.performed += PaddlePushInputP3;
            m_playerInputActions.PlayerActions.PushPaddleNegZP3.canceled += CanceledPaddlePushInputP3;
            m_playerInputActions.PlayerActions.PushPaddlePosZP4.performed += PaddlePushInputP4;
            m_playerInputActions.PlayerActions.PushPaddlePosZP4.canceled += CanceledPaddlePushInputP4;
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

        #region Custom Methods
        /// <summary>
        /// Get's called at Start and when the Menu closes again.
        /// </summary>
        private void ReEnablePlayerActions()
        {
            m_playerInputActions.Enable();
            m_playerController.m_playerMovement.ReStartPushCoroutine();
        }

        private void DisablePlayerActions()
        {
            m_playerInputActions.Disable();
            m_playerController.m_playerMovement.StopPushCoroutine();
        }
        #endregion

        #region CallbackContext Methods
        protected void ToggleMenu(InputAction.CallbackContext _callbackContext)
        {
            if (m_playerController.m_matchManager == null)
                m_playerController.m_matchManager = FindObjectOfType<MatchManager>();  //Required, if not catched with '[SerializeField]'.

            if (!m_playerController.m_matchManager.GameIsPaused && m_playerInputActions.PlayerActions.enabled)
            {
                InGameMenuOpens?.Invoke();
                InputManager.ToggleActionMaps(InputManager.m_PlayerInputActions.UI);
            }
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