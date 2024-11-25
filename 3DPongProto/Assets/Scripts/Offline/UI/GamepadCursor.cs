using System;
using ThreeDeePongProto.Shared.InputActions;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Users;

//HINT: Currently Point and MousePosition both positions of physical and virtual mouse.
namespace ThreeDeePongProto.Offline.UI
{
    public class GamepadCursor : MonoBehaviour
    {
        //NOTE: May combine GamepadCursor & CursorVisibility scripts.
        private PlayerInputActions m_playerInputActions;
        [SerializeField] private PlayerInput m_playerInput;

        [SerializeField] private RectTransform m_gamepadCursor;
        [SerializeField] private Canvas m_mainCanvas;
        private RectTransform m_canvasRectTransform;
        [SerializeField] private Camera m_myCamera; //If not 'm_camera = Camera.main;' without '[SerializeField]'.
        [SerializeField] private float m_cursorSpeed = 1000.0f;
        [SerializeField] private float m_borderZone = 30.0f;

        private Mouse m_physicalMouse;
        private Mouse m_virtualMouse;
        private bool m_previousMouseState;

        private string m_previousControlScheme = "";
        private const string m_virtualMouseString = "VirtualMouse";
        private const string m_keyboardMouseScheme = "KeyboardMouse";           //Inputsystem's KeyboardMouse scheme. (groups)
        private const string m_gamePadScheme = "Gamepad";                       //Inputsystem's Gamepad scheme. (groups)

        private void Awake()
        {
            m_canvasRectTransform = m_mainCanvas.GetComponent<RectTransform>();
            m_physicalMouse = Mouse.current;
        }

        private void OnEnable()
        {
            if (m_virtualMouse == null)
                m_virtualMouse = (Mouse)InputSystem.AddDevice(m_virtualMouseString);
            else if (!m_virtualMouse.added)
                InputSystem.AddDevice(m_virtualMouseString);

            //Pairs the device to use the PlayerInput-Component.
            InputUser.PerformPairingWithDevice(m_virtualMouse, m_playerInput.user);
            //if (Gamepad.current != null)
            //{
            //    m_gamepadUser = InputUser.PerformPairingWithDevice(Gamepad.current);
            //    InputUser.PerformPairingWithDevice(m_virtualMouse, _inputUser: m_gamepadUser);
            //    m_gamepadUser.AssociateActionsWithUser(m_playerInputActions);
            //}

            if (m_gamepadCursor != null)
            {
                Vector2 cursorPosition = m_gamepadCursor.anchoredPosition;
                InputState.Change(m_virtualMouse.position, cursorPosition); //New virtualMouse position = old mouse position.
            }

            InputSystem.onAfterUpdate += UpdateVirtualMousePosition;
            //m_playerInput.onControlsChanged += OnInputDeviceChanged;

            InputUser.onChange += OnChangedToNewDevice;
            m_previousControlScheme = m_keyboardMouseScheme;
        }

        private void OnDisable()
        {
            if (m_virtualMouse != null && m_virtualMouse.added)
            {
                m_playerInput.user.UnpairDevice(m_virtualMouse);
                InputSystem.RemoveDevice(m_virtualMouse);
            }

            InputSystem.onAfterUpdate -= UpdateVirtualMousePosition;
            //m_playerInput.onControlsChanged -= OnInputDeviceChanged;

            InputUser.onChange -= OnChangedToNewDevice;
        }

        private void Start()
        {
            m_playerInputActions = InputManager.m_PlayerInputActions;
            m_playerInputActions.UI.Enable();
        }

