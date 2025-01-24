// Extended ActionMap for PlayerActions.
// Get's set up in Unity Editor, but represents the structur.
using UnityEngine.InputSystem;
using UnityEngine;

[CreateAssetMenu(fileName = "Scriptable Objects/InputActions", menuName = "Input/PlayerActions")]
public class CustomPlayerInputActions : InputActionAsset
{
    // PlayerMovement (Vector2)
    public InputAction Move => FindActionMap("PlayerActions").FindAction("Move");

    // PlayerRotation (Vector2 oder Float)
    public InputAction Rotate => FindActionMap("PlayerActions").FindAction("Rotate");

    // Actions like Jump, Interact and Attack.
    public InputAction Push => FindActionMap("PlayerActions").FindAction("Push");
    public InputAction Kick => FindActionMap("PlayerActions").FindAction("Kick");

    // Camera and Interface
    public InputAction MousePosition => FindActionMap("PlayerActions").FindAction("MousePosition");
    public InputAction CursorVisibility => FindActionMap("PlayerActions").FindAction("CursorVisibility");

    // MenuControl
    public InputAction ToggleGameMenu => FindActionMap("PlayerActions").FindAction("ToggleGameMenu");

    // Multiplayer-specific Extensions
    public InputActionMap MultiplayerActions => FindActionMap("MultiplayerActions");

    // Dynamic UI Access
    public InputActionMap UserInterface => FindActionMap("UserInterface");
}