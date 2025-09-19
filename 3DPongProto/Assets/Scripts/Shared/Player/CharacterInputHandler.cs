using System;
using System.Collections.Generic;
using ThreeDeePongProto.Shared.Managers;
using UnityEngine;
using UnityEngine.InputSystem;

public enum SearchDirection
{
    Forwards,
    Backwards,
    FullScan
}

namespace ThreeDeePongProto.Shared.PlayerCharacter
{
    internal class CharacterInputHandler : MonoBehaviour, IProvidePlayerID
    {
        [SerializeField] internal PlayerInput m_playerInput;
        [SerializeField] internal CharacterMainController m_playerController;

        private InputActionMap m_uiMap, m_playerMap;

        private const string m_moveString = "Move", m_rotateString = "Rotate", m_pushString = "Push", m_resetString = "ResetRotation", m_zoomString = "Zoom";
        private const string m_kickBallString = "KickBall", m_cursorString = "CursorVisibility", m_openGameMenuString = "OpenGameMenu", m_mousePositionString = "MousePosition";

        internal List<InputBinding> m_playerBindings = new();
        private int m_playerID;
        private Vector2 m_rotationVector;   //Saved current rotationInput.

        #region Actions_and_Functions
        internal static event Action<int> AKickBall;
        internal static event Action<Vector2> ASendMousePosition;
        #endregion

        //private PlayerProfileData m_playerProfile;

        private void OnEnable()
        {
            if (m_playerController == null)
                m_playerController.GetComponentInParent<CharacterMainController>();

            if (m_playerInput != null)
                SubscribeToInputActions(m_playerInput);
        }

        private void OnDisable()
        {
            if (m_playerInput != null)
                UnsubscribeToInputActions(m_playerInput);
        }

        private void OnDestroy()
        {
            if (m_playerInput != null)
                UnsubscribeToInputActions(m_playerInput);
        }

        private void FixedUpdate()
        {
            if (!IsLocalPlayer())
                return;

            //Use Rotation constantly.
            if (m_playerController.m_playerMovement == null)
                return;

            m_playerController.m_playerMovement.SetInputVector(m_rotationVector, m_playerID, true);
        }

        #region Un-Subscribe_Methods
        private void SubscribeToInputActions(PlayerInput _playerInput)
        {
            if (_playerInput.user.id != m_playerInput.user.id)
                return;

            UserInputManager.AChangeActiveActionMap += OnChangeActiveActionMap;
            m_playerInput.onActionTriggered += HandleActionTriggered;
        }

        private void UnsubscribeToInputActions(PlayerInput _playerInput)
        {
            if (_playerInput.user.id != m_playerInput.user.id)
                return;

            UserInputManager.AChangeActiveActionMap -= OnChangeActiveActionMap;
            m_playerInput.onActionTriggered -= HandleActionTriggered;
        }
        #endregion

        public void SetPlayerID(int _playerID)
        {
            m_playerID = _playerID;

            m_playerMap = m_playerInput.actions.FindActionMap(EInputActionMaps.PlayerActions.ToString());
            m_uiMap = m_playerInput.actions.FindActionMap(EInputActionMaps.UserInterface.ToString());

            foreach (var actionMap in m_playerInput.actions.actionMaps)
            {
                if (actionMap == m_playerMap)
                    actionMap.Enable();
                else
                    actionMap.Disable();
            }
        }

        private bool IsLocalPlayer()
        {
#if PHOTON_UNITY_NETWORKING
                return GetComponent<PhotonView>().IsMine;
#elif UNITY_NETCODE
                return GetComponent<NetworkBehaviour>().IsLocalPlayer;
#else
            return true; //SinglePlayer.
#endif
        }

        private void OnChangeActiveActionMap(string _actionMap)
        {
            if (_actionMap == EInputActionMaps.UserInterface.ToString())
            {
                m_playerInput.SwitchCurrentActionMap(EInputActionMaps.UserInterface.ToString());
            }
            else if (_actionMap == EInputActionMaps.PlayerActions.ToString())
            {
                m_playerInput.SwitchCurrentActionMap(EInputActionMaps.PlayerActions.ToString());
            }
        }

