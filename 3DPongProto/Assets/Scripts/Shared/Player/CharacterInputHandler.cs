using System;
using ThreeDeePongProto.Shared.Managers;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

namespace ThreeDeePongProto.Shared.PlayerCharacter
{
    internal class CharacterInputHandler : MonoBehaviour
    {
        [SerializeField] private PlayerInput m_playerInput;
        [SerializeField] internal CharacterMainController m_playerController;
        private UserInputManager m_userInputManager;

        private const string m_moveString = "Move", m_rotateString = "Rotate", m_pushString = "Push", m_zoomString = "Zoom";
        private const string m_kickBallString = "KickBall", m_toggleGameMenuString = "ToggleGameMenu", m_mousePositionString = "MousePosition";
        private const string m_uiActionMap = "UserInterface", m_playerActionMap = "PlayerActions";

        private const string m_keyboardMouse = "KeyboardMouse", m_keyboardSchemePID0 = "KeyboardPlayerID0", m_keyboardSchemePID1 = "KeyboardPlayerID1", m_keyboardSchemePID2 = "KeyboardPlayerID2", m_keyboardSchemePID3 = "KeyboardPlayerID3", m_keyboardDevice = "Keyboard";
        private const string m_gamePadScheme = "Gamepad", m_gamePadSchemePID0 = "GamepadPlayerID0", m_gamePadSchemePID1 = "GamepadPlayerID1", m_gamePadSchemePID2 = "GamepadPlayerID2", m_gamePadSchemePID3 = "GamepadPlayerID3", m_gamepadDevice = "Gamepad";

        private Vector2 m_rotationVector;   //Saved current rotationInput.

        internal static event Action<int> AKickBall;
        internal static event Action<string> AOpenMenu;   //LocalMatchManager subscribed to react on menu open/close.
        internal static event Action<Vector2> ASendMousePosition;

        private void Awake()
        {
            UserInputManager.AChangeActiveActionMap += OnChangeActiveActionMap;
            UserInputManager.ACheckForPlayerInput += PlayerInputCheck;
        }

        private void OnDisable()
        {
            if (m_playerInput != null)
                UnsubscribeToInputActions(m_playerInput, m_playerInput.user.id);
        }

        private void OnDestroy()
        {
            if (m_playerInput != null)
                UnsubscribeToInputActions(m_playerInput, m_playerInput.user.id);
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
            //Use Rotation constantly.
            if (m_rotationVector != Vector2.zero)
                m_playerController.m_playerMovement.SetInputVector(m_rotationVector, m_playerController.m_playerId, true);
        }

        private void OnChangeActiveActionMap(string _actionMap)
        {
            m_playerInput.SwitchCurrentActionMap(_actionMap);
        }

        private void PlayerInputCheck(uint _playerUserID)
        {
            //NOTE: 'm_playerInput.playerIndex' is here already != 'm_playerController.m_playerId'. Menu in PlayerInputManager took Debug-Index 0!
            //If the component is not set in the 'Player Input' script slot. (With 'UserInputManager.ACheckForPlayerInput'.)
            if (m_playerInput == null)
                m_playerInput = m_playerController.GetComponent<PlayerInput>();

            if (_playerUserID != m_playerInput.user.id)
                return;

            if (m_playerInput != null)
            {
                m_playerInput.GetComponent<PlayerInput>();
                SubscribeToInputActions(m_playerInput);

                //Listen to controlScheme changes.
                m_playerInput.onControlsChanged += OnControlsChanged;
            }
        }

        private void SubscribeToInputActions(PlayerInput _playerInput)
        {
            //foreach(InputActionMap inputActionMap in _playerInput.actions.actionMaps)
            //{
            //    for (int i = 0; i < inputActionMap.bindings.Count; i++)
            //    {
            //        if (inputActionMap.bindings[i].groups == m_keyboardMouse && !inputActionMap.bindings[i].isComposite)
            //        {
            //            Debug.Log(inputActionMap.bindings[i].name);
            //        }
            //    }
            //}

            var moveAction = _playerInput.actions[m_moveString];
            moveAction.performed += ctx => OnMove(ctx, _playerInput);
            moveAction.canceled += ctx => OnMoveCanceled(ctx, _playerInput);

            var rotateAction = _playerInput.actions[m_rotateString];
            rotateAction.performed += ctx => OnRotate(ctx, _playerInput);
            rotateAction.canceled += ctx => OnRotateCanceled(ctx, _playerInput);

            var pushAction = _playerInput.actions[m_pushString];
            pushAction.performed += ctx => OnPush(ctx, _playerInput);

            var kickBallAction = _playerInput.actions[m_kickBallString];
            kickBallAction.performed += ctx => OnKickBall(ctx, _playerInput);

            var zoomAction = _playerInput.actions[m_zoomString];
            zoomAction.performed += ctx => OnZoom(ctx, _playerInput);
            zoomAction.canceled += ctx => OnZoomCanceled(ctx, _playerInput);

            var mousePositionAction = _playerInput.actions[m_mousePositionString];
            mousePositionAction.performed += OnMousePosition;

            var toggleGameMenuAction = _playerInput.actions[m_toggleGameMenuString];
            toggleGameMenuAction.performed += OnOpenMenu;
        }

