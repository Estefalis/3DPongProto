using System.Collections;
using System.Collections.Generic;
using ThreeDeePongProto.Shared.InputActions;
using UnityEngine;
using UnityEngine.InputSystem;

public class MenuCamera : MonoBehaviour
{
    private PlayerInputActions m_playerInputActions;

    [Header("Cursor Restrictions")]
    [SerializeField] private CursorLockMode m_cursorLockMode;
    [SerializeField] private bool m_cursorVisibility = false;

    private Vector2 m_mousePosition, m_lastMousePosition;

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

    private void SetCursorRestrictions(CursorLockMode _lockMode, bool _visibility)
    {
        Cursor.lockState = _lockMode;    //Lock to ScreenCenter (Locked), or to the inside of the Screen (Confined).
        Cursor.visible = _visibility;    //true = visible, false = invisible.
    }

    private void SwitchCursorVisibility()
    {
        switch (Cursor.visible)
        {
            case false: //invisible to visible
            {
                Mouse.current.WarpCursorPosition(m_lastMousePosition);  //Set Mouse to it's last Position.
                //TODO: Disable objectSelection with Mouse, while it's invisible.
                SetCursorRestrictions(m_cursorLockMode, true);
                break;
            }
            case true:  //visible to invisible
            {
                GetMousePosition();
                m_lastMousePosition = m_mousePosition;

                SetCursorRestrictions(m_cursorLockMode, false);
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

    #region CallbackContext
    private void SwitchCursorVisibility(InputAction.CallbackContext _callbackContext)
    {
        SwitchCursorVisibility();
    }
    #endregion
}