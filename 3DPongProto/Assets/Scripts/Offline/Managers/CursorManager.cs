using ThreeDeePongProto.Shared.Managers;
using ThreeDeePongProto.Shared.InputActions;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace ThreeDeePongProto.Shared.Managers
{
    public class CursorManager : PersistentSingleton<CursorManager>
    {
        private PlayerInputActions m_inputActions;

        [Header("Visuals")]
        [SerializeField] private RectTransform m_gamepadCursorTransform; // Direkt als RectTransform
        [SerializeField] private Image m_gamepadCursorImage;
        
        [Header("Settings")]
        [SerializeField] private float m_cursorSpeed = 1000f;
        [SerializeField] private float m_deadzone = 0.15f;
        // [SerializeField] private bool m_hideCursorOnTyping = true;

        private enum ECursorMode
        {
            HardwareMouse,
            GamepadStick,
            HideOnNavigate
        }

        private ECursorMode m_currentMode = ECursorMode.HardwareMouse;
        private Vector2 m_virtualCursorPos;
        private float m_lastWarpTime = 0.0f;
        
        // Canvas Referenz für Skalierung
        private Canvas m_canvas;

        public bool IsCursorActive => m_currentMode == ECursorMode.GamepadStick || m_currentMode == ECursorMode.HardwareMouse;

        protected override void Awake()
        {
            base.Awake();
            
            if (m_gamepadCursorImage != null)
            {
                if (m_gamepadCursorTransform == null)
                    m_gamepadCursorTransform = m_gamepadCursorImage.GetComponent<RectTransform>();
                
                m_canvas = m_gamepadCursorImage.GetComponentInParent<Canvas>();
                
                //Start a screen center.
                m_virtualCursorPos = new Vector2(Screen.width / 2f, Screen.height / 2f);
            }

            m_inputActions = UserInputManager.Instance.GetCentralActions();
        }

        void OnEnable()
        {
            m_inputActions.UserInterface.Navigate.performed += NavByUiButtons;
            m_inputActions.UserInterface.CursorVisibility.performed += CrosshairChange;
        }

        void OnDisable()
        {
            m_inputActions.UserInterface.Navigate.performed -= NavByUiButtons;
            m_inputActions.UserInterface.CursorVisibility.performed -= CrosshairChange;
        }

        private void Start()
        {
            SetCursorMode(ECursorMode.HideOnNavigate);
        }

        private void Update()
        {
            if (!Application.isFocused) return;

            // CheckInputSources();         //REPLACE by CallbackContext
            // UpdateCursorBehavior();
        }

        // private void CheckInputSources()
        // {
        //     //1st GAMEPAD check
        //     if (Gamepad.current != null)
        //     {
        //         Vector2 stickInput = Gamepad.current.rightStick.ReadValue();
                
        //         if (stickInput.sqrMagnitude > m_deadzone * m_deadzone)
        //         {
        //             if (m_currentMode != ECursorMode.GamepadStick)
        //                 SetCursorMode(ECursorMode.GamepadStick);
                    
        //             return; //Skip HardwareMouse
        //         }
                
        //         //UI-D-Pad Navigation
        //         if (Gamepad.current.dpad.ReadValue().sqrMagnitude > 0.1f)
        //         {
        //             if (m_currentMode != ECursorMode.HideOnNavigate) SetCursorMode(ECursorMode.HideOnNavigate);
        //             return;
        //         }
        //     }

        //     // 2. MAUS CHECK (Mit Warp-Filter!)
        //     // Wir prüfen, ob der letzte Warp vor weniger als 0.2 Sekunden war.
        //     // Falls ja, ignorieren wir die Maus, weil das Signal vermutlich unser eigenes Echo ist.
        //     bool isWarpEcho = (Time.unscaledTime - m_lastWarpTime) < 0.2f;

        //     if (!isWarpEcho && Mouse.current != null)
        //     {
        //         // Nur auf echte, starke Bewegungen reagieren
        //         if (Mouse.current.delta.ReadValue().sqrMagnitude > 4.0f)
        //         {
        //             if (m_currentMode != ECursorMode.HardwareMouse)
        //                 SetCursorMode(ECursorMode.HardwareMouse);
        //         }
        //     }

        //     // 3. KEYBOARD
        //     if (m_hideCursorOnTyping && Keyboard.current != null)
        //     {
        //         if (Keyboard.current.anyKey.wasPressedThisFrame)
        //         {
        //              if (Keyboard.current.upArrowKey.isPressed || Keyboard.current.downArrowKey.isPressed || 
        //                  Keyboard.current.leftArrowKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
        //              {
        //                  if (m_currentMode != ECursorMode.HideOnNavigate) SetCursorMode(ECursorMode.HideOnNavigate);
        //              }
        //         }
        //     }
        // }

        private void UpdateCursorBehavior()
        {
            if (m_currentMode == ECursorMode.GamepadStick)
            {
                if (Gamepad.current == null) return;

                Vector2 moveInput = Gamepad.current.rightStick.ReadValue();
                if (moveInput.magnitude < m_deadzone) moveInput = Vector2.zero;

                // Wenn keine Eingabe da ist, nichts tun (Position beibehalten!)
                if (moveInput == Vector2.zero) return;

                // Position aktualisieren
                m_virtualCursorPos += moveInput * m_cursorSpeed * Time.unscaledDeltaTime;

                // Clamp
                m_virtualCursorPos.x = Mathf.Clamp(m_virtualCursorPos.x, 0, Screen.width);
                m_virtualCursorPos.y = Mathf.Clamp(m_virtualCursorPos.y, 0, Screen.height);

                // UI setzen
                if (m_gamepadCursorTransform != null)
                    m_gamepadCursorTransform.anchoredPosition = m_virtualCursorPos;

                // Hardware Maus warpen & Zeitstempel setzen
                if (Mouse.current != null)
                {
                    Mouse.current.WarpCursorPosition(m_virtualCursorPos);
                    m_lastWarpTime = Time.unscaledTime; // <--- WICHTIG: Zeit merken!
                }
            }
        }

        private void SetCursorMode(ECursorMode _newMode)
        {
            if (_newMode == ECursorMode.GamepadStick)
            {
                if (Mouse.current != null)
                {
                    Vector2 mousePos = Mouse.current.position.ReadValue();
                    
                    //If the mouse is at a screenBorder, set it to the middle.
                    bool isAtEdge = mousePos.y <= 1 || mousePos.y >= Screen.height - 1 || 
                                    mousePos.x <= 1 || mousePos.x >= Screen.width - 1;

                    if (isAtEdge)
                    {
                        m_virtualCursorPos = new Vector2(Screen.width / 2f, Screen.height / 2f);
                        Mouse.current.WarpCursorPosition(m_virtualCursorPos); 
                    }
                    else
                    {
                        m_virtualCursorPos = mousePos;
                    }
                }
            }

            m_currentMode = _newMode;

            switch (m_currentMode)
            {
                case ECursorMode.HardwareMouse:
                    Cursor.visible = true;
                    if (m_gamepadCursorImage) m_gamepadCursorImage.enabled = false;
                    break;

                case ECursorMode.GamepadStick:
                    Cursor.visible = false;
                    //.None shall be better against mouse-trembling in the Editor than .Confined.
                    Cursor.lockState = CursorLockMode.None; 
                    if (m_gamepadCursorImage) m_gamepadCursorImage.enabled = true;
                    break;

                case ECursorMode.HideOnNavigate:
                    Cursor.visible = false;
                    if (m_gamepadCursorImage) m_gamepadCursorImage.enabled = false;
                    break;
            }
            Debug.Log($"{m_currentMode}");
        }

        #region CallbackContext
        private void NavByUiButtons(InputAction.CallbackContext _callbackContext)
        {
            if(_callbackContext.ReadValue<Vector2>().magnitude != 0 && m_currentMode != ECursorMode.HideOnNavigate)
            {
                SetCursorMode(ECursorMode.HideOnNavigate);
            }
        }

        private void CrosshairChange(InputAction.CallbackContext _callbackContext)
        {
            Vector2 moveVector = _callbackContext.ReadValue<Vector2>();
            switch (_callbackContext.control.device)
            {
                case Mouse:
                {
                    if(moveVector.magnitude > 0.1f && m_currentMode != ECursorMode.HardwareMouse)
                    SetCursorMode(ECursorMode.HardwareMouse);
                    break;
                }
                case Gamepad:
                {
                    if(moveVector.sqrMagnitude > m_deadzone * m_deadzone && m_currentMode != ECursorMode.GamepadStick)
                    SetCursorMode(ECursorMode.GamepadStick);
                    break;
                }
                default: break;
            }
        }
        #endregion
    }
}