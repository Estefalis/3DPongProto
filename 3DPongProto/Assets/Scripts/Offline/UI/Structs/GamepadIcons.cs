using System;
using UnityEngine;

namespace ThreeDeePongProto.Offline.UI
{
    [Serializable]
    public struct GamepadIcons
    {
        public Sprite buttonNorth;
        public Sprite buttonSouth;
        public Sprite buttonWest;
        public Sprite buttonEast;
        public Sprite startButton;
        public Sprite selectButton;
        public Sprite leftTrigger;
        public Sprite rightTrigger;
        public Sprite leftShoulder;
        public Sprite rightShoulder;
        public Sprite dpad;
        public Sprite dpadUp;
        public Sprite dpadDown;
        public Sprite dpadLeft;
        public Sprite dpadRight;
        public Sprite leftStick;
        public Sprite leftStickX;
        public Sprite leftStickY;
        public Sprite rightStick;
        public Sprite rightStickX;
        public Sprite rightStickY;
        public Sprite leftStickPress;
        public Sprite rightStickPress;

        public Sprite GetGamepadSprite(string controlPath)
        {
            // From the input system, we get the path of the control on device. So we can just
            // map from that to the sprites we have for gamepads.
            switch (controlPath)
            {
                case "<Gamepad>/buttonNorth":
                    return buttonNorth;
                case "<Gamepad>/buttonSouth":
                    return buttonSouth;
                case "<Gamepad>/buttonWest":
                    return buttonWest;
                case "<Gamepad>/buttonEast":
                    return buttonEast;
                case "<Gamepad>/start":
                    return startButton;
                case "<Gamepad>/select":
                    return selectButton;
                case "<Gamepad>/leftTrigger":
                    return leftTrigger;
                case "<Gamepad>/rightTrigger":
                    return rightTrigger;
                case "<Gamepad>/leftShoulder":
                    return leftShoulder;
                case "<Gamepad>/rightShoulder":
                    return rightShoulder;
                case "<Gamepad>/dpad":
                    return dpad;
                case "<Gamepad>/dpad/up":
                    return dpadUp;
                case "<Gamepad>/dpad/down":
                    return dpadDown;
                case "<Gamepad>/dpad/left":
                    return dpadLeft;
                case "<Gamepad>/dpad/right":
                    return dpadRight;
                case "<Gamepad>/leftStick":
                    return leftStick;
                case "<Gamepad>/leftStick/x":
                case "<Gamepad>/leftStick/left":
                case "<Gamepad>/leftStick/right":
                    return leftStickX;
                case "<Gamepad>/leftStick/y":
                case "<Gamepad>/leftStick/up":
                case "<Gamepad>/leftStick/down":
                    return leftStickY;
                case "<Gamepad>/rightStick":
                    return rightStick;
                case "<Gamepad>/rightStick/x":
                case "<Gamepad>/rightStick/left":
                case "<Gamepad>/rightStick/right":
                    return rightStickX;
                case "<Gamepad>/rightStick/y":
                case "<Gamepad>/rightStick/up":
                case "<Gamepad>/rightStick/down":
                    return rightStickY;
                case "<Gamepad>/leftStickPress":
                    return leftStickPress;
                case "<Gamepad>/rightStickPress":
                    return rightStickPress;
                default:
                    return null;
            }
        }

        public Sprite GetDualShockGamepadSprite(string controlPath)
        {
            // From the input system, we get the path of the control on device. So we can just
            // map from that to the sprites we have for gamepads.
            switch (controlPath)
            {
                case "<DualShockGamepad>/touchpadButton":
                    return dpad;    //Until I get PS5 icons. If ever.
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
                case "<DualSenseGamepadHID>/systemButton":
                    return dpad;    //Until I get PS5 icons. If ever.
                case "<DualSenseGamepadHID>/micButton":
                    return dpad;    //Until I get PS5 icons. If ever.
                default:
                    return null;
            }
        }
    }
}