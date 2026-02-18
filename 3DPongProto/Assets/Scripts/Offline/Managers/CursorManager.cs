// using ThreeDeePongProto.Shared.Managers;
using ThreeDeePongProto.Shared.InputActions;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace ThreeDeePongProto.Shared.Managers
{
    public class CursorManager : PersistentSingleton<CursorManager>
    {
        private PlayerInputActions m_inputActions;
        private InputAction m_cursorMovement;

        [Header("Visuals")]
        [SerializeField] private RectTransform m_gamepadCursorTransform;
        [SerializeField] private Image m_gamepadCursorImage;
        
        [Header("Settings")]
        [SerializeField] private float m_cursorSpeed = 1000f;

        [SerializeField] private float m_deadzone = 0.15f;
        // [SerializeField] private bool m_hideCursorOnTyping = true;

        private enum ECursorMode
        {
            HardwareMouse,
            GamepadStick,
            HideOnNavigate,
            Blocked
        }

        [SerializeField] private ECursorMode m_currentMode = ECursorMode.HardwareMouse;
        // private Vector2 m_virtualCursorPos;
        // private float m_lastWarpTime = 0.0f;
        
        // Canvas Referenz für Skalierung
        // private Canvas m_canvas;

        public bool IsCursorActive => m_currentMode == ECursorMode.GamepadStick || m_currentMode == ECursorMode.HardwareMouse;

        protected override void Awake()
        {
            base.Awake();
            
            #region VirtualCursor
            // if (m_gamepadCursorImage != null)
            // {
            //     if (m_gamepadCursorTransform == null)
            //         m_gamepadCursorTransform = m_gamepadCursorImage.GetComponent<RectTransform>();
                
            //     // m_canvas = m_gamepadCursorImage.GetComponentInParent<Canvas>();
                
            //     // //Start a screen center.
            //     // m_virtualCursorPos = new Vector2(Screen.width / 2f, Screen.height / 2f);
            // }
            #endregion

            m_inputActions = UserInputManager.Instance.GetCentralActions();
            m_cursorMovement = m_inputActions.UserInterface.CursorMovement;
        }

        void OnEnable()
        {
            m_inputActions.UserInterface.Navigate.performed += NavByUiButtons;
            m_inputActions.UserInterface.CursorMovement.performed += OnCursorMovement;
        }

        void OnDisable()
        {
            m_inputActions.UserInterface.Navigate.performed -= NavByUiButtons;
            m_inputActions.UserInterface.CursorMovement.performed -= OnCursorMovement;
        }

        private void Start()
        {
            SetCursorMode(m_currentMode);
        }

        private void Update()
        {
            BlockInputCases();

            // CheckInputSources();         //REPLACE by CallbackContext
            UpdateCursorBehavior();
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
        //     // Wir prüfen, ob der letzte Warp vor weniger als 0.2 Sekunden warhoulder.
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

        private void BlockInputCases()
        {
            //Other App is focused.
            if (!Application.isFocused) return;

            //Multiple Inputs
            var mouse = Mouse.current.delta.ReadValue().magnitude > m_deadzone;
            var leftStick = Gamepad.current.leftStick.ReadValue().sqrMagnitude > m_deadzone * m_deadzone;
            var rightStick = Gamepad.current.rightStick.ReadValue().sqrMagnitude > m_deadzone * m_deadzone;

            if(m_currentMode != ECursorMode.Blocked && mouse && (leftStick || rightStick))
            {
                SetCursorMode(ECursorMode.Blocked);
                return;
            }
        }
        private void UpdateCursorBehavior()
        {
            if(m_currentMode == ECursorMode.Blocked) return;
            else if(m_currentMode != ECursorMode.Blocked)
            {
                // // 1. Stick-Input lesen
                // Vector2 input = m_cursorMovement.ReadValue<Vector2>();

                // // Wenn kein Input da ist, nichts tun (spart Performance)
                // if (input.sqrMagnitude < 0.01f) return;

                // // 2. Aktuelle Mausposition holen
                // if (Mouse.current == null) return;
                // Vector2 currentPos = Mouse.current.position.ReadValue();

                // // 3. Neue Position berechnen
                // Vector2 newPos = currentPos + (input * m_cursorSpeed * Time.deltaTime);

                // // 4. WICHTIG: Position auf den Bildschirm begrenzen (Clamping)
                // // Sonst verschwindet die Maus aus dem Fenster
                // newPos.x = Mathf.Clamp(newPos.x, 0, Screen.width);
                // newPos.y = Mathf.Clamp(newPos.y, 0, Screen.height);

                // // 5. Die Hardware-Maus verschieben
                // Mouse.current.WarpCursorPosition(newPos);
            }

            #region VirtualCursor
            // if (m_currentMode == ECursorMode.GamepadStick)
            // {
            //     if (Gamepad.current == null) return;

            //     Vector2 moveInput = Gamepad.current.rightStick.ReadValue();
            //     if (moveInput.magnitude < m_deadzone) moveInput = Vector2.zero;

            //     // Wenn keine Eingabe da ist, nichts tun (Position beibehalten!)
            //     if (moveInput == Vector2.zero) return;

            //     // Position aktualisieren
            //     m_virtualCursorPos += moveInput * m_cursorSpeed * Time.unscaledDeltaTime;

            //     // Clamp
            //     m_virtualCursorPos.x = Mathf.Clamp(m_virtualCursorPos.x, 0, Screen.width);
            //     m_virtualCursorPos.y = Mathf.Clamp(m_virtualCursorPos.y, 0, Screen.height);

            //     // UI setzen
            //     if (m_gamepadCursorTransform != null)
            //         m_gamepadCursorTransform.anchoredPosition = m_virtualCursorPos;

            //     // Hardware Maus warpen & Zeitstempel setzen
            //     if (Mouse.current != null)
            //     {
            //         Mouse.current.WarpCursorPosition(m_virtualCursorPos);
            //         m_lastWarpTime = Time.unscaledTime; // <--- WICHTIG: Zeit merken!
            //     }
            // }
            #endregion
        }

        private void SetCursorMode(ECursorMode _newMode)
        {
            #region VirtualCursor
            // if (_newMode == ECursorMode.GamepadStick)
            // {
            //     if (Mouse.current != null)
            //     {
            //         Vector2 mousePos = Mouse.current.position.ReadValue();
                    
            //         //If the mouse is at a screenBorder, set it to the middle.
            //         bool isAtEdge = mousePos.y <= 1 || mousePos.y >= Screen.height - 1 || 
            //                         mousePos.x <= 1 || mousePos.x >= Screen.width - 1;

            //         if (isAtEdge)
            //         {
            //             m_virtualCursorPos = new Vector2(Screen.width / 2f, Screen.height / 2f);
            //             Mouse.current.WarpCursorPosition(m_virtualCursorPos); 
            //         }
            //         else
            //         {
            //             m_virtualCursorPos = mousePos;
            //         }
            //     }
            // }
            #endregion

            m_currentMode = _newMode;

            switch (m_currentMode)
            {
                case ECursorMode.HardwareMouse:
                {
                    Cursor.visible = true;
                    // if (m_gamepadCursorImage) m_gamepadCursorImage.enabled = false;
                    break;
                }
                case ECursorMode.GamepadStick:
                {
                    Cursor.visible = true;
                    //.None shall be better against mouse-trembling in the Editor than .Confined.
                    Cursor.lockState = CursorLockMode.None; 
                    // if (m_gamepadCursorImage) m_gamepadCursorImage.enabled = true;
                    break;
                }
                case ECursorMode.HideOnNavigate:
                {
                    Cursor.visible = false;
                    // if (m_gamepadCursorImage) m_gamepadCursorImage.enabled = false;
                    break;
                }
                case ECursorMode.Blocked:
                {
                    // Cursor.visible = false;
                    break;
                }
                default: break;
            }
        }

        #region CallbackContext
        private void NavByUiButtons(InputAction.CallbackContext _callbackContext)
        {
            if(_callbackContext.ReadValue<Vector2>().magnitude != 0 && m_currentMode != ECursorMode.HideOnNavigate)
            {
                SetCursorMode(ECursorMode.HideOnNavigate);
            }
        }

        private void OnCursorMovement(InputAction.CallbackContext _callbackContext)
        {
            Vector2 moveVector = _callbackContext.ReadValue<Vector2>();
            if(moveVector == Vector2.zero) return;
            
            switch (_callbackContext.control.device)
            {
                case Mouse:
                {
                    if(moveVector.magnitude > m_deadzone && m_currentMode != ECursorMode.HardwareMouse)
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