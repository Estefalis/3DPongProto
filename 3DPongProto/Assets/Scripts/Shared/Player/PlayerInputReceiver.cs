using System;
using ThreeDeePongProto.Shared.InputActions;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ThreeDeePongProto.Shared.Player
{
    internal class PlayerInputReceiver : MonoBehaviour
    {
        private PlayerInputActions m_playerInputActions;
        [SerializeField] internal PlayerController m_playerController;

        private Vector2 m_currentRotationInput; //Save current rotationInput.
        private const string m_MoveAction = "Move", m_RotateAction = "Rotate", m_PushAction = "Push", m_Kick = "Kick";

        public static event Action<int> m_KickBall;

        private void Awake()
        {
            m_playerInputActions = new PlayerInputActions();
        }

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
        private void OnActionTriggered(InputAction.CallbackContext _context)
        {
            //Identify Actions by their Names.
            switch (_context.action.name)
            {
                case m_MoveAction:
                    HandleMove(_context);
                    break;
                case m_RotateAction:
                    HandleRotate(_context);
                    break;
                case m_PushAction:
                    HandlePush(_context);
                    break;
                case m_Kick:
                {
                    if (_context.action.WasPerformedThisFrame())    //Else started- & canceled-Phase invoke the event as well.
                        m_KickBall?.Invoke(m_playerController.m_playerId);
                    break;
                }
                default:
                    //Debug.LogWarning($"Unknown Action: {_context.action.name}!");
                    break;
            }
        }

        //Process 'Move'-Action.
        private void HandleMove(InputAction.CallbackContext _context)
        {
            Vector2 moveInput = _context.ReadValue<Vector2>();
            m_playerController.m_playerMovement.SetInputVector(moveInput, m_playerController.m_playerId, false);
        }

        //Process 'Rotate'-Action.
        private void HandleRotate(InputAction.CallbackContext _context)
        {
            if (_context.phase == InputActionPhase.Performed || _context.phase == InputActionPhase.Started)
            {
                m_currentRotationInput = _context.ReadValue<Vector2>(); // Eingabe speichern
            }
            else if (_context.phase == InputActionPhase.Canceled)
            {
                m_currentRotationInput = Vector2.zero; // Eingabe zurücksetzen, wenn Taste losgelassen wird
            }
        }

        //Process 'Push'-Action.
        private void HandlePush(InputAction.CallbackContext _context)
        {
            if (_context.performed) // Button gedrückt
            {
                m_playerController.m_playerMovement.PushProgress(true);
            }
            else if (_context.canceled) // Button losgelassen
            {
                m_playerController.m_playerMovement.PushProgress(false);
            }
        }
        #endregion
    }
}