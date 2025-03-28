using System;
using System.Collections.Generic;
using System.Linq;
using ThreeDeePongProto.Shared.HelperClasses;
using ThreeDeePongProto.Shared.InputActions;
using ThreeDeePongProto.Shared.Managers;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

namespace ThreeDeePongProto.Shared.PlayerCharacter
{
    internal class CharacterInputHandler : MonoBehaviour
    {
        [SerializeField] internal PlayerInput m_playerInput;
        [SerializeField] internal CharacterMainController m_playerController;
        private UserInputManager m_userInputManager;

        private const string m_moveString = "Move", m_rotateString = "Rotate", m_pushString = "Push", m_resetString = "ResetRotation", m_zoomString = "Zoom";
        private const string m_kickBallString = "KickBall", m_openGameMenuString = "OpenGameMenu", m_mousePositionString = "MousePosition";
        private const string m_uiActionMap = "UserInterface", m_playerActionMap = "PlayerActions";

        private const string m_keyboardMouseScheme = "KeyboardMouse", m_keyboardSchemePID0 = "KeyboardPlayerID0", m_keyboardSchemePID1 = "KeyboardPlayerID1", m_keyboardSchemePID2 = "KeyboardPlayerID2", m_keyboardSchemePID3 = "KeyboardPlayerID3", m_keyboardDevice = "Keyboard";
        private const string m_gamePadScheme = "Gamepad", m_gamePadSchemePID0 = "GamepadPlayerID0", m_gamePadSchemePID1 = "GamepadPlayerID1", m_gamePadSchemePID2 = "GamepadPlayerID2", m_gamePadSchemePID3 = "GamepadPlayerID3", m_gamepadDevice = "Gamepad";

        private Vector2 m_rotationVector;   //Saved current rotationInput.
        private int m_playerIndex;
        private bool m_onQuitProcess = false;

        private Dictionary<InputDevice, List<InputBinding>> m_inputBindings;
        private Dictionary<InputDevice, Dictionary<int, List<InputBinding>>> m_combinedRouteDict = new();

        #region Actions_and_Functions
        internal static event Action<int> AKickBall;
        internal static event Action<Vector2> ASendMousePosition;
        #endregion

        private void Awake()
        {
            m_playerIndex = -1;
            UserInputManager.AChangeActiveActionMap += OnChangeActiveActionMap;
            UserInputManager.ACheckForPlayerInput += PlayerInputCheck;
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
            //Use Rotation constantly.
            m_playerController.m_playerMovement.SetInputVector(m_rotationVector, m_playerController.m_playerId, true);
        }

        private void OnChangeActiveActionMap(string _actionMap)
        {
            switch (_actionMap)
            {
                case m_uiActionMap:
                {
                    m_playerInput.SwitchCurrentActionMap(m_uiActionMap);
                    break;
                }
                case m_playerActionMap:
                {
                    m_playerInput.SwitchCurrentActionMap(m_playerActionMap);
                    break;
                }
                default:
                    break;
            }
        }

        private void PlayerInputCheck(int _playerIndex)
        {
            //NOTE: 'm_currentPlayerInput._playerIndex' is here already != 'm_playerController.m_playerId'. Menu in PlayerInputManager took Debug-Index 0!

            if (m_playerInput == null)
                m_playerInput = m_playerController.GetComponent<PlayerInput>();

            if (_playerIndex != m_playerInput.playerIndex)
                return;

            m_playerIndex = _playerIndex;

            if (m_playerInput != null)
            {
                m_playerInput.GetComponent<PlayerInput>();
                SubscribeToInputActions(m_playerInput);
                OrganizeInputBindings(m_playerInput.actions);   //TODO: Complete OrganizeInputBindings and Contextual PlayerMovement.
                //Listen to controlScheme changes.
                m_playerInput.onControlsChanged += OnControlsChanged;
            }
        }

