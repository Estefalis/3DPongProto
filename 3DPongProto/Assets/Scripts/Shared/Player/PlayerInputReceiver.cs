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

        private Vector2 m_currentRotationInput; //Save currentRotationInput.

        //private Vector2 m_sideMoveVector;
        //private bool m_isPushing;

        private void Awake()
        {
            m_playerInputActions = new PlayerInputActions();
        }

        private void OnEnable()
        {
            var playerInput = m_playerController.GetComponent<PlayerInput>();
            playerInput.onActionTriggered += OnActionTriggered;
            //m_playerInputActions.Enable();
            //BindPlayerInputs();
        }

        private void OnDisable()
        {
            var playerInput = m_playerController.GetComponent<PlayerInput>();
            playerInput.onActionTriggered -= OnActionTriggered;
            //m_playerInputActions.Disable();
            //UnbindPlayerInputs();
        }

        private void Update()
        {
            //Vector2 rotationInput = m_playerInputActions.PlayerActions.Rotate.ReadValue<Vector2>();
            //if (rotationInput != Vector2.zero)
            //{
            //    //if (_playerId == m_playerController.m_playerId) // Ensure correct player
            //    //{
            //    m_playerController.m_playerMovement.SetInputVector(rotationInput, m_playerController.m_playerId, true);
            //}
            ////}
        }

        private void FixedUpdate()
        {
            // Rotation kontinuierlich anwenden
            if (m_currentRotationInput != Vector2.zero)
            {
                m_playerController.m_playerMovement.SetInputVector(m_currentRotationInput, m_playerController.m_playerId, true);
            }
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
        // Verarbeite alle Aktionen zentral
        private void OnActionTriggered(InputAction.CallbackContext _context)
        {
            // Identifiziere die Aktion anhand ihres Namens
            switch (_context.action.name)
            {
                case "Move":
                    HandleMove(_context);
                    break;

                case "Rotate":
                    HandleRotate(_context);
                    break;

                case "Push":
                    HandlePush(_context);
                    break;

                default:
                    Debug.LogWarning($"Unbekannte Aktion: {_context.action.name}");
                    break;
            }
        }

        // Verarbeitet die "Move"-Aktion
        private void HandleMove(InputAction.CallbackContext _context)
        {
            Vector2 moveInput = _context.ReadValue<Vector2>();
            m_playerController.m_playerMovement.SetInputVector(moveInput, m_playerController.m_playerId, false);
        }

        // Verarbeitet die "Rotate"-Aktion
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

        // Verarbeitet die "Push"-Aktion
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