        private void UnsubscribeToInputActions(PlayerInput _playerInput, uint _playerID)
        {
            if (_playerInput.user.id != _playerID)
                return;

            UserInputManager.ACheckForPlayerInput -= PlayerInputCheck;
            UserInputManager.AChangeActiveActionMap -= OnChangeActiveActionMap;
            m_playerInput.onControlsChanged -= OnControlsChanged;

            var moveAction = _playerInput.actions[m_moveString];
            moveAction.performed -= ctx => OnMove(ctx, _playerInput);
            moveAction.canceled -= ctx => OnMoveCanceled(ctx, _playerInput);

            var rotateAction = _playerInput.actions[m_rotateString];
            rotateAction.performed -= ctx => OnRotate(ctx, _playerInput);
            rotateAction.canceled -= ctx => OnRotateCanceled(ctx, _playerInput);

            var pushAction = _playerInput.actions[m_pushString];
            pushAction.performed -= ctx => OnPush(ctx, _playerInput);

            var kickBallAction = _playerInput.actions[m_kickBallString];
            kickBallAction.performed -= ctx => OnKickBall(ctx, _playerInput);

            var zoomAction = _playerInput.actions[m_zoomString];
            zoomAction.performed -= ctx => OnZoom(ctx, _playerInput);
            zoomAction.canceled -= ctx => OnZoomCanceled(ctx, _playerInput);

            var mousePositionAction = _playerInput.actions[m_mousePositionString];
            mousePositionAction.performed -= OnMousePosition;

            var toggleGameMenuAction = _playerInput.actions[m_toggleGameMenuString];
            toggleGameMenuAction.performed -= OnOpenMenu;
        }

        private void OnControlsChanged(PlayerInput _playerInput)
        {
            if (m_userInputManager == null)
            {
                Debug.LogError("UserInputManager not found!");
                return;
            }

            var deviceMap = m_userInputManager.GetPlayerDeviceMap();

            if (!m_userInputManager.gDevicesInitialized)    //g for getter in the future. (Yes?, No!, Maybe~.)
            {
                Debug.LogWarning("Device map not initialized yet!");
                return;
            }

            InputDevice newDevice = _playerInput.devices.Count > 0 ? _playerInput.devices[0] : null;

            if (newDevice == null)
            {
                Debug.LogWarning($"No active device found for PlayerIndex {_playerInput.playerIndex} | PlayerUserID: {_playerInput.user.id}.");
                return;
            }

            if (deviceMap.TryGetValue(_playerInput.user.id, out var currentDevice))
            {
                if (newDevice != currentDevice)
                {
                    deviceMap[_playerInput.user.id] = newDevice;
                    InputUser.PerformPairingWithDevice(newDevice, _playerInput.user);
                    Debug.Log($"Updated device for PlayerIndex {_playerInput.playerIndex} with PlayerUserID {_playerInput.user.id} to {newDevice.name}.");
                }
                else
                {
                    Debug.Log($"Device for PlayerIndex {_playerInput.playerIndex} with PlayerUserID {_playerInput.user.id} unchanged ({newDevice.name}).");
                }
            }
            else
            {
                deviceMap.Add(_playerInput.user.id, newDevice);
                InputUser.PerformPairingWithDevice(newDevice, _playerInput.user);
                Debug.Log($"Added PlayerIndex {_playerInput.playerIndex} with PlayerUserID {_playerInput.user.id} and device {newDevice.name} to device map.");
            }
        }

        private void OnMove(InputAction.CallbackContext _callbackContext, PlayerInput _playerInput)
        {
            Vector2 moveVector = _callbackContext.ReadValue<Vector2>();
            m_playerController.m_playerMovement.SetInputVector(moveVector, m_playerController.m_playerId, false);
        }

        private void OnMoveCanceled(InputAction.CallbackContext _callbackContext, PlayerInput _playerInput)
        {
            Vector2 moveVector = Vector2.zero;
            m_playerController.m_playerMovement.SetInputVector(moveVector, m_playerController.m_playerId, false);
        }

        //Process 'Rotate'-Action.
        private void OnRotate(InputAction.CallbackContext _callbackContext, PlayerInput _playerInput)
        {
            m_rotationVector = _callbackContext.ReadValue<Vector2>();
        }

        private void OnRotateCanceled(InputAction.CallbackContext _callbackContext, PlayerInput _playerInput)
        {
            m_rotationVector = Vector2.zero; //Reset InputVector, once button is released/action is canceled.
        }

        //Process 'Push'-Action.
        private void OnPush(InputAction.CallbackContext _callbackContext, PlayerInput _playerInput)
        {
            var initializePush = _callbackContext.ReadValueAsButton();
            if (initializePush)
                m_playerController.m_playerMovement.InitializePush(true);
        }

        private void OnKickBall(InputAction.CallbackContext _callbackContext, PlayerInput _playerInput)
        {
            var kickBall = _callbackContext.ReadValueAsButton();
            if (kickBall)
                AKickBall?.Invoke(m_playerController.m_playerId);  //Tell the Ball, that it has been kicked! *kick*
        }

        private void OnZoom(InputAction.CallbackContext _callbackContext, PlayerInput _playerInput)
        {
            Vector2 zoomVector = _callbackContext.ReadValue<Vector2>();
            m_playerController.m_playerCameraController.Zooming(zoomVector, m_playerController.m_playerId, _playerInput.currentControlScheme);
        }

        private void OnZoomCanceled(InputAction.CallbackContext _callbackContext, PlayerInput _playerInput)
        {
            m_playerController.m_playerCameraController.Zooming(Vector2.zero, m_playerController.m_playerId, "");
        }

        private void OnMousePosition(InputAction.CallbackContext _callbackContext)
        {
            Vector2 mouseVector = _callbackContext.ReadValue<Vector2>();
            ASendMousePosition?.Invoke(mouseVector);
        }

        private void OnOpenMenu(InputAction.CallbackContext _callbackContext)
        {
            UserInputManager.ToggleActionMaps(m_uiActionMap);

            if (m_playerController.m_playerId == 0)
                AOpenMenu?.Invoke(m_uiActionMap);
        }
    }
}