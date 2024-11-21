using ThreeDeePongProto.Shared.InputActions;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Users;

namespace ThreeDeePongProto.Offline.UI
{
    public class GamepadCursor : MonoBehaviour
    {
        //NOTE: May combine GamepadCursor & CursorVisibility scripts.
        private PlayerInputActions m_playerInputActions;
        [SerializeField] private PlayerInput m_playerInput;

        [SerializeField] private RectTransform m_cursorTransform;
        [SerializeField] private float m_cursorSpeed = 1000.0f;

        private Mouse m_virtualMouse;
        private bool m_previousMouseState;
        private const string m_virtualMouseString = "VirtualMouse";

        private void OnEnable()
        {
            if (m_virtualMouse == null)
                m_virtualMouse = (Mouse)InputSystem.AddDevice(m_virtualMouseString);
            else if(!m_virtualMouse.added)
                InputSystem.AddDevice(m_virtualMouseString);

            //Pairs the device to use the PlayerInput-Component.
            InputUser.PerformPairingWithDevice(m_virtualMouse, m_playerInput.user);

            if(m_cursorTransform != null)
            {
                Vector2 cursorPosition = m_cursorTransform.anchoredPosition;
                InputState.Change(m_virtualMouse.position, cursorPosition); //New virtualMouse position = old mouse position.
            }

            InputSystem.onAfterUpdate += UpdateVirtualMousePosition;
        }

        private void OnDisable()
        {
            InputSystem.onAfterUpdate -= UpdateVirtualMousePosition;
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

            Vector2 deltaValue = m_playerInputActions.PlayerActions.MousePosition.ReadValue<Vector2>();
            deltaValue *= m_cursorSpeed * Time.unscaledDeltaTime;  //'deltaTime' or 'unscaledDeltaTime'?

            Vector2 virtualMousePos = m_virtualMouse.position.ReadValue();
            Vector2 newVMPosition = virtualMousePos + deltaValue;

            newVMPosition.x = Mathf.Clamp(newVMPosition.x, 0 /*+ m_borderZone*/, Screen.width /*- m_borderZone*/);
            newVMPosition.y = Mathf.Clamp(newVMPosition.y, 0 /*+ m_borderZone*/, Screen.height /*- m_borderZone*/);

            InputState.Change(m_virtualMouse.position, newVMPosition);
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
        }
    }
}