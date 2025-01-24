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

        private Vector2 m_moveVector;
        private Vector2 m_rotationVector;   //Save current rotationInput.
        private Vector2 m_zoomVector;
        private Vector2 m_mouseVector;

        internal static event Action<int> m_KickBall;
        internal static event Action m_menuOpens;   //MatchManager subscribed to react on menu open/close.
        internal static event Action<Vector2> m_sendScrollVector;
        internal static event Action<Vector2> m_sendMousePosition;


        private const string m_moveString = "Move", m_rotateString = "Rotate", m_pushString = "Push", m_zoomString = "Zoom";
        private const string m_kickBallString = "KickBall", m_toggleGameMenuString = "ToggleGameMenu", m_mousePositionString = "MousePosition";

        private const string m_playerActionsName = "PlayerActions";
        private const string m_userInterfaceName = "UserInterface";

        private void Awake()
        {
            //m_playerInput = GetComponent<PlayerInput>(); //If not set with '[SerializeField]'.

            if (m_playerInput == null)
            {
                Debug.LogWarning("PlayerInput component missing! Adding a new PlayerInput component...");
                m_playerInput = m_playerController.gameObject.AddComponent<PlayerInput>();

                // Optional: Konfiguriere die neu hinzugefügte PlayerInput-Komponente
                m_playerInput.actions = Resources.Load<InputActionAsset>("InputActions/PlayerInputActions");    //Load the InputActionAsset.
                m_playerInput.defaultControlScheme = "KeyboardMouse";   //Set desired ControlDevice.
                m_playerInput.neverAutoSwitchControlSchemes = false;    //Enable active ControlScheme switch.
                m_playerInput.notificationBehavior = PlayerNotifications.InvokeCSharpEvents;
            }

            SubscribeToActions(m_playerInput);
            InputManager.m_changeActiveActionMap += OnInputManagerChangedActionMap;
        }

        private void OnDisable()
        {
            if (m_playerInput != null)
                UnsubscribeToActions(m_playerInput);

            InputManager.m_changeActiveActionMap -= OnInputManagerChangedActionMap;
        }

        private void OnDestroy()
        {
            if (m_playerInput != null)
                UnsubscribeToActions(m_playerInput);

            InputManager.m_changeActiveActionMap -= OnInputManagerChangedActionMap;
        }

        private void FixedUpdate()
        {
            //Use Rotation constantly.
            if (m_rotationVector != Vector2.zero)
                m_playerController.m_playerMovement.SetInputVector(m_rotationVector, m_playerController.m_playerId, true);
        }

        private void SubscribeToActions(PlayerInput _playerInput)
        {
            //Subscribe on Actions.
            var moveAction = m_playerInput.actions[m_moveString];
            moveAction.performed += OnMove;
            moveAction.canceled += OnMoveCanceled;

            var rotateAction = m_playerInput.actions[m_rotateString];
            rotateAction.performed += OnRotate;
            rotateAction.canceled += OnRotateCanceled;

            var pushAction = m_playerInput.actions[m_pushString];
            pushAction.performed += OnPush;

            var zoomAction = m_playerInput.actions[m_zoomString];
            zoomAction.performed += OnZoom;
            zoomAction.canceled += OnZoomCanceled;

            var kickBallAction = m_playerInput.actions[m_kickBallString];
            kickBallAction.performed += OnKickBall;

            var mousePositionAction = m_playerInput.actions[m_mousePositionString];
            kickBallAction.performed += OnMousePosition;

            var toggleGameMenuAction = m_playerInput.actions[m_toggleGameMenuString];
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
            kickBallAction.performed -= OnMousePosition;

            var toggleGameMenuAction = m_playerInput.actions[m_toggleGameMenuString];
            toggleGameMenuAction.performed -= OnToggleMenu;
        }

        private void OnMove(InputAction.CallbackContext _callbackContext)
        {
            m_moveVector = _callbackContext.ReadValue<Vector2>();
            m_playerController.m_playerMovement.SetInputVector(m_moveVector, m_playerController.m_playerId, false);
        }

        private void OnMoveCanceled(InputAction.CallbackContext _callbackContext)
        {
            m_moveVector = Vector2.zero;
            m_playerController.m_playerMovement.SetInputVector(m_moveVector, m_playerController.m_playerId, false);
        }

        //Process 'Rotate'-Action.
        private void OnRotate(InputAction.CallbackContext _callbackContext)
        {
            m_rotationVector = _callbackContext.ReadValue<Vector2>(); //Tempsave InputVector.
            m_playerController.m_playerMovement.SetInputVector(m_rotationVector, m_playerController.m_playerId, true);
        }

        private void OnRotateCanceled(InputAction.CallbackContext _callbackContext)
        {
            m_rotationVector = Vector2.zero; //Reset InputVector, once button is released/action is canceled.
            m_playerController.m_playerMovement.SetInputVector(m_rotationVector, m_playerController.m_playerId, true);
        }

        //Process 'Push'-Action.
        private void OnPush(InputAction.CallbackContext _callbackContext)
        {
            m_playerController.m_playerMovement.InitializePush(true);
        }

        private void OnKickBall(InputAction.CallbackContext _callbackContext)
        {
            m_KickBall?.Invoke(m_playerController.m_playerId);  //Tell the Ball, that it has been kicked! *kick*
        }

        private void OnZoom(InputAction.CallbackContext _callbackContext)
        {
            m_zoomVector = _callbackContext.ReadValue<Vector2>();
            m_sendScrollVector?.Invoke(m_zoomVector);
        }

        private void OnZoomCanceled(InputAction.CallbackContext _callbackContext)
        {
            m_zoomVector = Vector2.zero;
        }

        private void OnMousePosition(InputAction.CallbackContext _callbackContext)
        {
            m_mouseVector = _callbackContext.ReadValue<Vector2>();
            m_sendMousePosition?.Invoke(m_mouseVector);
        }

        private void OnToggleMenu(InputAction.CallbackContext _callbackContext)
        {
            InputManager.ToggleActionMaps(InputManager.m_PlayerInputActions.UserInterface);
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