        private void UpdateVirtualMousePosition()
        {
            if (m_virtualMouse == null || Gamepad.current == null)
                return;

            Vector2 deltaValue = m_playerInputActions.UI.Navigate.ReadValue<Vector2>();
            deltaValue *= m_cursorSpeed * Time.unscaledDeltaTime;  //'deltaTime' or 'unscaledDeltaTime'?

            Vector2 virtualMousePos = m_virtualMouse.position.ReadValue();
            Vector2 newPosition = virtualMousePos + deltaValue;

            newPosition.x = Mathf.Clamp(newPosition.x, 0 + m_borderZone, Screen.width - m_borderZone);
            newPosition.y = Mathf.Clamp(newPosition.y, 0 + m_borderZone, Screen.height - m_borderZone);

            InputState.Change(m_virtualMouse.position, newPosition);
            InputState.Change(m_virtualMouse.delta, deltaValue);

            bool submitButtonIsPressed = m_playerInputActions.UI.Submit.IsPressed();    //Or Gamepad.current.a/xButton.IsPressed().
            //bool submitButtonIsPressed = m_playerInputActions.PlayerActions.enabled ? m_playerInputActions.PlayerActions.XYZ.IsPressed() : m_playerInputActions.UI.Submit.IsPressed();
            if (m_previousMouseState != submitButtonIsPressed)
            {
                //Copy the state of the virtualMouse and map it to the leftMouseButton.
                m_virtualMouse.CopyState<MouseState>(out var mouseState);
                mouseState.WithButton(MouseButton.Left, submitButtonIsPressed);
                InputState.Change(m_virtualMouse, mouseState);
                m_previousMouseState = submitButtonIsPressed;
            }

            ReplaceCursorAt(newPosition);
        }

        private void ReplaceCursorAt(Vector2 _newPosition)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(m_canvasRectTransform, _newPosition, m_mainCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : m_myCamera, out Vector2 newPosition);
            m_gamepadCursor.anchoredPosition = newPosition;
        }

        //private void OnInputDeviceChanged(PlayerInput _playerInput)
        //{
        //    if (m_playerInput.currentControlScheme == m_keyboardMouseScheme && m_previousControlScheme != m_keyboardMouseScheme)
        //    {
        //        m_gamepadCursor.gameObject.SetActive(false);
        //        Cursor.visible = true;
        //        m_physicalMouse.WarpCursorPosition(m_virtualMouse.position.ReadValue());
        //        m_previousControlScheme = m_keyboardMouseScheme;
        //    }
        //    else if (m_playerInput.currentControlScheme == m_gamePadScheme && m_previousControlScheme != m_gamePadScheme)
        //    {
        //        m_gamepadCursor.gameObject.SetActive(true);
        //        Cursor.visible = false;
        //        InputState.Change(m_virtualMouse.position, m_physicalMouse.position.ReadValue());
        //        ReplaceCursorAt(m_physicalMouse.position.ReadValue());
        //        m_previousControlScheme = m_keyboardMouseScheme;
        //    }
        //}

        //May Update() 'if(m_previousControlScheme != m_playerInput.currentControlScheme) with an own 'OnControlsChanged()' method. And update 'm_previousControlScheme = m_playerInput.currentControlScheme;' to it.

        private void OnChangedToNewDevice(InputUser _inputUser, InputUserChange _inputUserChange, InputDevice _inputDevice)
        {
            if (_inputUserChange == InputUserChange.ControlSchemeChanged)
            {
                switch (_inputUser.controlScheme.Value.name)
                {
                    case m_keyboardMouseScheme:
                    {
                        if (m_previousControlScheme != m_keyboardMouseScheme)
                        {
                            m_gamepadCursor.gameObject.SetActive(false);
                            Cursor.visible = true;
                            m_physicalMouse.WarpCursorPosition(m_virtualMouse.position.ReadValue());
                            m_previousControlScheme = m_keyboardMouseScheme;
                            Debug.Log($"String: {m_previousControlScheme} | Method: {_inputUser.controlScheme.Value.name}");
                        }
                        break;
                    }
                    case m_gamePadScheme:
                    {
                        if (m_previousControlScheme != m_gamePadScheme)
                        {
                            m_gamepadCursor.gameObject.SetActive(true);
                            Cursor.visible = false;
                            InputState.Change(m_virtualMouse.position, m_physicalMouse.position.ReadValue());
                            ReplaceCursorAt(m_physicalMouse.position.ReadValue());
                            m_previousControlScheme = m_gamePadScheme;
                            Debug.Log($"String: {m_previousControlScheme} | Method: {_inputUser.controlScheme.Value.name}");
                        }
                        break;
                    }
                }
            }
        }
    }
}