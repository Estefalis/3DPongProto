using ThreeDeePongProto.Shared.InputActions;
using ThreeDeePongProto.Shared.Managers;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Users;

namespace ThreeDeePongProto.Shared.UI
{
    public class CursorManager : MonoBehaviour
    {
        private PlayerInputActions m_characterInputActions;

        [SerializeField] private PlayerInput m_playerInput;

        [SerializeField] private RectTransform m_cursorTransform;
        [SerializeField] private Canvas m_mainCanvas;
        [SerializeField] private Camera m_myCamera; //If not 'm_camera = Camera.main;' without '[SerializeField]'.
        [SerializeField] private float m_cursorSpeed = 1000.0f;
        [SerializeField] private float m_borderZone = 30.0f;

        [Header("Cursor Restrictions")]
        [SerializeField] private CursorLockMode m_cursorLockMode;
        [SerializeField] private bool m_cursorVisibility = false;

        private RectTransform m_canvasRectTransform;

        private Mouse m_virtualMouse;
        private Mouse m_physicalMouse;
        private bool m_previousMouseState;

        //private GameObject m_tempSaveObject;

        private string m_lastControlSchemeChange = "";
        private const string m_virtualMouseString = "VirtualMouse";
        private const string m_keyboardMouseScheme = "KeyboardMouse";           //Inputsystem's KeyboardMouse scheme. (groups)
        private const string m_gamePadScheme = "Gamepad";                       //Inputsystem's Gamepad scheme. (groups)

        private void Awake()
        {
            m_canvasRectTransform = m_mainCanvas.GetComponent<RectTransform>();
            m_physicalMouse = Mouse.current;

            m_cursorTransform.SetAsLastSibling();
            SetCursorRestrictions(m_cursorLockMode, m_cursorVisibility);
        }

        private void OnEnable()
        {
            if (m_virtualMouse == null)
                m_virtualMouse = (Mouse)InputSystem.AddDevice(m_virtualMouseString);
            else if (!m_virtualMouse.added)
                InputSystem.AddDevice(m_virtualMouseString);

            //Pairs the device to use the PlayerInput-Component.
            InputUser.PerformPairingWithDevice(m_virtualMouse, m_playerInput.user);

            if (m_cursorTransform != null)
            {
                Vector2 cursorPosition = m_cursorTransform.anchoredPosition;
                InputState.Change(m_virtualMouse.position, cursorPosition); //New virtualMouse position = old mouse position.
            }

            InputSystem.onAfterUpdate += UpdateMicePositions;
            //m_menuInput.onControlsChanged += OnInputDeviceChanged;

            InputUser.onChange += OnDeviceChange;
        }

        private void OnDisable()
        {
            if (m_virtualMouse != null && m_virtualMouse.added)
            {
                m_playerInput.user.UnpairDevice(m_virtualMouse);
                InputSystem.RemoveDevice(m_virtualMouse);
            }

            InputSystem.onAfterUpdate -= UpdateMicePositions;
            //m_menuInput.onControlsChanged -= OnInputDeviceChanged;

            InputUser.onChange -= OnDeviceChange;

            m_characterInputActions.Disable();
            m_characterInputActions.UserInterface.CursorVisibility.performed -= SwitchCursorVisibility;
        }

        private void Start()
        {
            m_characterInputActions = RebindManager.m_PlayerInputActions;
            m_characterInputActions.Enable();

            m_characterInputActions.UserInterface.CursorVisibility.performed += SwitchCursorVisibility;
        }

        private void UpdateMicePositions()
        {
            if (m_virtualMouse == null || Gamepad.current == null)
                return;

            Vector2 deltaValue = Gamepad.current.leftStick.ReadValue();
            deltaValue *= m_cursorSpeed * Time.unscaledDeltaTime;

            Vector2 virtualMousePos = m_virtualMouse.position.ReadValue();
            Vector2 newPosition = virtualMousePos + deltaValue;

            newPosition.x = Mathf.Clamp(newPosition.x, 0 + m_borderZone, Screen.width - m_borderZone);
            newPosition.y = Mathf.Clamp(newPosition.y, 0 + m_borderZone, Screen.height - m_borderZone);

            InputState.Change(m_virtualMouse.position, newPosition);
            InputState.Change(m_virtualMouse.delta, deltaValue);

            bool southButtonIsPressed = Gamepad.current.buttonSouth.isPressed;
            if (m_previousMouseState != southButtonIsPressed)
            {
                m_virtualMouse.CopyState<MouseState>(out var mouseState);
                mouseState.WithButton(MouseButton.Left, southButtonIsPressed);
                InputState.Change(m_virtualMouse, mouseState);
                m_previousMouseState = southButtonIsPressed;
            }

            bool southButtonIsReleased = !Gamepad.current.buttonSouth.isPressed;
            if (m_previousMouseState != southButtonIsReleased)
            {
                //Copy the state of the virtualMouse and map it to the leftMouseButton.
                m_virtualMouse.CopyState<MouseState>(out var mouseState);
                mouseState.WithButton(MouseButton.Left, southButtonIsReleased);
                InputState.Change(m_virtualMouse, mouseState);
                m_previousMouseState = southButtonIsReleased;
            }

            ReplaceCursorAt(newPosition);
        }