        #region CallbackContext_Methods
        private void HandleActionTriggered(InputAction.CallbackContext _callbackContext)
        {
            switch (_callbackContext.action.name)
            {
                case m_moveString:
                {
                    switch (_callbackContext.action.phase)
                    {
                        case InputActionPhase.Performed:
                        {
                            OnMove(_callbackContext);
                            break;
                        }
                        case InputActionPhase.Canceled:
                        {
                            OnMoveCanceled(_callbackContext);
                            break;
                        }
                    }
                    break;
                }
                case m_rotateString:
                {
                    switch (_callbackContext.action.phase)
                    {
                        case InputActionPhase.Performed:
                        {
                            OnRotate(_callbackContext);
                            break;
                        }
                        case InputActionPhase.Canceled:
                        {
                            OnRotateCanceled(_callbackContext);
                            break;
                        }
                        default:
                            break;
                    }
                    break;
                }
                case m_pushString:
                {
                    switch (_callbackContext.action.phase)
                    {
                        case InputActionPhase.Performed:
                        {
                            OnPush(_callbackContext);
                            break;
                        }
                        default:
                            break;
                    }
                    break;
                }
                case m_resetString:
                {
                    switch (_callbackContext.action.phase)
                    {
                        case InputActionPhase.Performed:
                        {
                            OnResetRotation(_callbackContext);
                            break;
                        }
                        default:
                            break;
                    }
                    break;
                }
                case m_zoomString:
                {
                    switch (_callbackContext.action.phase)
                    {
                        case InputActionPhase.Performed:
                        {
                            OnZoom(_callbackContext);
                            break;
                        }
                        case InputActionPhase.Canceled:
                        {
                            OnZoomCanceled(_callbackContext);
                            break;
                        }
                        default:
                            break;
                    }
                    break;
                }
                case m_kickBallString:
                {
                    switch (_callbackContext.action.phase)
                    {
                        case InputActionPhase.Performed:
                        {
                            OnKickBall(_callbackContext);
                            break;
                        }
                        default:
                            break;
                    }
                    break;
                }
                case m_cursorString:
                {
                    switch (_callbackContext.action.phase)
                    {
                        case InputActionPhase.Performed:
                        {
                            Debug.Log("OnCursorVisibility not implemented, yet.");
                            break;
                        }
                        default:
                            break;
                    }
                    break;
                }
                case m_mousePositionString:
                {
                    switch (_callbackContext.action.phase)
                    {
                        case InputActionPhase.Performed:
                        {
                            OnMousePosition(_callbackContext);
                            break;
                        }
                        default:
                            break;
                    }
                    break;
                }
                case m_openGameMenuString:
                {
                    switch (_callbackContext.action.phase)
                    {
                        case InputActionPhase.Performed:
                        {
                            OnOpenMenu(_callbackContext);
                            break;
                        }
                        default:
                            break;
                    }
                    break;
                }
                default:
                    break;
            }
        }

        private void OnMove(InputAction.CallbackContext _callbackContext)
        {
            if (!IsLocalPlayer())
                return;

            #region Save for later!
            //string bindingControlScheme = _callbackContext.action.bindings[_callbackContext.action.GetBindingIndexForControl(_callbackContext.control)].groups;

            //if (bindingControlScheme != _playerInput.currentControlScheme)
            //    return;
            #endregion
            
            Vector2 moveVector = _callbackContext.ReadValue<Vector2>();
            m_playerController.m_playerMovement.SetInputVector(moveVector, m_playerID, false);
        }

        private void OnMoveCanceled(InputAction.CallbackContext _callbackContext)
        {
            if (!IsLocalPlayer())
                return;

            Vector2 moveVector = Vector2.zero;
            m_playerController.m_playerMovement.SetInputVector(moveVector, m_playerID, false);
        }

        //Process 'Rotate'-Action.
        private void OnRotate(InputAction.CallbackContext _callbackContext)
        {
            if (!IsLocalPlayer())
                return;
            
            m_rotationVector = _callbackContext.ReadValue<Vector2>();
        }

        private void OnRotateCanceled(InputAction.CallbackContext _callbackContext)
        {
            if (!IsLocalPlayer())
                return;

            m_rotationVector = Vector2.zero; //Reset InputVector, once button is released/playerAction is canceled.
        }

        //Process 'Push'-Action.
        private void OnPush(InputAction.CallbackContext _callbackContext)
        {
            if (!IsLocalPlayer())
                return;
            
            if (_callbackContext.ReadValueAsButton())
            {
                m_playerController.m_playerMovement.m_receivedUserID = m_playerInput.user.id;
                m_playerController.m_playerMovement.InitializePush(m_playerID, true);
            }
        }

        private void OnResetRotation(InputAction.CallbackContext _callbackContext)
        {
            if (!IsLocalPlayer())
                return;

            if (_callbackContext.ReadValueAsButton())
                m_playerController.m_playerMovement.ResetPlayerRotation(m_playerID);
        }

        private void OnKickBall(InputAction.CallbackContext _callbackContext)
        {
            if (!IsLocalPlayer())
                return;

            if (_callbackContext.ReadValueAsButton())
                AKickBall?.Invoke(m_playerID);  //Tell the Ball, that it has been kicked! *kick*
        }

        private void OnZoom(InputAction.CallbackContext _callbackContext)
        {
            if (!IsLocalPlayer())
                return;

            Vector2 zoomVector = _callbackContext.ReadValue<Vector2>();
            m_playerController.m_playerCameraController.Zoom(zoomVector, _callbackContext.control.device, m_playerID);
        }

        private void OnZoomCanceled(InputAction.CallbackContext _callbackContext)
        {
            if (!IsLocalPlayer())
                return;

            m_playerController.m_playerCameraController.Zoom(Vector2.zero, _callbackContext.control.device, m_playerID);
        }

        private void OnMousePosition(InputAction.CallbackContext _callbackContext)
        {
            if (!IsLocalPlayer())
                return;

            Vector2 mouseVector = _callbackContext.ReadValue<Vector2>();
            ASendMousePosition?.Invoke(mouseVector);
        }

        private void OnOpenMenu(InputAction.CallbackContext _callbackContext)
        {
            if (!IsLocalPlayer())
                return;

            //Depending on MenuManager's inactive firstElement in GameScene, it triggers PlayerActions ActionMap in GameScene.
            if (UserInputManager.SetActionMap == EInputActionMaps.PlayerActions.ToString())
                UserInputManager.Instance.ToggleActionMaps(EInputActionMaps.UserInterface.ToString());
        }

        //internal void StoreData(PlayerProfileData _playerProfile)
        //{
        //    m_playerProfile = _playerProfile;
        //}
        #endregion
    }
}