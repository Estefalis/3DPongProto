using ThreeDeePongProto.Shared.InputActions;
using UnityEngine;
using UnityEngine.InputSystem;

public class MenuCamera : MonoBehaviour
{
    private PlayerInputActions m_playerInputActions;

    [Header("Cursor Restrictions")]
    [SerializeField] private CursorLockMode m_cursorLockMode;
    [SerializeField] private bool m_cursorVisibility = false;

    private Vector2 m_mousePosition, m_lastMousePosition, m_disabledMousePosition = Vector2.zero;

    private void Awake()
    {
        SetCursorRestrictions(m_cursorLockMode, m_cursorVisibility);
    }

    private void OnDisable()
    {
        m_playerInputActions.UI.Disable();
        m_playerInputActions.UI.CursorVisibility.performed -= SwitchCursorVisibility;
    }

    private void Start()
    {
        m_playerInputActions = InputManager.m_PlayerInputActions;
        m_playerInputActions.UI.Enable();

        m_playerInputActions.UI.CursorVisibility.performed += SwitchCursorVisibility;
    }

    private void SwitchCursorState()
    {
        switch (Cursor.visible)
        {
            case true:  //Cursor is currently visible.
            {
                GetMousePosition();
                SetCursorRestrictions(CursorLockMode.Confined, false);
                m_playerInputActions.UI.MousePosition.Disable();
                break;
            }
            case false: //Cursor is currently invisible.
            {
                Mouse.current.WarpCursorPosition(m_lastMousePosition);  //Set Mouse to it's last visible Position.
                SetCursorRestrictions(CursorLockMode.None, true);
                m_playerInputActions.UI.MousePosition.Enable();
                break;
            }
        }
    }

    private void GetMousePosition()
    {
#if ENABLE_INPUT_SYSTEM
        m_mousePosition = m_playerInputActions.UI.MousePosition.ReadValue<Vector2>();
#else
            m_mousePosition = Input.mousePosition;
#endif
    }

    private void SetCursorRestrictions(CursorLockMode _lockMode, bool _visibility)
    {
        switch (_visibility)
        {
            case false:
            {
                m_lastMousePosition = m_mousePosition;
                Mouse.current.WarpCursorPosition(m_disabledMousePosition);
                break;
            }
            case true:
                break;
        }

        m_cursorLockMode = _lockMode;   //Sets 'm_cursorLockMode' in the Inspector.
        Cursor.lockState = _lockMode;   //Lock Cursor inside the Screen with '.Confined'. Unlocks the Cursor with '.None'.
        Cursor.visible = _visibility;   //true = visible, false = invisible.
    }

    #region CallbackContext
    private void SwitchCursorVisibility(InputAction.CallbackContext _callbackContext)
    {
        SwitchCursorState();
    }
    #endregion
}