        #region Un-Subscribe_Methods
        private void SubscribeToInputActions(PlayerInput _playerInput)
        {
            var moveAction = _playerInput.actions[m_moveString];
            moveAction.performed += ctx => OnMove(ctx, _playerInput);
            moveAction.canceled += ctx => OnMoveCanceled(ctx, _playerInput);

            var rotateAction = _playerInput.actions[m_rotateString];
            rotateAction.performed += ctx => OnRotate(ctx, _playerInput);
            rotateAction.canceled += ctx => OnRotateCanceled(ctx, _playerInput);

            var pushAction = _playerInput.actions[m_pushString];
            pushAction.performed += ctx => OnPush(ctx, _playerInput);

            var resetAction = _playerInput.actions[m_resetString];
            resetAction.performed += ctx => OnRotationReset(ctx, _playerInput);

            var kickBallAction = _playerInput.actions[m_kickBallString];
            kickBallAction.performed += ctx => OnKickBall(ctx, _playerInput);

            var zoomAction = _playerInput.actions[m_zoomString];
            zoomAction.performed += ctx => OnZoom(ctx, _playerInput);
            zoomAction.canceled += ctx => OnZoomCanceled(ctx, _playerInput);

            var mousePositionAction = _playerInput.actions[m_mousePositionString];
            mousePositionAction.performed += OnMousePosition;

            var openGameMenuAction = _playerInput.actions[m_openGameMenuString];
            openGameMenuAction.performed += OnOpenMenu;
        }

        private void UnsubscribeToInputActions(PlayerInput _playerInput)
        {
            if (_playerInput.playerIndex != m_playerIndex)
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

            var resetAction = _playerInput.actions[m_resetString];
            resetAction.performed -= ctx => OnRotationReset(ctx, _playerInput);

            var kickBallAction = _playerInput.actions[m_kickBallString];
            kickBallAction.performed -= ctx => OnKickBall(ctx, _playerInput);

            var zoomAction = _playerInput.actions[m_zoomString];
            zoomAction.performed -= ctx => OnZoom(ctx, _playerInput);
            zoomAction.canceled -= ctx => OnZoomCanceled(ctx, _playerInput);

            var mousePositionAction = _playerInput.actions[m_mousePositionString];
            mousePositionAction.performed -= OnMousePosition;

            var openGameMenuAction = _playerInput.actions[m_openGameMenuString];
            openGameMenuAction.performed -= OnOpenMenu;
        }
        #endregion

        private void OrganizeInputBindings(InputActionAsset _inputActionAsset)
        {
            m_combinedRouteDict.Clear();

            foreach (var actionMap in _inputActionAsset.actionMaps)
            {
                if (actionMap.name != m_playerActionMap)
                    continue;

                foreach (var playerAction in actionMap.actions)
                {
                    bool isComposite = false;
                    //Debug.Log($"ActionMap: {actionMap.name}, Action: {playerAction.name}, Bindings: {playerAction.bindings.Count}");

                    for (int i = 0; i < playerAction.bindings.Count; i++)
                    {
                        if (playerAction.bindings[i].isComposite)
                        {
                            //Debug.Log($"Composite: {playerAction.bindings[i].name}");
                            isComposite = true;
                            continue;
                        }

                        int playerIndex = i / 2;

                        if (isComposite)
                        {
                            if (playerAction.bindings[i].isPartOfComposite && i >= playerIndex * 2 && i < playerIndex * 2 + 2)
                            {
                                //Debug.Log($"Action: {playerAction.name}, Composite: {playerAction.bindings[i].name}, {playerAction.bindings[i].groups}");
                                CollectBindingForPlayer(playerIndex, playerAction.bindings[i]);
                            }
                        }
                        else
                        {
                            //Debug.Log($"Action: {playerAction.name}, Composite: {playerAction.bindings[i].name}, {playerAction.bindings[i].groups}");
                            CollectBindingForPlayer(playerIndex, playerAction.bindings[i]);
                        }
                    }
                }
            }
        }

