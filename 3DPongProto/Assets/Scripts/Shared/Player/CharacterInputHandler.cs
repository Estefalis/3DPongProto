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

        private Vector2 m_rotationVector;   //Save current rotationInput.
        //private Vector2 m_moveVector, m_zoomVector, m_mouseVector;

        internal static event Action<int> m_KickBall;
        internal static event Action m_menuOpens;   //LocalMatchManager subscribed to react on menu open/close.
        //internal static event Action<Vector2> m_sendScrollVector;
        internal static event Action<Vector2> m_sendMousePosition;


        private const string m_moveString = "Move", m_rotateString = "Rotate", m_pushString = "Push", m_zoomString = "Zoom";
        private const string m_kickBallString = "KickBall", m_toggleGameMenuString = "ToggleGameMenu", m_mousePositionString = "MousePosition";

        private const string m_playerActionsName = "PlayerActions";
        private const string m_userInterfaceName = "UserInterface";

        private void Awake()
        {
            RebindManager.m_changeActiveActionMap += OnInputManagerChangedActionMap;
            PlayerInputCheck(m_playerInput.playerIndex);    //Or 'm_playerController.m_playerId'.
        }

        private void OnDisable()
        {
            if (m_playerInput != null)
                UnsubscribeToActions(m_playerInput);

            RebindManager.m_changeActiveActionMap -= OnInputManagerChangedActionMap;
        }

        private void OnDestroy()
        {
            if (m_playerInput != null)
                UnsubscribeToActions(m_playerInput);

            RebindManager.m_changeActiveActionMap -= OnInputManagerChangedActionMap;
        }

        private void FixedUpdate()
        {
            //Use Rotation constantly.
            if (m_rotationVector != Vector2.zero)
                m_playerController.m_playerMovement.SetInputVector(m_rotationVector, m_playerController.m_playerId, true);
        }

        private void PlayerInputCheck(int _playerIndex)
        {
            m_playerInput = m_playerController.GetComponent<PlayerInput>();
            if (m_playerInput != null && _playerIndex == m_playerController.m_playerId)
            {
                m_playerInput.GetComponent<PlayerInput>();
                SubscribeToActions(m_playerInput);
            }
        }

        private void SubscribeToActions(PlayerInput _playerInput)
        {
            //Subscribe on Actions.
            var moveAction = _playerInput.actions[m_moveString];
            moveAction.performed += OnMove;
            moveAction.canceled += OnMoveCanceled;

            var rotateAction = _playerInput.actions[m_rotateString];
            rotateAction.performed += OnRotate;
            rotateAction.canceled += OnRotateCanceled;

            var pushAction = _playerInput.actions[m_pushString];
            pushAction.performed += OnPush;

            var zoomAction = _playerInput.actions[m_zoomString];
            zoomAction.performed += OnZoom;
            zoomAction.canceled += OnZoomCanceled;

            var kickBallAction = _playerInput.actions[m_kickBallString];
            kickBallAction.performed += OnKickBall;

            var mousePositionAction = _playerInput.actions[m_mousePositionString];
            mousePositionAction.performed += OnMousePosition;

            var toggleGameMenuAction = _playerInput.actions[m_toggleGameMenuString];
            toggleGameMenuAction.performed += OnToggleMenu;
        }

        private void UnsubscribeToActions(PlayerInput _playerInput)
        {
            //Unsubscribe on Actions.
            var moveAction = m_playerInput.actions[m_moveString];
            moveAction.performed -= OnMove;
            moveAction.canceled -= OnMoveCanceled;

            var rotateAction = m_playerInput.actions[m_rotateString];
            rotateAction.performed -= OnRotate;
            rotateAction.canceled -= OnRotateCanceled;

            var pushAction = m_playerInput.actions[m_pushString];
            pushAction.performed -= OnPush;

            var zoomAction = m_playerInput.actions[m_zoomString];
            zoomAction.performed -= OnZoom;
            zoomAction.canceled -= OnZoomCanceled;

            var kickBallAction = m_playerInput.actions[m_kickBallString];
            kickBallAction.performed -= OnKickBall;

            var mousePositionAction = m_playerInput.actions[m_mousePositionString];
            mousePositionAction.performed -= OnMousePosition;

            var toggleGameMenuAction = m_playerInput.actions[m_toggleGameMenuString];
            toggleGameMenuAction.performed -= OnToggleMenu;
        }

        private void OnMove(InputAction.CallbackContext _callbackContext)
        {
            Vector2 moveVector = _callbackContext.ReadValue<Vector2>();
            m_playerController.m_playerMovement.SetInputVector(moveVector, m_playerController.m_playerId, false);
        }

        private void OnMoveCanceled(InputAction.CallbackContext _callbackContext)
        {
            Vector2 moveVector = Vector2.zero;
            m_playerController.m_playerMovement.SetInputVector(moveVector, m_playerController.m_playerId, false);
        }

        //Process 'Rotate'-Action.
        private void OnRotate(InputAction.CallbackContext _callbackContext)
        {
            m_rotationVector = _callbackContext.ReadValue<Vector2>(); //Tempsave InputVector.
        }

        private void OnRotateCanceled(InputAction.CallbackContext _callbackContext)
        {
            m_rotationVector = Vector2.zero; //Reset InputVector, once button is released/action is canceled.
        }

        //Process 'Push'-Action.
        private void OnPush(InputAction.CallbackContext _callbackContext)
        {
            var initializePush = _callbackContext.ReadValueAsButton();
            if (initializePush)
                m_playerController.m_playerMovement.InitializePush(true);
        }

        private void OnKickBall(InputAction.CallbackContext _callbackContext)
        {
            var kickBall = _callbackContext.ReadValueAsButton();
            if (kickBall)
                m_KickBall?.Invoke(m_playerController.m_playerId);  //Tell the Ball, that it has been kicked! *kick*
        }

        private void OnZoom(InputAction.CallbackContext _callbackContext)
        {
            Vector2 zoomVector = _callbackContext.ReadValue<Vector2>();
            //m_sendScrollVector?.Invoke(zoomVector);
            m_playerController.m_playerCameraController.Zooming(zoomVector);
        }

        private void OnZoomCanceled(InputAction.CallbackContext _callbackContext)
        {
            //m_zoomVector = Vector2.zero;
            m_playerController.m_playerCameraController.Zooming(Vector2.zero);
        }

        private void OnMousePosition(InputAction.CallbackContext _callbackContext)
        {
            Vector2 mouseVector = _callbackContext.ReadValue<Vector2>();
            m_sendMousePosition?.Invoke(mouseVector);
        }

        private void OnToggleMenu(InputAction.CallbackContext _callbackContext)
        {
            RebindManager.ToggleActionMaps(RebindManager.m_PlayerInputActions.UserInterface);
        }

        private void OnInputManagerChangedActionMap(InputActionMap _inputActionMap)
        {
            #region Before PlayerInput-Component
            //switch (_inputActionMap.name == m_playerActionsName)
            //{
            //    case true: m_playerInputActions.PlayerActions.Enable();
            //        break;
            //    case false:
            //        m_playerInputActions.PlayerActions.Disable();
            //        break;
            //}

            //if (!m_playerInputActions.PlayerActions.enabled)
            //    m_menuOpens?.Invoke(); 
            #endregion

            // Change to current ActionMap by PlayerInput.
            if (m_playerController.m_matchUIStates.EGameConnectModi == EGameModi.LocalPC && m_playerController.m_playerId == 0)
            {
                m_playerInput.SwitchCurrentActionMap(_inputActionMap.name);

                switch (_inputActionMap.name)
                {
                    case m_userInterfaceName:
                    {
                        m_menuOpens?.Invoke();
                        Debug.Log("Menu opened!");
                        break;
                    }
                    case m_playerActionsName:
                    {
                        Debug.Log("Returned to PlayerActions.");
                        break;
                    }
                    default:
                        break;
                }
            }
        }
    }
}