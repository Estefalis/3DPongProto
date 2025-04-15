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
            return true; //Singleplayer.
#endif
        }

        private void OnChangeActiveActionMap(string _actionMap)
        {
            if (_actionMap == EInputActionMaps.UserInterface.ToString())
            {
                //m_playerMap.Disable();
                m_playerInput.SwitchCurrentActionMap(EInputActionMaps.UserInterface.ToString());
            }
            else if (_actionMap == EInputActionMaps.PlayerActions.ToString())
            {
                //m_playerMap.Enable();
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
            }
        }

        private void OnMove(InputAction.CallbackContext _callbackContext)
        {
            if (!IsLocalPlayer())
                return;

            //string bindingControlScheme = _callbackContext.action.bindings[_callbackContext.action.GetBindingIndexForControl(_callbackContext.control)].groups;

            //if (bindingControlScheme != _playerInput.currentControlScheme)
            //    return;

            Vector2 moveVector = _callbackContext.ReadValue<Vector2>();
            m_playerController.m_playerMovement.SetInputVector(moveVector, m_playerID, false);
        }

        private void OnMoveCanceled(InputAction.CallbackContext _callbackContext)
        {
            if (!IsLocalPlayer())
                return;

            //if (_playerInput.devices.Contains(_callbackContext.control.device))
            //{
            //    string bindingControlScheme = _callbackContext.action.bindings[_callbackContext.action.GetBindingIndexForControl(_callbackContext.control)].groups;

            //    if (bindingControlScheme == _playerInput.currentControlScheme)
            //    {
            Vector2 moveVector = Vector2.zero;
            m_playerController.m_playerMovement.SetInputVector(moveVector, m_playerID, false);
            //    }
            //}
        }

        //Process 'Rotate'-Action.
        private void OnRotate(InputAction.CallbackContext _callbackContext)
        {
            if (!IsLocalPlayer())
                return;

            //string bindingControlScheme = _callbackContext.action.bindings[_callbackContext.action.GetBindingIndexForControl(_callbackContext.control)].groups;

            //if (bindingControlScheme == _playerInput.currentControlScheme)
            m_rotationVector = _callbackContext.ReadValue<Vector2>();
        }

        private void OnRotateCanceled(InputAction.CallbackContext _callbackContext)
        {
            if (!IsLocalPlayer())
                return;

            //string bindingControlScheme = _callbackContext.action.bindings[_callbackContext.action.GetBindingIndexForControl(_callbackContext.control)].groups;

            //if (bindingControlScheme == _playerInput.currentControlScheme)
            m_rotationVector = Vector2.zero; //Reset InputVector, once button is released/playerAction is canceled.
        }

        //Process 'Push'-Action.
        private void OnPush(InputAction.CallbackContext _callbackContext)
        {
            if (!IsLocalPlayer())
                return;

            //string bindingControlScheme = _callbackContext.action.bindings[_callbackContext.action.GetBindingIndexForControl(_callbackContext.control)].groups;

            //if (bindingControlScheme == _playerInput.currentControlScheme)
            //{
            if (_callbackContext.ReadValueAsButton())
            {
                m_playerController.m_playerMovement.m_receivedUserID = m_playerInput.user.id;
                m_playerController.m_playerMovement.InitializePush(m_playerID, true);
            }
            //}
        }

        private void OnResetRotation(InputAction.CallbackContext _callbackContext)
        {
            if (!IsLocalPlayer())
                return;

            //string bindingControlScheme = _callbackContext.action.bindings[_callbackContext.action.GetBindingIndexForControl(_callbackContext.control)].groups;

            //if (bindingControlScheme == _playerInput.currentControlScheme)
            //{
            if (_callbackContext.ReadValueAsButton())
                m_playerController.m_playerMovement.ResetPlayerRotation(m_playerID);
            //}
        }

        private void OnKickBall(InputAction.CallbackContext _callbackContext)
        {
            if (!IsLocalPlayer())
                return;

            //string bindingControlScheme = _callbackContext.action.bindings[_callbackContext.action.GetBindingIndexForControl(_callbackContext.control)].groups;

            //if (bindingControlScheme == _playerInput.currentControlScheme)
            //{
            if (_callbackContext.ReadValueAsButton())
                AKickBall?.Invoke(m_playerID);  //Tell the Ball, that it has been kicked! *kick*
            //}
        }

        private void OnZoom(InputAction.CallbackContext _callbackContext)
        {
            if (!IsLocalPlayer())
                return;

            //string bindingControlScheme = _callbackContext.action.bindings[_callbackContext.action.GetBindingIndexForControl(_callbackContext.control)].groups;

            //if (bindingControlScheme == _playerInput.currentControlScheme)
            //{
            Vector2 zoomVector = _callbackContext.ReadValue<Vector2>();
            m_playerController.m_playerCameraController.Zooming(zoomVector, m_playerID, m_playerInput.currentControlScheme);
            //}
        }

        private void OnZoomCanceled(InputAction.CallbackContext _callbackContext)
        {
            if (!IsLocalPlayer())
                return;

            //string bindingControlScheme = _callbackContext.action.bindings[_callbackContext.action.GetBindingIndexForControl(_callbackContext.control)].groups;

            //if (bindingControlScheme == _playerInput.currentControlScheme)
            m_playerController.m_playerCameraController.Zooming(Vector2.zero, m_playerID, "");
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

            if (/*m_playerID == 0 && */UserInputManager.SetActionMap == EInputActionMaps.PlayerActions.ToString())
                UserInputManager.ToggleActionMaps(EInputActionMaps.UserInterface.ToString());
        }
        #endregion
    }
}