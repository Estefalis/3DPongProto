using System;
using ThreeDeePongProto.Shared.InputActions;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ThreeDeePongProto.Shared.PlayerCharacter
{
    internal class CharacterInput : MonoBehaviour
    {
        private PlayerInput m_playerInput;
        //private static PlayerInputActions m_PlayerInputActions;
        [SerializeField] internal CharacterMainController m_playerController;

        private Vector2 m_currentRotationInput; //Save current rotationInput.
        private const string m_moveAction = "Move", m_rotateAction = "Rotate", m_pushAction = "Push";
        private const string m_kickAction = "Kick";
        private const string m_zoomAction = "Zoom", m_mousePosAction = "MousePosition";
        private const string m_toggleMenuAction = "ToggleGameMenu", m_CursorVisAction = "CursorVisibility";

        internal static event Action<int> m_KickBall;
        internal static event Action m_menuOpens;   //MenuManager and MatchManager subscribed to react on menu open/close.
        internal static event Action<Vector2> m_sendScrollVector;
        internal static event Action<Vector2> m_sendMousePosition;

        private void Awake()
        {
            //if (m_PlayerInputActions == null)
            //    m_PlayerInputActions = new PlayerInputActions();

            switch (m_playerController.GetComponent<PlayerInput>() == null)
            {
                case true:
                {
                    m_playerController.AddComponent<PlayerInput>();
                    //playerInput.actions = Resources.Load<InputActionAsset>(m_targetData); //Load Input Actions.
                    //playerInput.defaultControlScheme = "KeyboardMouse";
                    //playerInput.neverAutoSwitchControlSchemes = false; //Allow switching.
                    break;
                }
                case false:
                {
                    m_playerInput = m_playerController.GetComponent<PlayerInput>();

                    if (m_playerInput == null)
                    {
                        Debug.LogError("PlayerInput not found!");
                        return;
                    }

                    break;
                }
            }

            m_playerInput.onActionTriggered += OnActionTriggered;
        }

        private void OnDisable()
        {
            m_playerInput.onActionTriggered -= OnActionTriggered;
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
            //Return InputActionMap from a generated Wrapper.
            //var uiActionMap = RebindManager.m_PlayerInputActions.UserInterface.Get();
            //var playerActionsMap = RebindManager.m_PlayerInputActions.PlayerActions.Get();

            //if (_callbackContext.action.actionMap != playerActionsMap)
            //{
            //    Debug.Log($"Ignored action: {_callbackContext.action.name} (not part of PlayerActions)");
            //    return;
            //}

            //Debug.Log(playerActionsMap == _callbackContext.action.actionMap);

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
                        HandleKick(_callbackContext);
                    break;
                }
                case m_zoomAction:
                {
                    if (_callbackContext.action.WasPerformedThisFrame())
                        HandleZoom(_callbackContext);
                    break;
                }
                case m_mousePosAction:
                {
                    if (_callbackContext.action.WasPerformedThisFrame())
                        HandleMousePosition(_callbackContext);
                    break;
                }
                case m_toggleMenuAction:
                    if (_callbackContext.action.WasPerformedThisFrame())
                        OpenMenu();
                    break;
                case m_CursorVisAction:
                    break;
                default:
                    Debug.LogWarning($"Unhandled Action: {_callbackContext.action.name}.");
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
            switch (_callbackContext.phase)
            {
                case InputActionPhase.Started:
                case InputActionPhase.Performed:
                    m_currentRotationInput = _callbackContext.ReadValue<Vector2>(); //Tempsave InputVector.
                    break;
                case InputActionPhase.Canceled:
                    m_currentRotationInput = Vector2.zero; //Reset InputVector, once button is released/action is canceled.
                    break;
            }
        }

        //Process 'Push'-Action.
        private void HandlePush(InputAction.CallbackContext _callbackContext)
        {
            switch (_callbackContext.phase)
            {
                case InputActionPhase.Performed:    //If button is pressed.
                    m_playerController.m_playerMovement.PushProgress(true);
                    break;
                case InputActionPhase.Canceled:     //If button is released.
                    m_playerController.m_playerMovement.PushProgress(false);
                    break;
                case InputActionPhase.Started:
                default:
                    break;
            }
        }

        private void HandleKick(InputAction.CallbackContext _callbackContext)
        {
            m_KickBall?.Invoke(m_playerController.m_playerId);  //Tell the Ball, that it has been kicked! *kick*
        }

        private void HandleZoom(InputAction.CallbackContext _callbackContext)
        {
            Vector2 scrollVector = _callbackContext.ReadValue<Vector2>();
            m_sendScrollVector?.Invoke(scrollVector);
        }

        private void HandleMousePosition(InputAction.CallbackContext _callbackContext)
        {
            Vector2 mousePosition = _callbackContext.ReadValue<Vector2>();
            m_sendMousePosition?.Invoke(mousePosition);
        }

        private void OpenMenu()
        {
            //if (m_playerController.m_playerId == 0)     //Equal to Master on Online Games? 
            m_menuOpens?.Invoke();
            RebindManager.ToggleActionMaps(RebindManager.m_PlayerInputActions.UserInterface);
        }
        #endregion
    }
}