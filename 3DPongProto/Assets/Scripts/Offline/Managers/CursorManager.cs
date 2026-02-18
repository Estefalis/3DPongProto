using ThreeDeePongProto.Shared.InputActions;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ThreeDeePongProto.Shared.Managers
{
    public class CursorManager : PersistentSingleton<CursorManager>
    {
        private PlayerInputActions m_inputActions;
        private InputAction m_cursorMovement;
        
        [Header("Settings")]
        [SerializeField] private float m_potency = 2.0f;
        private float m_baseSpeed = 1000f;

        [SerializeField] private float m_deadzone = 0.15f;

        private enum ECursorMode
        {
            HardwareMouse,
            GamepadStick,
            HideOnNavigate,
            Blocked
        }

        [SerializeField] private ECursorMode m_currentMode = ECursorMode.HardwareMouse;

        // public bool IsCursorActive => m_currentMode == ECursorMode.GamepadStick || m_currentMode == ECursorMode.HardwareMouse;
        private bool m_gamepadMovement = false, m_mouseMovement = false;

        protected override void Awake()
        {
            base.Awake();
            m_inputActions = UserInputManager.Instance.GetCentralActions();
            m_cursorMovement = m_inputActions.UserInterface.PadCursorMove;
        }

        void OnEnable()
        {
            m_inputActions.UserInterface.Navigate.performed += NavByUiButtons;
        }

        void OnDisable()
        {
            m_inputActions.UserInterface.Navigate.performed -= NavByUiButtons;
        }

        private void Start()
        {
            SetCursorMode(m_currentMode);
        }

        private void Update()
        {
            if (!Application.isFocused) return;
            
            UpdateCursorBehavior();
        }

        private void UpdateCursorBehavior()
        {
            if (Mouse.current == null) return;                                                          //Mouse-check.
            m_mouseMovement = Mouse.current.delta.magnitude > m_deadzone;
            Vector2 actionInput = m_cursorMovement.ReadValue<Vector2>();
            m_gamepadMovement = actionInput.sqrMagnitude > m_deadzone * m_deadzone;
            var blocked = m_mouseMovement && m_gamepadMovement;
            
            if(blocked)
            {
                if(m_currentMode != ECursorMode.Blocked)
                    SetCursorMode(ECursorMode.Blocked);
                return;
            }

            if(m_gamepadMovement && m_currentMode != ECursorMode.GamepadStick)
                SetCursorMode(ECursorMode.GamepadStick);
            
            if(m_mouseMovement && m_currentMode != ECursorMode.HardwareMouse)
                SetCursorMode(ECursorMode.HardwareMouse);


            if (m_currentMode != ECursorMode.GamepadStick || blocked) return;

            if (actionInput.magnitude < m_deadzone * m_deadzone) return;                                //Reduce performance-cost.

            Vector2 currentMousePos = Mouse.current.position.ReadValue();                               //Get current mouse-position.
            Vector2 newPos = currentMousePos + (m_baseSpeed * m_potency * Time.unscaledDeltaTime * actionInput);  //Calculate new position.
            
            //Clamping the cursor within the window.
            newPos.x = Mathf.Clamp(newPos.x, 0, Screen.width);
            newPos.y = Mathf.Clamp(newPos.y, 0, Screen.height);

            Mouse.current.WarpCursorPosition(newPos);                                                   //Set hardwareMouse on new position.
        }

        private void SetCursorMode(ECursorMode _newMode)
        {
            if (m_currentMode == _newMode) return;  //Block identical calls.

            m_currentMode = _newMode;
            //CursorLockMode.None shall be better against mouse-trembling in the Editor than CursorLockMode.Confined.
            switch (m_currentMode)
            {
                case ECursorMode.HardwareMouse:
                case ECursorMode.GamepadStick:
                {
                    Cursor.visible = true;
                    Cursor.lockState = CursorLockMode.None;
                    break;
                }
                case ECursorMode.HideOnNavigate:
                {
                    Cursor.visible = false;
                    Cursor.lockState = CursorLockMode.Locked;
                    break;
                }
                case ECursorMode.Blocked:
                {
                    Cursor.visible = false;
                    // Mouse.current.WarpCursorPosition(Vector2.zero);
                    break;
                }
                default: break;
            }
        }

        #region CallbackContext
        private void NavByUiButtons(InputAction.CallbackContext _callbackContext)
        {
            if(_callbackContext.action.WasPerformedThisFrame() && m_currentMode != ECursorMode.HideOnNavigate)
            {
                SetCursorMode(ECursorMode.HideOnNavigate);
            }
        }
        #endregion
    }
}