using System;
using UnityEngine;

namespace ThreeDeePongProto.Offline.UI
{
    [Serializable]
    public struct GamepadIcons  //struct and Sprites NEVER readonly! Because that RESETS the button-assignments in the inspector! :(
    {
        public Sprite buttonNorth;
        public Sprite buttonSouth;
        public Sprite buttonWest;
        public Sprite buttonEast;
        public Sprite startButton;
        public Sprite selectButton;
        public Sprite leftShoulder;
        public Sprite rightShoulder;
        public Sprite leftTrigger;
        public Sprite rightTrigger;
        public Sprite dpad;
        public Sprite dpadUp;
        public Sprite dpadDown;
        public Sprite dpadLeft;
        public Sprite dpadRight;
        public Sprite leftStick;
        public Sprite leftStickX;
        public Sprite leftStickLeft;
        public Sprite leftStickRight;
        public Sprite leftStickY;
        public Sprite leftStickUp;
        public Sprite leftStickDown;
        public Sprite rightStick;
        public Sprite rightStickX;
        public Sprite rightStickLeft;
        public Sprite rightStickRight;
        public Sprite rightStickY;
        public Sprite rightStickUp;
        public Sprite rightStickDown;
        public Sprite leftStickPress;
        public Sprite rightStickPress;
        public Sprite touchpadButtonPress;
        //public Sprite home;

        public Sprite GetGamepadSprite(string controlPath)
        {
            // From the input system, we get the path of the control on device. So we can just
            // map from that to the sprites we have for gamepads.
            return controlPath switch
            {
                "<Gamepad>/buttonNorth" => buttonNorth,
                "<Gamepad>/buttonSouth" => buttonSouth,
                "<Gamepad>/buttonWest" => buttonWest,
                "<Gamepad>/buttonEast" => buttonEast,
                "<Gamepad>/start" => startButton,
                "<Gamepad>/select" => selectButton,
                "<Gamepad>/leftTrigger" => leftTrigger,
                "<Gamepad>/rightTrigger" => rightTrigger,
                "<Gamepad>/leftShoulder" => leftShoulder,
                "<Gamepad>/rightShoulder" => rightShoulder,
                "<Gamepad>/dpad" => dpad,
                "<Gamepad>/dpad/up" => dpadUp,
                "<Gamepad>/dpad/down" => dpadDown,
                "<Gamepad>/dpad/left" => dpadLeft,
                "<Gamepad>/dpad/right" => dpadRight,
                "<Gamepad>/leftStick" => leftStick,
                "<Gamepad>/leftStick/x" => leftStickX,
                "<Gamepad>/leftStick/left" => leftStickLeft,
                "<Gamepad>/leftStick/right" => leftStickRight,
                "<Gamepad>/leftStick/y" => leftStickY,
                "<Gamepad>/leftStick/up" => leftStickUp,
                "<Gamepad>/leftStick/down" => leftStickDown,
                "<Gamepad>/rightStick" => rightStick,
                "<Gamepad>/rightStick/x" => rightStickX,
                "<Gamepad>/rightStick/left" => rightStickLeft,
                "<Gamepad>/rightStick/right" => rightStickRight,
                "<Gamepad>/rightStick/y" => rightStickY,
                "<Gamepad>/rightStick/up" => rightStickUp,
                "<Gamepad>/rightStick/down" => rightStickDown,
                "<Gamepad>/leftStickPress" => leftStickPress,
                "<Gamepad>/rightStickPress" => rightStickPress,
                _ => null,
            };
        }

        public Sprite GetDualShockGamepadSprite(string controlPath)
        {
            // From the input system, we get the path of the control on device. So we can just
            // map from that to the sprites we have for gamepads.
            switch (controlPath)
            {
                case "<DualShockGamepad>/touchpadButton":
                    return touchpadButtonPress;    //Until I get PS5 icons. If ever.
                default:
                    return null;
            }
        }

        public Sprite GetDualSenseGamepadHIDSprite(string controlPath)
        {
            // From the input system, we get the path of the control on device. So we can just
            // map from that to the sprites we have for gamepads.
            switch (controlPath)
            {
                case "<DualSenseGamepadHID>/leftTriggerButton":
                    return leftTrigger;
                case "<DualSenseGamepadHID>/rightTriggerButton":
                    return rightTrigger;
                //case "<DualSenseGamepadHID>/systemButton":
                //    return home;  //Disabled for ControlRebinds.
                //case "<DualSenseGamepadHID>/micButton":
                //    return dpad;  //Disabled for ControlRebinds. (And no icon available at the moment.)
                default:
                    return null;
            }
        }
    }
}