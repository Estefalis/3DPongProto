using System;
using UnityEngine;

namespace ThreeDeePongProto.Shared.UI
{
    [Serializable]
    public struct GamepadIcons  //struct and Sprites NEVER readonly! Because that RESETS the button-assignments in the inspector! :(
    {
        public Sprite triangleNorth;
        public Sprite crossSouth;
        public Sprite squareWest;
        public Sprite circleEast;
        public Sprite startOptions;
        public Sprite createShare;
        public Sprite shoulderL1LB;
        public Sprite shoulderR1RB;
        public Sprite triggerL2LT;
        public Sprite triggerR2RT;
        public Sprite dpad;
        public Sprite dpadX;
        public Sprite dpadY;
        public Sprite dpadUp;
        public Sprite dpadDown;
        public Sprite dpadLeft;
        public Sprite dpadRight;
        public Sprite leftStick;
        public Sprite leftStickLeft;
        public Sprite leftStickRight;
        public Sprite lStickLeftRight;
        public Sprite leftStickUp;
        public Sprite leftStickDown;
        public Sprite lStickUpDown;
        public Sprite rightStick;
        public Sprite rightStickLeft;
        public Sprite rightStickRight;
        public Sprite rStickLeftRight;
        public Sprite rightStickUp;
        public Sprite rightStickDown;
        public Sprite rStickUpDown;
        public Sprite lStickClickL3;
        public Sprite rStickClickR3;
        public Sprite touchpadPress;
        //public Sprite home;

        public Sprite GetGamepadSprite(string controlPath)
        {
            // From the input system, we get the path of the control on device. So we can just
            // map from that to the sprites we have for gamepads.
            return controlPath switch
            {
                "<Gamepad>/buttonNorth" => triangleNorth,
                "<Gamepad>/buttonSouth" => crossSouth,
                "<Gamepad>/buttonWest" => squareWest,
                "<Gamepad>/buttonEast" => circleEast,
                "<Gamepad>/start" => startOptions,
                "<Gamepad>/select" => createShare,
                "<Gamepad>/leftTrigger" => triggerL2LT,
                "<Gamepad>/rightTrigger" => triggerR2RT,
                "<Gamepad>/leftShoulder" => shoulderL1LB,
                "<Gamepad>/rightShoulder" => shoulderR1RB,
                "<Gamepad>/dpad" => dpad,
                "<Gamepad>/dpad/x" => dpadX,
                "<Gamepad>/dpad/y" => dpadY,
                "<Gamepad>/dpad/up" => dpadUp,
                "<Gamepad>/dpad/down" => dpadDown,
                "<Gamepad>/dpad/left" => dpadLeft,
                "<Gamepad>/dpad/right" => dpadRight,
                "<Gamepad>/leftStick" => leftStick,
                "<Gamepad>/leftStick/x" => lStickLeftRight,
                "<Gamepad>/leftStick/left" => leftStickLeft,
                "<Gamepad>/leftStick/right" => leftStickRight,
                "<Gamepad>/leftStick/y" => lStickUpDown,
                "<Gamepad>/leftStick/up" => leftStickUp,
                "<Gamepad>/leftStick/down" => leftStickDown,
                "<Gamepad>/rightStick" => rightStick,
                "<Gamepad>/rightStick/x" => rStickLeftRight,
                "<Gamepad>/rightStick/left" => rightStickLeft,
                "<Gamepad>/rightStick/right" => rightStickRight,
                "<Gamepad>/rightStick/y" => rStickUpDown,
                "<Gamepad>/rightStick/up" => rightStickUp,
                "<Gamepad>/rightStick/down" => rightStickDown,
                "<Gamepad>/leftStickPress" => lStickClickL3,
                "<Gamepad>/rightStickPress" => rStickClickR3,
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
                    return touchpadPress;
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
                    return triggerL2LT;
                case "<DualSenseGamepadHID>/rightTriggerButton":
                    return triggerR2RT;
                case "<DualSenseGamepadHID>/touchpadButton":
                    return touchpadPress;
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