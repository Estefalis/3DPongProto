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

        //private PlayerInput m_playerInput;
        //private PlayerInput[] m_allPlayerInputs = new PlayerInput[4];

        private Vector2 m_sideMoveVectorP1, m_sideMoveVectorP2, m_sideMoveVectorP3, m_sideMoveVectorP4;
        private Vector2 m_axisRotation1, m_axisRotation2, m_axisRotation3, m_axisRotation4;

        private void OnDisable()
        {
            SetActionMaps(m_playerController.m_playerId, false);

            InputSystem.onDeviceChange -= OnDeviceChange;
            MenuManager.BackToGame -= ButtonClosesMenu;
            MenuManager.RestartGame -= ButtonClosesMenu;
        }

        /// <summary>
        /// PlayerController and UIControls need to be moved into 'Start()' and the PlayerInputActions of the InputManager into 'Awake()', to prevent Exceptions.
        /// </summary>
        private void Start()
        {
            m_playerInputActions = InputManager.m_PlayerInputActions;
            SetActionMaps(m_playerController.m_playerId, true);

            //for (int i = 0; i < m_allPlayerInputs.Length; i++)
            //{
            //    if (m_playerController.m_playerId == i) //'m_playerInput.playerIndex' equals 'm_playerController.m_playerId'.
            //    {
            //        m_allPlayerInputs[i] = gameObject.GetComponentInParent<PlayerInput>();

            //        if (m_playerInput == null)
            //        {
            //            var parent = gameObject.transform.parent.parent;
            //            m_allPlayerInputs[i] = parent.gameObject.AddComponent<PlayerInput>();
            //            //var addedPlayerInput = gameObject.AddComponent<PlayerInput>();
            //            //m_allPlayerInputs[i] = addedPlayerInput;
            //            m_allPlayerInputs[i].actions = m_playerInputActions.asset;
            //            m_allPlayerInputs[i].neverAutoSwitchControlSchemes = false;
            //            m_allPlayerInputs[i].notificationBehavior = PlayerNotifications.InvokeCSharpEvents;
            //        }
            //        //m_allPlayerInputs[m_playerController.m_playerId] = m_playerInput;
            //        //Debug.Log($"{m_playerInput.playerIndex} - {m_playerController.m_playerId} - {i} - {m_allPlayerInputs[i].name}");
            //    }
            //}

            InputSystem.onDeviceChange += OnDeviceChange;
            MenuManager.BackToGame += ButtonClosesMenu;
            MenuManager.RestartGame += ButtonClosesMenu;
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
        private void SetActionMaps(int _ownPlayerID, bool _onOff)
        {
            switch (_onOff)
            {
                case true:
                {
                    m_playerInputActions.Enable();
                    m_playerInputActions.PlayerActions.ToggleGameMenu.performed += OnMenuOpening;

                    switch (_ownPlayerID)
                    {
                        case 0:
                        {
                            m_playerInputActions.PlayerActions.MoveNegZP1.performed += MoveSidewardsP1;
                            m_playerInputActions.PlayerActions.MoveNegZP1.canceled += CancelMoveSidewardsP1;
                            m_playerInputActions.PlayerActions.RotateNegZP1.performed += RotatePaddleP1;
                            m_playerInputActions.PlayerActions.RotateNegZP1.canceled += CancelRotatePaddleP1;
                            m_playerInputActions.PlayerActions.PushNegZP1.performed += PushInputP1;
                            m_playerInputActions.PlayerActions.PushNegZP1.canceled += CancelPushInputP1;
                            break;
                        }
                        case 1:
                        {
                            m_playerInputActions.PlayerActions.MovePosZP2.performed += MoveSidewardsP2;
                            m_playerInputActions.PlayerActions.MovePosZP2.canceled += CancelMoveSidewardsP2;
                            m_playerInputActions.PlayerActions.RotatePosZP2.performed += RotatePaddleP2;
                            m_playerInputActions.PlayerActions.RotatePosZP2.canceled += CancelRotatePaddleP2;
                            m_playerInputActions.PlayerActions.PushPosZP2.performed += PushInputP2;
                            m_playerInputActions.PlayerActions.PushPosZP2.canceled += CancelPushInputP2;
                            break;
                        }
                        case 2:
                        {
                            m_playerInputActions.PlayerActions.MoveNegZP3.performed += MoveSidewardsP3;
                            m_playerInputActions.PlayerActions.MoveNegZP3.canceled += CancelMoveSidewardsP3;
                            m_playerInputActions.PlayerActions.RotateNegZP3.performed += RotatePaddleP3;
                            m_playerInputActions.PlayerActions.RotateNegZP3.canceled += CancelRotatePaddleP3;
                            m_playerInputActions.PlayerActions.PushNegZP3.performed += PushInputP3;
                            m_playerInputActions.PlayerActions.PushNegZP3.canceled += CancelPushInputP3;
                            break;
                        }
                        case 3:
                        {
                            m_playerInputActions.PlayerActions.MovePosZP4.performed += MoveSidewardsP4;
                            m_playerInputActions.PlayerActions.MovePosZP4.canceled += CancelMoveSidewardsP4;
                            m_playerInputActions.PlayerActions.RotatePosZP4.performed += RotatePaddleP4;
                            m_playerInputActions.PlayerActions.RotatePosZP4.canceled += CancelRotatePaddleP4;
                            m_playerInputActions.PlayerActions.PushPosZP4.performed += PushInputP4;
                            m_playerInputActions.PlayerActions.PushPosZP4.canceled += CancelPushInputP4;
                            break;
                        }
                    }

                    break;
                }
                case false:
                {
                    m_playerInputActions.Disable();
                    m_playerInputActions.PlayerActions.ToggleGameMenu.performed -= OnMenuOpening;

                    switch (_ownPlayerID)
                    {
                        case 0:
                        {
                            m_playerInputActions.PlayerActions.MoveNegZP1.performed -= MoveSidewardsP1;
                            m_playerInputActions.PlayerActions.MoveNegZP1.canceled -= CancelMoveSidewardsP1;
                            m_playerInputActions.PlayerActions.RotateNegZP1.performed -= RotatePaddleP1;
                            m_playerInputActions.PlayerActions.RotateNegZP1.canceled -= CancelRotatePaddleP1;
                            m_playerInputActions.PlayerActions.PushNegZP1.performed -= PushInputP1;
                            m_playerInputActions.PlayerActions.PushNegZP1.canceled -= CancelPushInputP1;
                            break;
                        }
                        case 1:
                        {
                            m_playerInputActions.PlayerActions.MovePosZP2.performed -= MoveSidewardsP2;
                            m_playerInputActions.PlayerActions.MovePosZP2.canceled -= CancelMoveSidewardsP2;
                            m_playerInputActions.PlayerActions.RotatePosZP2.performed -= RotatePaddleP2;
                            m_playerInputActions.PlayerActions.RotatePosZP2.canceled -= CancelRotatePaddleP2;
                            m_playerInputActions.PlayerActions.PushPosZP2.performed -= PushInputP2;
                            m_playerInputActions.PlayerActions.PushPosZP2.canceled -= CancelPushInputP2;
                            break;
                        }
                        case 2:
                        {
                            m_playerInputActions.PlayerActions.MoveNegZP3.performed -= MoveSidewardsP3;
                            m_playerInputActions.PlayerActions.MoveNegZP3.canceled -= CancelMoveSidewardsP3;
                            m_playerInputActions.PlayerActions.RotateNegZP3.performed -= RotatePaddleP3;
                            m_playerInputActions.PlayerActions.RotateNegZP3.canceled -= CancelRotatePaddleP3;
                            m_playerInputActions.PlayerActions.PushNegZP3.performed -= PushInputP3;
                            m_playerInputActions.PlayerActions.PushNegZP3.canceled -= CancelPushInputP3;
                            break;
                        }
                        case 3:
                        {
                            m_playerInputActions.PlayerActions.MovePosZP4.performed -= MoveSidewardsP4;
                            m_playerInputActions.PlayerActions.MovePosZP4.canceled -= CancelMoveSidewardsP4;
                            m_playerInputActions.PlayerActions.RotatePosZP4.performed -= RotatePaddleP4;
                            m_playerInputActions.PlayerActions.RotatePosZP4.canceled -= CancelRotatePaddleP4;
                            m_playerInputActions.PlayerActions.PushPosZP4.performed -= PushInputP4;
                            m_playerInputActions.PlayerActions.PushPosZP4.canceled -= CancelPushInputP4;
                            break;
                        }
                    }

                    break;
                }
            }
        }

        private void ButtonClosesMenu()
        {
            m_playerInputActions.Enable();
        }
        #endregion

        #region CallbackContext-Methods
        /// <summary>
        /// Use 'var _control = _callbackContext._control;' to get the _control that triggered the action. 
        /// Receive 'var currentControlScheme = GetActiveControlScheme(_control);' to display the triggered controlScheme and action part that triggered the change.
        /// Example: Debug.Log($"Control Scheme: {currentControlScheme} (Triggered by: {_control.displayName}).");
        /// </summary>
        /// <param name="_control"></param>
        /// <returns></returns>
        private string GetActiveControlScheme(InputControl _control)
        {
            foreach (var scheme in m_playerInputActions.controlSchemes)
            {
                //Check if the '_control' belongs to this scheme.
                if (scheme.SupportsDevice(_control.device))     //'_control.device' contains 'device' and 'deviceId' to compare devices.
                {
                    return scheme.name; //Return the name of the matched scheme.
                }
            }

            return "Unknown Control Scheme";
        }

        private void OnMenuOpening(InputAction.CallbackContext _callbackContext)
        {
            //if (m_playerController.m_playerId == 0)     //Equal to Master on Online Games?
            InputManager.ToggleActionMaps(InputManager.m_PlayerInputActions.UserInterface);
            m_playerInputActions.Disable();  //Paddles can still be moved, if 'm_playerInputActions' here isn't disabled.
            m_playerController.m_playerMovement.StopPushCoroutine();
        }

        #region Player1
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

        private void PushInputP1(InputAction.CallbackContext _callbackContext)
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

        private void CancelPushInputP1(InputAction.CallbackContext _callbackContext)
        {
            m_playerController.m_playerMovement.m_pushPlayer = false;
        }
        #endregion

        #region Player2
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

        private void PushInputP2(InputAction.CallbackContext _callbackContext)
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

        private void CancelPushInputP2(InputAction.CallbackContext _callbackContext)
        {
            m_playerController.m_playerMovement.m_pushPlayer = false;
        }
        #endregion

        #region Player3
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

        private void PushInputP3(InputAction.CallbackContext _callbackContext)
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

        private void CancelPushInputP3(InputAction.CallbackContext _callbackContext)
        {
            m_playerController.m_playerMovement.m_pushPlayer = false;
        }
        #endregion

        #region Player4
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

        private void PushInputP4(InputAction.CallbackContext _callbackContext)
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

        private void CancelPushInputP4(InputAction.CallbackContext _callbackContext)
        {
            m_playerController.m_playerMovement.m_pushPlayer = false;
        }
        #endregion
        #endregion

        private void OnDeviceChange(InputDevice _inputDevice, InputDeviceChange _deviceChange)
        {
            if (m_playerController.m_playerId != 0)
                return;
            switch (_deviceChange)
            {
                case InputDeviceChange.Added:
                    //New Device.
                    Debug.Log($"{_inputDevice} got added.");
                    break;
                case InputDeviceChange.Removed:
                    //Remove from Input System entirely; by default, Devices stay in the system once discovered.
                    Debug.Log($"{_inputDevice} got removed.");
                    break;
                case InputDeviceChange.Disconnected:
                    //If this is happening, activate some boolean that will tell the game that a controller is now "missing".
                    Debug.Log($"{_inputDevice} got disconnected.");
                    break;
                case InputDeviceChange.Reconnected:
                    //Plugged back in.
                    Debug.Log($"{_inputDevice} got reconnected.");
                    break;
                default:
                    //Always includes a default case for when a unused case is being called. Leave it empty.
                    break;
            }
        }
    }
}