        private void ReplaceCursorAt(Vector2 _exchangePosition)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(m_canvasRectTransform, _exchangePosition, m_mainCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : m_myCamera, out Vector2 newPosition);
            m_cursorTransform.anchoredPosition = newPosition;
        }

        #region Samyam's Version
        //private void OnInputDeviceChanged(PlayerInput _playerInput)
        //{
        //    if (m_menuInput.m_currentControlScheme == m_keyboardMouseScheme && m_lastControlSchemeChange != m_keyboardMouseScheme)
        //    {
        //        m_cursorTransform.gameObject.SetActive(false);
        //        Cursor.visible = true;
        //        m_physicalMouse.WarpCursorPosition(m_virtualMouse.position.ReadValue());
        //        m_lastControlSchemeChange = m_keyboardMouseScheme;
        //    }
        //    else if (m_menuInput.m_currentControlScheme == m_gamePadScheme && m_lastControlSchemeChange != m_gamePadScheme)
        //    {
        //        m_cursorTransform.gameObject.SetActive(true);
        //        Cursor.visible = false;
        //        InputState.Change(m_virtualMouse.position, m_physicalMouse.position.ReadValue());
        //        ReplaceCursorAt(m_physicalMouse.position.ReadValue());
        //        m_lastControlSchemeChange = m_keyboardMouseScheme;
        //    }
        //}

        //May Update() 'if(m_lastControlSchemeChange != m_menuInput.m_currentControlScheme) with an own 'OnControlsChanged()' method. And update 'm_lastControlSchemeChange = m_menuInput.m_currentControlScheme;' to it.
        #endregion

        private void OnDeviceChange(InputUser _inputUser, InputUserChange _inputUserChange, InputDevice _inputDevice)
        {
            if (_inputUserChange == InputUserChange.ControlSchemeChanged)
            {
                switch (_inputUser.controlScheme.Value.name)
                {
                    case m_keyboardMouseScheme:
                    {
                        m_cursorTransform.gameObject.SetActive(false);
                        Cursor.visible = true;
                        m_physicalMouse.WarpCursorPosition(m_virtualMouse.position.ReadValue());
                        break;
                    }
                    case m_gamePadScheme:
                    {
                        m_cursorTransform.gameObject.SetActive(true);
                        Cursor.visible = false;
                        InputState.Change(m_virtualMouse.position, m_physicalMouse.position.ReadValue());
                        ReplaceCursorAt(m_physicalMouse.position.ReadValue());
                        break;
                    }
                    default:
                        break;
                }

                m_lastControlSchemeChange = _inputUser.controlScheme.Value.name;
            }
        }

        private void SwitchCursorVisibility()
        {
            switch (Cursor.visible)
            {
                case true:  //Cursor is currently visible.
                {
                    SetCursorRestrictions(m_cursorLockMode, false);
                    break;
                }
                case false: //Cursor is currently invisible.
                {
                    SetCursorRestrictions(m_cursorLockMode, true);
                    break;
                }
            }
        }

        private void SetCursorRestrictions(CursorLockMode _lockMode, bool _visibility)
        {
            Cursor.lockState = _lockMode;   //Lock Cursor inside the Screen with '.Confined'. Unlocks the Cursor with '.None'.
            Cursor.visible = _visibility;   //true = visible, false = invisible.
            //TODO: If mouseCursor shall be invisible, it needs to get locked at a certain position and objectDetection to be disabled.
        }

        private void SwitchCursorVisibility(InputAction.CallbackContext _callbackContext)
        {
            SwitchCursorVisibility();
        }
    }
}