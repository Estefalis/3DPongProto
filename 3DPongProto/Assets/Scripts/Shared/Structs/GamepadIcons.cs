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

        public Sprite GetGamepadSprite(string _controlPath)
        {
            if (_controlPath.EndsWith("/buttonNorth")) return triangleNorth;
            if (_controlPath.EndsWith("/buttonSouth")) return crossSouth;
            if (_controlPath.EndsWith("/buttonWest")) return squareWest;
            if (_controlPath.EndsWith("/buttonEast")) return circleEast;

            if (_controlPath.EndsWith("/start")) return startOptions;
            if (_controlPath.EndsWith("/select")) return createShare;

            if (_controlPath.EndsWith("/leftTrigger")) return triggerL2LT;
            if (_controlPath.EndsWith("/rightTrigger")) return triggerR2RT;
            if (_controlPath.EndsWith("/leftShoulder")) return shoulderL1LB;
            if (_controlPath.EndsWith("/rightShoulder")) return shoulderR1RB;

            if (_controlPath.EndsWith("/dpad")) return dpad;
            if (_controlPath.EndsWith("/dpad/up")) return dpadUp;
            if (_controlPath.EndsWith("/dpad/down")) return dpadDown;
            if (_controlPath.EndsWith("/dpad/left")) return dpadLeft;
            if (_controlPath.EndsWith("/dpad/right")) return dpadRight;

            if (_controlPath.EndsWith("/leftStick")) return leftStick;
            if (_controlPath.EndsWith("/leftStick/x")) return lStickLeftRight;
            if (_controlPath.EndsWith("/leftStick/left")) return leftStickLeft;
            if (_controlPath.EndsWith("/leftStick/right")) return leftStickRight;
            if (_controlPath.EndsWith("/leftStick/y")) return lStickUpDown;
            if (_controlPath.EndsWith("/leftStick/up")) return leftStickUp;
            if (_controlPath.EndsWith("/leftStick/down")) return leftStickDown;
            if (_controlPath.EndsWith("/leftStickPress")) return lStickClickL3;

            if (_controlPath.EndsWith("/rightStick")) return rightStick;
            if (_controlPath.EndsWith("/rightStick/x")) return rStickLeftRight;
            if (_controlPath.EndsWith("/rightStick/left")) return rightStickLeft;
            if (_controlPath.EndsWith("/rightStick/right")) return rightStickRight;
            if (_controlPath.EndsWith("/rightStick/y")) return rStickUpDown;
            if (_controlPath.EndsWith("/rightStick/up")) return rightStickUp;
            if (_controlPath.EndsWith("/rightStick/down")) return rightStickDown;
            if (_controlPath.EndsWith("/rightStickPress")) return rStickClickR3;
            else
            return null;
        }
    }
}