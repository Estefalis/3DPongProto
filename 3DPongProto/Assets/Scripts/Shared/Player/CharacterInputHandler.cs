using System;
using ThreeDeePongProto.Shared.Managers;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ThreeDeePongProto.Shared.PlayerCharacter
{
    internal class CharacterInputHandler : MonoBehaviour
    {
        [SerializeField] private PlayerInput m_playerInput;
        [SerializeField] internal CharacterMainController m_playerController;

        private Vector2 m_rotationVector;   //Saved current rotationInput.

        internal static event Action<int> AKickBall;
        internal static event Action AMenuOpens;   //LocalMatchManager subscribed to react on menu open/close.
        internal static event Action<Vector2> ASendMousePosition;

        private const string m_moveString = "Move", m_rotateString = "Rotate", m_pushString = "Push", m_zoomString = "Zoom";
        private const string m_kickBallString = "KickBall", m_toggleGameMenuString = "ToggleGameMenu", m_mousePositionString = "MousePosition";

        private void Awake()
        {
            UserInputManager.ACheckForPlayerInput += UserInputManagerSetPlayerInput;
            //PlayerInputCheck(m_playerInput.playerIndex);  //Used before invoking with 'UserInputManager.ACheckForPlayerInput'.
        }

        private void OnDisable()
        {
            if (m_playerInput != null)
                UnsubscribeToActions(m_playerInput, m_playerInput.playerIndex);

            UserInputManager.ACheckForPlayerInput -= UserInputManagerSetPlayerInput;
        }

        private void OnDestroy()
        {
            if (m_playerInput != null)
                UnsubscribeToActions(m_playerInput, m_playerInput.playerIndex);

            UserInputManager.ACheckForPlayerInput -= UserInputManagerSetPlayerInput;
        }

        private void FixedUpdate()
        {
            //Use Rotation constantly.
            if (m_rotationVector != Vector2.zero)
                m_playerController.m_playerMovement.SetInputVector(m_rotationVector, m_playerController.m_playerId, true);
        }

        private void UserInputManagerSetPlayerInput(int _index)
        {
            PlayerInputCheck(_index);   //With 'UserInputManager.ACheckForPlayerInput'.
        }

        private void PlayerInputCheck(int _playerIndex = 0)
        {
            if (m_playerController.m_playerId != _playerIndex)
                return;

            //If the component is not set in the 'Player Input' script slot.
            if (m_playerInput == null)
                m_playerInput = m_playerController.GetComponent<PlayerInput>();

            if (m_playerInput != null)
            {
                m_playerInput.GetComponent<PlayerInput>();
                SubscribeToActions(m_playerInput, m_playerInput.playerIndex);
            }
        }

        private void SubscribeToActions(PlayerInput _playerInput, int _playerID)
        {
            //Subscribe on Actions.
            switch (_playerID)
            {
                case 0:
                {
                    var moveAction = _playerInput.actions[m_moveString];
                    moveAction.performed += OnMove;
                    moveAction.canceled += OnMoveCanceled;
                    break;
                }
                case 1:
                    break;
                case 2:
                    break;
                case 3:
                    break;
                default:
                    break;
            }

            var rotateAction = _playerInput.actions[m_rotateString];
            rotateAction.performed += OnRotate;
            rotateAction.canceled += OnRotateCanceled;

            var pushAction = _playerInput.actions[m_pushString];
            pushAction.performed += OnPush;

            var kickBallAction = _playerInput.actions[m_kickBallString];
            kickBallAction.performed += OnKickBall;

            var zoomAction = _playerInput.actions[m_zoomString];
            zoomAction.performed += OnZoom;
            zoomAction.canceled += OnZoomCanceled;

            var mousePositionAction = _playerInput.actions[m_mousePositionString];
            mousePositionAction.performed += OnMousePosition;

            var toggleGameMenuAction = _playerInput.actions[m_toggleGameMenuString];
            toggleGameMenuAction.performed += OnOpenMenu;
        }

        private void UnsubscribeToActions(PlayerInput _playerInput, int _playerID)
        {
            //Unsubscribe on Actions.
            switch (_playerID)
            {
                case 0:
                {
                    var moveAction = _playerInput.actions[m_moveString];
                    moveAction.performed -= OnMove;
                    moveAction.canceled -= OnMoveCanceled;
                    break;
                }
                case 1:
                    break;
                case 2:
                    break;
                case 3:
                    break;
                default:
                    break;
            }

            var rotateAction = _playerInput.actions[m_rotateString];
            rotateAction.performed -= OnRotate;
            rotateAction.canceled -= OnRotateCanceled;

            var pushAction = _playerInput.actions[m_pushString];
            pushAction.performed -= OnPush;

            var kickBallAction = _playerInput.actions[m_kickBallString];
            kickBallAction.performed -= OnKickBall;

            var zoomAction = _playerInput.actions[m_zoomString];
            zoomAction.performed -= OnZoom;
            zoomAction.canceled -= OnZoomCanceled;

            var mousePositionAction = _playerInput.actions[m_mousePositionString];
            mousePositionAction.performed -= OnMousePosition;

            var toggleGameMenuAction = _playerInput.actions[m_toggleGameMenuString];
            toggleGameMenuAction.performed -= OnOpenMenu;
        }

        private void OnMove(InputAction.CallbackContext _callbackContext)
        {
            //if (m_playerInput.playerIndex != m_playerController.m_playerId)
            //    return;
            Vector2 moveVector = _callbackContext.ReadValue<Vector2>();
            Debug.Log($"Player {m_playerController.m_playerId} Move-Input: {moveVector} von {_callbackContext.control.device.name}");
            m_playerController.m_playerMovement.SetInputVector(moveVector, m_playerController.m_playerId, false);
        }

        private void OnMoveCanceled(InputAction.CallbackContext _callbackContext)
        {
            //if (m_playerInput.playerIndex != m_playerController.m_playerId)
            //    return;

            Vector2 moveVector = Vector2.zero;
            m_playerController.m_playerMovement.SetInputVector(moveVector, m_playerController.m_playerId, false);
        }

        //Process 'Rotate'-Action.
        private void OnRotate(InputAction.CallbackContext _callbackContext)
        {
            //if (m_playerInput.playerIndex != m_playerController.m_playerId)
            //return;

            m_rotationVector = _callbackContext.ReadValue<Vector2>();
        }

        private void OnRotateCanceled(InputAction.CallbackContext _callbackContext)
        {
            //if (m_playerInput.playerIndex != m_playerController.m_playerId)
            //    return;

            m_rotationVector = Vector2.zero; //Reset InputVector, once button is released/action is canceled.
        }

        //Process 'Push'-Action.
        private void OnPush(InputAction.CallbackContext _callbackContext)
        {
            //if (m_playerInput.playerIndex != m_playerController.m_playerId)
            //    return;

            var initializePush = _callbackContext.ReadValueAsButton();
            if (initializePush)
                m_playerController.m_playerMovement.InitializePush(true);    //Or m_playerController.m_playerId?
        }

        private void OnKickBall(InputAction.CallbackContext _callbackContext)
        {
            //if (m_playerInput.playerIndex != m_playerController.m_playerId)
            //    return;

            var kickBall = _callbackContext.ReadValueAsButton();
            if (kickBall)
                AKickBall?.Invoke(m_playerController.m_playerId);  //Tell the Ball, that it has been kicked! *kick*
        }

        private void OnZoom(InputAction.CallbackContext _callbackContext)
        {
            //if (m_playerInput.playerIndex != m_playerController.m_playerId)
            //    return;

            Vector2 zoomVector = _callbackContext.ReadValue<Vector2>();
            m_playerController.m_playerCameraController.Zooming(zoomVector);
        }

        private void OnZoomCanceled(InputAction.CallbackContext _callbackContext)
        {
            //if (m_playerInput.playerIndex != m_playerController.m_playerId)
            //    return;

            m_playerController.m_playerCameraController.Zooming(Vector2.zero);
        }

        private void OnMousePosition(InputAction.CallbackContext _callbackContext)
        {
            //if (m_playerInput.playerIndex != m_playerController.m_playerId)
            //    return;

            Vector2 mouseVector = _callbackContext.ReadValue<Vector2>();
            ASendMousePosition?.Invoke(mouseVector);
        }

        private void OnOpenMenu(InputAction.CallbackContext _callbackContext)
        {
            //if (m_playerInput.playerIndex != m_playerController.m_playerId)
            //    return;

            if (/*m_playerController.m_matchUIStates.EGameConnectModi == EGameConnectionModi.LocalPC && */m_playerController.m_playerId == 0)
                AMenuOpens?.Invoke();
        }
    }
}