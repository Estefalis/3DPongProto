using System;
using System.Collections.Generic;
using System.Linq;
using ThreeDeePongProto.Shared.HelperClasses;
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
        private UserInputManager m_userInputManager;
        private InputActionMap m_uiMap, m_playerMap;

        private const string m_moveString = "Move", m_rotateString = "Rotate", m_pushString = "Push", m_resetString = "ResetRotation", m_zoomString = "Zoom";
        private const string m_kickBallString = "KickBall", m_openGameMenuString = "OpenGameMenu", m_mousePositionString = "MousePosition";

        private const string m_keyboardMouseScheme = "KeyboardMouse", m_keyboardDevice = "Keyboard";
        private const string m_gamePadScheme = "Gamepad", m_gamepadDevice = "Gamepad";

        internal List<InputBinding> m_playerBindings = new();
        //private bool m_onQuitProcess = false;
        private int m_playerID;
        private uint m_playerUserID;
        private Vector2 m_rotationVector;   //Saved current rotationInput.

        #region Actions_and_Functions
        internal static event Action<int> AKickBall;
        internal static event Action<Vector2> ASendMousePosition;
        #endregion

        private void OnEnable()
        {
            if (m_playerController == null)
                m_playerController.GetComponentInParent<CharacterMainController>();

            if (m_playerInput != null && m_playerUserID != m_playerInput.user.id)
            {
                FilterInputBindings(m_playerInput, m_playerID);
                SubscribeToInputActions(m_playerInput);
            }
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

        private void Start()
        {
            m_userInputManager = FindObjectOfType<UserInputManager>();
            if (m_userInputManager == null)
            {
                Debug.LogError("UserInputManager not found!");
                return;
            }
        }

        private void FixedUpdate()
        {
            if (!IsLocalPlayer())
                return;
            //Use Rotation constantly.
            m_playerController.m_playerMovement.SetInputVector(m_rotationVector, m_playerID, true);
        }

        private void OnChangeActiveActionMap(string _actionMap)
        {
            if (_actionMap == EInputActionMaps.UserInterface.ToString())
            {
                m_playerMap.Disable();
                m_playerInput.SwitchCurrentActionMap(EInputActionMaps.UserInterface.ToString());
            }
            else if (_actionMap == EInputActionMaps.PlayerActions.ToString())
            {
                m_playerMap.Enable();
                m_playerInput.SwitchCurrentActionMap(EInputActionMaps.PlayerActions.ToString());
            }
        }

        private void PlayerInputCheck(uint _playerUserID)
        {
            //NOTE: 'm_currentPlayerInput._playerInputIndex' is here already != 'm_playerController.m_playerID'. Menu in PlayerInputManager took Debug-Index 0!

            if (m_playerInput == null)
                m_playerInput = m_playerController.GetComponent<PlayerInput>();

            if (_playerUserID != m_playerInput.user.id)
                return;

            m_playerUserID = m_playerInput.user.id;

            if (m_playerInput != null)
            {
                m_playerInput.GetComponent<PlayerInput>();
                m_playerMap = m_playerInput.actions.FindActionMap(EInputActionMaps.PlayerActions.ToString());
                m_uiMap = m_playerInput.actions.FindActionMap(EInputActionMaps.UserInterface.ToString());
                foreach (var actionMap in m_playerInput.actions.actionMaps)
                {
                    if (actionMap == m_playerMap)
                        actionMap.Enable();
                    else
                        actionMap.Disable();
                }
                FilterInputBindings(m_playerInput, m_playerID);  //Else m_playerInput.actions.
                SubscribeToInputActions(m_playerInput);
            }
        }

        #region Un-Subscribe_Methods
        private void SubscribeToInputActions(PlayerInput _playerInput)
        {
            UnsubscribeToInputActions(m_playerInput);

            UserInputManager.ACheckForPlayerInput += PlayerInputCheck;
            UserInputManager.AChangeActiveActionMap += OnChangeActiveActionMap;
            //m_playerInput.onControlsChanged += OnControlsChanged;
            InputSystem.onDeviceChange += OnDeviceChange;

            var moveAction = _playerInput.actions[m_moveString];
            moveAction.performed += _callbackContext => OnMove(_callbackContext, _playerInput);
            moveAction.canceled += _callbackContext => OnMoveCanceled(_callbackContext, _playerInput);

            var rotateAction = _playerInput.actions[m_rotateString];
            rotateAction.performed += _callbackContext => OnRotate(_callbackContext, _playerInput);
            rotateAction.canceled += _callbackContext => OnRotateCanceled(_callbackContext, _playerInput);

            var pushAction = _playerInput.actions[m_pushString];
            pushAction.performed += _callbackContext => OnPush(_callbackContext, _playerInput);

            var resetAction = _playerInput.actions[m_resetString];
            resetAction.performed += _callbackContext => OnRotationReset(_callbackContext, _playerInput);

            var kickBallAction = _playerInput.actions[m_kickBallString];
            kickBallAction.performed += _callbackContext => OnKickBall(_callbackContext, _playerInput);

            var zoomAction = _playerInput.actions[m_zoomString];
            zoomAction.performed += _callbackContext => OnZoom(_callbackContext, _playerInput);
            zoomAction.canceled += _callbackContext => OnZoomCanceled(_callbackContext, _playerInput);

            var mousePositionAction = _playerInput.actions[m_mousePositionString];
            mousePositionAction.performed += OnMousePosition;

            var openGameMenuAction = _playerInput.actions[m_openGameMenuString];
            openGameMenuAction.performed += OnOpenMenu;
        }

        private void UnsubscribeToInputActions(PlayerInput _playerInput)
        {
            if (_playerInput.user.id != m_playerInput.user.id)
                return;

            UserInputManager.ACheckForPlayerInput -= PlayerInputCheck;
            UserInputManager.AChangeActiveActionMap -= OnChangeActiveActionMap;
            //m_playerInput.onControlsChanged -= OnControlsChanged;
            InputSystem.onDeviceChange += OnDeviceChange;

            var moveAction = _playerInput.actions[m_moveString];
            moveAction.performed -= _callbackContext => OnMove(_callbackContext, _playerInput);
            moveAction.canceled -= _callbackContext => OnMoveCanceled(_callbackContext, _playerInput);

            var rotateAction = _playerInput.actions[m_rotateString];
            rotateAction.performed -= _callbackContext => OnRotate(_callbackContext, _playerInput);
            rotateAction.canceled -= _callbackContext => OnRotateCanceled(_callbackContext, _playerInput);

            var pushAction = _playerInput.actions[m_pushString];
            pushAction.performed -= _callbackContext => OnPush(_callbackContext, _playerInput);

            var resetAction = _playerInput.actions[m_resetString];
            resetAction.performed -= _callbackContext => OnRotationReset(_callbackContext, _playerInput);

            var kickBallAction = _playerInput.actions[m_kickBallString];
            kickBallAction.performed -= _callbackContext => OnKickBall(_callbackContext, _playerInput);

            var zoomAction = _playerInput.actions[m_zoomString];
            zoomAction.performed -= _callbackContext => OnZoom(_callbackContext, _playerInput);
            zoomAction.canceled -= _callbackContext => OnZoomCanceled(_callbackContext, _playerInput);

            var mousePositionAction = _playerInput.actions[m_mousePositionString];
            mousePositionAction.performed -= OnMousePosition;

            var openGameMenuAction = _playerInput.actions[m_openGameMenuString];
            openGameMenuAction.performed -= OnOpenMenu;
        }
        #endregion

        private void FilterInputBindings(PlayerInput _playerInput, int _playerID)
        {
            m_playerBindings.Clear();

            foreach (var action in _playerInput.actions)
            {
                if (action.actionMap.name != EInputActionMaps.PlayerActions.ToString())
                    continue;   //Skip all, except PlayerActions ActionMap.

                for (int i = 0; i < action.bindings.Count; i++)
                {
                    if (action.bindings[i].isComposite)
                        continue; //Skip (.is)Composites. (No controlScheme/groups available on parent.)

                    string controlScheme = action.bindings[i].groups;

                    if (StringManipulation.ContainsPlayerID(controlScheme, _playerID, 1, SearchDirection.FullScan))
                        m_playerBindings.Add(action.bindings[i]);
                }
            }

            //m_playerController.m_playerBindings.Sort((a, b) => a.id.CompareTo(b.id));
#if UNITY_EDITOR
            //Debug.Log($"PlayerIndex {_playerInput.playerIndex} Bindings updated: {m_playerController.m_playerBindings.Count}");
            //if (m_playerID == 0)
            //    foreach (var bindingIndex in m_playerController.m_playerBindings)
            //        Debug.Log($"Action: {bindingIndex.action}, Scheme: {bindingIndex.groups}, ePath: {bindingIndex.effectivePath}"); 
#endif
        }

        //        private void OnControlsChanged(PlayerInput _playerInput)
        //        {
        //            if (m_userInputManager == null)
        //            {
        //                if (!m_onQuitProcess)
        //                    return;
        //            }

        //            if (!m_userInputManager.GDevicesInitialized)    //g for getter in the future. (Yes?, No!, Maybe~.)
        //            {
        //#if UNITY_EDITOR
        //                Debug.LogWarning("Device map not initialized yet!");
        //#endif
        //                return;
        //            }
        //        }

        private void OnDeviceChange(InputDevice _inputDevice, InputDeviceChange _deviceChange)
        {
            switch (_deviceChange)
            {
                case InputDeviceChange.Added:
                case InputDeviceChange.Removed:
                case InputDeviceChange.ConfigurationChanged:
                {
#if UNITY_EDITOR
                    Debug.Log($"Device switched: {_inputDevice.displayName}");
#endif
                    FilterInputBindings(m_playerInput, m_playerID);
                }
                break;
            }
        }

        #region CallbackContext_Methods
        private void OnMove(InputAction.CallbackContext _callbackContext, PlayerInput _playerInput)
        {
            if (!IsLocalPlayer())
                return;

            string bindingControlScheme = _callbackContext.action.bindings[_callbackContext.action.GetBindingIndexForControl(_callbackContext.control)].groups;

            if (bindingControlScheme != _playerInput.currentControlScheme)
                return;

            //TODO: Fuer Gamepad ueberlegen, wie Dpad left/right indices zwischen GamepadID 0 - 3 unterscheiden können.
            Vector2 moveVector = _callbackContext.ReadValue<Vector2>();
            m_playerController.m_playerMovement.SetInputVector(moveVector, m_playerID, false);
        }

        private void OnMoveCanceled(InputAction.CallbackContext _callbackContext, PlayerInput _playerInput)
        {
            if (!IsLocalPlayer())
                return;

            if (_playerInput.devices.Contains(_callbackContext.control.device))
            {
                string bindingControlScheme = _callbackContext.action.bindings[_callbackContext.action.GetBindingIndexForControl(_callbackContext.control)].groups;

                if (bindingControlScheme == _playerInput.currentControlScheme)
                {
                    Vector2 moveVector = Vector2.zero;
                    m_playerController.m_playerMovement.SetInputVector(moveVector, m_playerID, false);
                }
            }
        }

        //Process 'Rotate'-Action.
        private void OnRotate(InputAction.CallbackContext _callbackContext, PlayerInput _playerInput)
        {
            if (!IsLocalPlayer())
                return;

            string bindingControlScheme = _callbackContext.action.bindings[_callbackContext.action.GetBindingIndexForControl(_callbackContext.control)].groups;

            if (bindingControlScheme == _playerInput.currentControlScheme)
                m_rotationVector = _callbackContext.ReadValue<Vector2>();
        }

        private void OnRotateCanceled(InputAction.CallbackContext _callbackContext, PlayerInput _playerInput)
        {
            if (!IsLocalPlayer())
                return;

            string bindingControlScheme = _callbackContext.action.bindings[_callbackContext.action.GetBindingIndexForControl(_callbackContext.control)].groups;

            if (bindingControlScheme == _playerInput.currentControlScheme)
                m_rotationVector = Vector2.zero; //Reset InputVector, once button is released/playerAction is canceled.
        }

        //Process 'Push'-Action.
        private void OnPush(InputAction.CallbackContext _callbackContext, PlayerInput _playerInput)
        {
            if (!IsLocalPlayer())
                return;

            string bindingControlScheme = _callbackContext.action.bindings[_callbackContext.action.GetBindingIndexForControl(_callbackContext.control)].groups;

            if (bindingControlScheme == _playerInput.currentControlScheme)
            {
                var initializePush = _callbackContext.ReadValueAsButton();
                if (initializePush)
                {
                    m_playerController.m_playerMovement.m_receivedUserID = _playerInput.user.id;
                    m_playerController.m_playerMovement.InitializePush(m_playerID, true);
                }
            }
        }

        private void OnRotationReset(InputAction.CallbackContext _callbackContext, PlayerInput _playerInput)
        {
            if (!IsLocalPlayer())
                return;

            string bindingControlScheme = _callbackContext.action.bindings[_callbackContext.action.GetBindingIndexForControl(_callbackContext.control)].groups;

            if (bindingControlScheme == _playerInput.currentControlScheme)
            {
                var resetRotation = _callbackContext.ReadValueAsButton();
                if (resetRotation)
                {
                    m_playerController.m_playerMovement.ResetPlayerRotation(m_playerID);
                }
            }
        }

        private void OnKickBall(InputAction.CallbackContext _callbackContext, PlayerInput _playerInput)
        {
            if (!IsLocalPlayer())
                return;

            string bindingControlScheme = _callbackContext.action.bindings[_callbackContext.action.GetBindingIndexForControl(_callbackContext.control)].groups;

            if (bindingControlScheme == _playerInput.currentControlScheme)
            {
                var kickBall = _callbackContext.ReadValueAsButton();
                if (kickBall)
                    AKickBall?.Invoke(m_playerID);  //Tell the Ball, that it has been kicked! *kick*
            }
        }

        private void OnZoom(InputAction.CallbackContext _callbackContext, PlayerInput _playerInput)
        {
            if (!IsLocalPlayer())
                return;

            string bindingControlScheme = _callbackContext.action.bindings[_callbackContext.action.GetBindingIndexForControl(_callbackContext.control)].groups;

            if (bindingControlScheme == _playerInput.currentControlScheme)
            {
                Vector2 zoomVector = _callbackContext.ReadValue<Vector2>();
                m_playerController.m_playerCameraController.Zooming(zoomVector, m_playerID, _playerInput.currentControlScheme);
            }
        }

        private void OnZoomCanceled(InputAction.CallbackContext _callbackContext, PlayerInput _playerInput)
        {
            if (!IsLocalPlayer())
                return;

            string bindingControlScheme = _callbackContext.action.bindings[_callbackContext.action.GetBindingIndexForControl(_callbackContext.control)].groups;

            if (bindingControlScheme == _playerInput.currentControlScheme)
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

            if (m_playerID == 0 && UserInputManager.SetActionMap == EInputActionMaps.PlayerActions.ToString())
            {                
                UserInputManager.ToggleActionMaps(EInputActionMaps.UserInterface.ToString());
            }
        }
        #endregion

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

        //private void OnApplicationQuit()
        //{
        //    //m_onQuitProcess = true;
        //}

        public void SetPlayerID(int _playerID)
        {
            m_playerID = _playerID;
        }
    }
}