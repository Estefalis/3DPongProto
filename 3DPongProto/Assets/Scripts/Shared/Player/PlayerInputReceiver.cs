using System;
using ThreeDeePongProto.Shared.InputActions;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ThreeDeePongProto.Shared.Player
{
    internal class PlayerInputReceiver : MonoBehaviour
    {
        [SerializeField] internal PlayerController m_playerController;

        private Vector2 m_currentRotationInput; //Save current rotationInput.
        private const string m_moveAction = "Move", m_rotateAction = "Rotate", m_pushAction = "Push";
        private const string m_kickAction = "Kick";
        private const string m_zoomAction = "Zoom", m_mousePosAction = "MousePosition";
        private const string m_toggleMenuAction = "ToggleGameMenu", m_CursorVisAction = "CursorVisibility";

        public static event Action<int> m_KickBall;
        public static event Action m_menuOpens;

        private void OnEnable()
        {
            var playerInput = m_playerController.GetComponent<PlayerInput>();
            playerInput.onActionTriggered += OnActionTriggered;
        }

        private void OnDisable()
        {
            var playerInput = m_playerController.GetComponent<PlayerInput>();
            playerInput.onActionTriggered -= OnActionTriggered;
        }

        private void FixedUpdate()
        {
            //Use Rotation constantly.
            if (m_currentRotationInput != Vector2.zero)
                m_playerController.m_playerMovement.SetInputVector(m_currentRotationInput, m_playerController.m_playerId, true);
        }

        #region Removed_Methods_before_PlayerInput_Component
        //private void BindPlayerInputs()
        //{
        //    // Bind Move input (all players)
        //    m_playerInputActions.PlayerActions.Move.performed += _callbackContext => HandleMove(_callbackContext, m_playerController.m_playerId);
        //    m_playerInputActions.PlayerActions.Move.canceled += _callbackContext => HandleMoveCancel(_callbackContext, m_playerController.m_playerId);

        //    // Bind Push input (all players)
        //    m_playerInputActions.PlayerActions.Push.performed += _callbackContext => HandlePush(_callbackContext, m_playerController.m_playerId);
        //    m_playerInputActions.PlayerActions.Push.canceled += _callbackContext => HandlePushCancel(_callbackContext, m_playerController.m_playerId);
        //}

        //private void UnbindPlayerInputs()
        //{
        //    // Unbind Move input
        //    m_playerInputActions.PlayerActions.Move.performed -= _callbackContext => HandleMove(_callbackContext, m_playerController.m_playerId);
        //    m_playerInputActions.PlayerActions.Move.canceled -= _callbackContext => HandleMoveCancel(_callbackContext, m_playerController.m_playerId);

        //    // Unbind Push input
        //    m_playerInputActions.PlayerActions.Push.performed -= _callbackContext => HandlePush(_callbackContext, m_playerController.m_playerId);
        //    m_playerInputActions.PlayerActions.Push.canceled -= _callbackContext => HandlePushCancel(_callbackContext, m_playerController.m_playerId);
        //}

        //private void HandleMove(InputAction.CallbackContext _callbackContext, int _playerId)
        //{
        //    if (_playerId == m_playerController.m_playerId) // Ensure correct player
        //    {
        //        m_sideMoveVector = _callbackContext.ReadValue<Vector2>();
        //        m_playerController.m_playerMovement.SetInputVector(m_sideMoveVector, m_playerController.m_playerId, false);
        //    }
        //}

        //private void HandleMoveCancel(InputAction.CallbackContext _callbackContext, int _playerId)
        //{
        //    if (_playerId == m_playerController.m_playerId) // Ensure correct player
        //    {
        //        m_sideMoveVector = Vector2.zero;
        //        m_playerController.m_playerMovement.SetInputVector(m_sideMoveVector, m_playerController.m_playerId, false);
        //    }
        //}

        //private void HandlePush(InputAction.CallbackContext _callbackContext, int _playerId)
        //{
        //    if (_playerId == m_playerController.m_playerId) // Ensure correct player
        //    {
        //        bool isPushing = _callbackContext.ReadValue<float>() > 0; // Push-Button pressed?
        //        m_playerController.m_playerMovement.PushProgress(isPushing);
        //    }
        //}

        //private void HandlePushCancel(InputAction.CallbackContext _callbackContext, int _playerId)
        //{
        //    if (_playerId == m_playerController.m_playerId) // Ensure correct player
        //    {
        //        m_isPushing = false;
        //        m_playerController.m_playerMovement.PushProgress(m_isPushing);
        //    }
        //}
        #endregion

        #region PlayerInput_Component
        //Process all actions central.
        private void OnActionTriggered(InputAction.CallbackContext _callbackContext)
        {
            //switch (_callbackContext.action.actionMap)
            //{
            //}

            //Identify Actions by their Names.
            switch (_callbackContext.action.name)
            {
                case m_moveAction:
                    HandleMove(_callbackContext);
                    break;
                case m_rotateAction:
                    HandleRotate(_callbackContext);
                    break;
                case m_pushAction:
                    HandlePush(_callbackContext);
                    break;
                case m_kickAction:
                {
                    if (_callbackContext.action.WasPerformedThisFrame())    //Else started- & canceled-Phase invoke the event as well.
                        m_KickBall?.Invoke(m_playerController.m_playerId);  //Tell the Ball, that it has been kicked! *kick*
                    break;
                }
                case m_zoomAction:
                {
                    //m_playerController.m_playerCameraController.Zooming(_callbackContext.ReadValue<Vector2>());
                    break;
                }
                case m_mousePosAction:
                {
                    //m_playerController.m_playerCameraController.m_mousePosition = _callbackContext.ReadValue<Vector2>();
                    break;
                }
                case m_toggleMenuAction:
                    OpenMenu(_callbackContext);
                    break;
                case m_CursorVisAction:
                    break;
                default:
                    Debug.LogWarning($"Unknown Action: {_callbackContext.action.name}!");
                    break;
            }
        }

        //Process 'Move'-Action.
        private void HandleMove(InputAction.CallbackContext _callbackContext)
        {
            Vector2 moveInput = _callbackContext.ReadValue<Vector2>();
            m_playerController.m_playerMovement.SetInputVector(moveInput, m_playerController.m_playerId, false);
        }

        //Process 'Rotate'-Action.
        private void HandleRotate(InputAction.CallbackContext _callbackContext)
        {
            if (_callbackContext.phase == InputActionPhase.Performed || _callbackContext.phase == InputActionPhase.Started)
            {
                m_currentRotationInput = _callbackContext.ReadValue<Vector2>(); // Eingabe speichern
            }
            else if (_callbackContext.phase == InputActionPhase.Canceled)
            {
                m_currentRotationInput = Vector2.zero; // Eingabe zurücksetzen, wenn Taste losgelassen wird
            }
        }

        //Process 'Push'-Action.
        private void HandlePush(InputAction.CallbackContext _callbackContext)
        {
            if (_callbackContext.performed) // Button gedrückt
            {
                m_playerController.m_playerMovement.PushProgress(true);
            }
            else if (_callbackContext.canceled) // Button losgelassen
            {
                m_playerController.m_playerMovement.PushProgress(false);
            }
        }

        private void OpenMenu(InputAction.CallbackContext _callbackContext)
        {
            //if (m_playerController.m_playerId == 0)     //Equal to Master on Online Games?
            //TODO: All scripts that depend on Menu Opening need to subscribe here. 
            m_menuOpens?.Invoke();
            InputManager.ToggleActionMaps(InputManager.m_PlayerInputActions.UserInterface);
            //m_playerInputActions.Disable();  //Paddles can still be moved, if 'm_playerInputActions' here isn't disabled.
            //m_playerController.m_playerMovement.StopPushCoroutine();
        }
        #endregion
    }
}