        private void CollectBindingForPlayer(int _playerIndex, InputBinding _inputBinding)
        {
            //InputDevice inputDevice = _inputBinding.effectivePath != null ? InputSystem.GetDevice(_inputBinding.effectivePath) : null;
            string deviceType = StringManipulation.GetCharacterBetweenArgs($"{_inputBinding.effectivePath}", "<", ">");
            //Debug.Log($"Device: {deviceType}");
            InputDevice inputDevice = InputSystem.devices.FirstOrDefault(d => d.description.ToString().Contains(deviceType));

            if (inputDevice == null)
            {
                //Debug.Log($"Binding: {_inputBinding.effectivePath}");
                return;
            }

            if (!m_combinedRouteDict.ContainsKey(inputDevice))
            {
                m_combinedRouteDict[inputDevice] = new Dictionary<int, List<InputBinding>>();
            }

            if (!m_combinedRouteDict[inputDevice].ContainsKey(_playerIndex))
            {
                m_combinedRouteDict[inputDevice][_playerIndex] = new List<InputBinding>();
            }

            m_combinedRouteDict[inputDevice][_playerIndex].Add(_inputBinding);
            //Debug.Log($"Binding of Device: {inputDevice.name}, PlayerIndex: {_playerIndex}, Path: {_inputBinding.path}");
        }

        private void OnControlsChanged(PlayerInput _playerInput)
        {
            if (m_userInputManager == null)
            {
                if (!m_onQuitProcess)
                    return;
            }

            if (!m_userInputManager.GDevicesInitialized)    //g for getter in the future. (Yes?, No!, Maybe~.)
            {
                Debug.LogWarning("Device map not initialized yet!");
                return;
            }

            InputDevice newDevice = _playerInput.devices.Count > 0 ? _playerInput.devices[0] : null;
            if (newDevice == null)
                return;

            var playerId = _playerInput.user.id;
            var deviceMap = m_userInputManager.GetPlayerDeviceMap();

            //If the same inputDevice is already assigned, ignore.
            if (deviceMap.TryGetValue(playerId, out var currentDevice) && newDevice == currentDevice)
            {
                //Debug.Log($"OnControlsChanged: Device for PlayerIndex {_playerInput._playerIndex} | PlayerUserID {playerId} unchanged ({newDevice.name}).");
                return;
            }

            //Update inputDevice map & pair new inputDevice.
            deviceMap[playerId] = newDevice;
            _playerInput.user.UnpairDevices();
            InputUser.PerformPairingWithDevice(newDevice, _playerInput.user);

            //Get correct control scheme.
            GetDeviceHelper.GetPlayerControlScheme(_playerInput.playerIndex, newDevice, out string newScheme, out _);
            Debug.Log($"OnControlsChanged-Device(s): {string.Join(", ", _playerInput.devices.Select(d => d.name))} | Index: {m_playerInput.playerIndex} | UserID: {_playerInput.user.id}.");

            if (_playerInput.currentControlScheme == newScheme)
            {
                Debug.Log($"Player {_playerInput.playerIndex} already using correct scheme: {newScheme}");
                return;
            }

            _playerInput.SwitchCurrentControlScheme(newScheme, newDevice);
            Debug.Log($"Player {_playerInput.playerIndex} now using {newDevice.name} with scheme {newScheme}.");
        }

        #region CallbackContext_Methods
        private void OnMove(InputAction.CallbackContext _callbackContext, PlayerInput _playerInput)
        {
            string bindingControlScheme = _callbackContext.action.bindings[_callbackContext.action.GetBindingIndexForControl(_callbackContext.control)].groups;

            if (bindingControlScheme != _playerInput.currentControlScheme)
                return;

            //TODO: Fuer Gamepad ueberlegen, wie Dpad left/right indices zwischen GamepadID 0 - 3 unterscheiden können.
            Vector2 moveVector = _callbackContext.ReadValue<Vector2>();
            m_playerController.m_playerMovement.SetInputVector(moveVector, m_playerController.m_playerId, false);
        }

        private void OnMoveCanceled(InputAction.CallbackContext _callbackContext, PlayerInput _playerInput)
        {
            if (_playerInput.devices.Contains(_callbackContext.control.device))
            {
                string bindingControlScheme = _callbackContext.action.bindings[_callbackContext.action.GetBindingIndexForControl(_callbackContext.control)].groups;

                if (bindingControlScheme == _playerInput.currentControlScheme)
                {
                    Vector2 moveVector = Vector2.zero;
                    m_playerController.m_playerMovement.SetInputVector(moveVector, m_playerController.m_playerId, false);
                }
            }
        }

        //Process 'Rotate'-Action.
        private void OnRotate(InputAction.CallbackContext _callbackContext, PlayerInput _playerInput)
        {
            string bindingControlScheme = _callbackContext.action.bindings[_callbackContext.action.GetBindingIndexForControl(_callbackContext.control)].groups;

            if (bindingControlScheme == _playerInput.currentControlScheme)
                m_rotationVector = _callbackContext.ReadValue<Vector2>();
        }

        private void OnRotateCanceled(InputAction.CallbackContext _callbackContext, PlayerInput _playerInput)
        {
            string bindingControlScheme = _callbackContext.action.bindings[_callbackContext.action.GetBindingIndexForControl(_callbackContext.control)].groups;

            if (bindingControlScheme == _playerInput.currentControlScheme)
                m_rotationVector = Vector2.zero; //Reset InputVector, once button is released/playerAction is canceled.
        }

        //Process 'Push'-Action.
        private void OnPush(InputAction.CallbackContext _callbackContext, PlayerInput _playerInput)
        {
            string bindingControlScheme = _callbackContext.action.bindings[_callbackContext.action.GetBindingIndexForControl(_callbackContext.control)].groups;

            if (bindingControlScheme == _playerInput.currentControlScheme)
            {
                var initializePush = _callbackContext.ReadValueAsButton();
                if (initializePush)
                {
                    m_playerController.m_playerMovement.m_receivedUserID = _playerInput.user.id;
                    m_playerController.m_playerMovement.InitializePush(m_playerController.m_playerId, true);
                }
            }
        }

        private void OnRotationReset(InputAction.CallbackContext _callbackContext, PlayerInput _playerInput)
        {
            string bindingControlScheme = _callbackContext.action.bindings[_callbackContext.action.GetBindingIndexForControl(_callbackContext.control)].groups;

            if (bindingControlScheme == _playerInput.currentControlScheme)
            {
                var resetRotation = _callbackContext.ReadValueAsButton();
                if (resetRotation)
                {
                    m_playerController.m_playerMovement.ResetPlayerRotation(m_playerController.m_playerId);
                }
            }
        }

        private void OnKickBall(InputAction.CallbackContext _callbackContext, PlayerInput _playerInput)
        {
            string bindingControlScheme = _callbackContext.action.bindings[_callbackContext.action.GetBindingIndexForControl(_callbackContext.control)].groups;

            if (bindingControlScheme == _playerInput.currentControlScheme)
            {
                var kickBall = _callbackContext.ReadValueAsButton();
                if (kickBall)
                    AKickBall?.Invoke(m_playerController.m_playerId);  //Tell the Ball, that it has been kicked! *kick*
            }
        }

        private void OnZoom(InputAction.CallbackContext _callbackContext, PlayerInput _playerInput)
        {
            string bindingControlScheme = _callbackContext.action.bindings[_callbackContext.action.GetBindingIndexForControl(_callbackContext.control)].groups;

            if (bindingControlScheme == _playerInput.currentControlScheme)
            {
                Vector2 zoomVector = _callbackContext.ReadValue<Vector2>();
                m_playerController.m_playerCameraController.Zooming(zoomVector, m_playerController.m_playerId, _playerInput.currentControlScheme);
            }
        }

        private void OnZoomCanceled(InputAction.CallbackContext _callbackContext, PlayerInput _playerInput)
        {
            string bindingControlScheme = _callbackContext.action.bindings[_callbackContext.action.GetBindingIndexForControl(_callbackContext.control)].groups;

            if (bindingControlScheme == _playerInput.currentControlScheme)
                m_playerController.m_playerCameraController.Zooming(Vector2.zero, m_playerController.m_playerId, "");
        }

        private void OnMousePosition(InputAction.CallbackContext _callbackContext)
        {
            Vector2 mouseVector = _callbackContext.ReadValue<Vector2>();
            ASendMousePosition?.Invoke(mouseVector);
        }

        private void OnOpenMenu(InputAction.CallbackContext _callbackContext)
        {
            if (m_playerController.m_playerId == 0)
                UserInputManager.ToggleActionMaps(m_uiActionMap);

            //var currentDevice = _callbackContext.control.inputDevice;                //Get the current inputDevice.
            //switch (currentDevice)
        }
        #endregion

        private void OnApplicationQuit()
        {
            m_onQuitProcess = true;
        }
    }
}