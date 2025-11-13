using UnityEngine.InputSystem;

namespace ThreeDeePongProto.Shared.HelperClasses
{
    internal static class GetDeviceHelper
    {
        private const string m_keyboardMouseScheme = "KeyboardMouse", m_keyboardSchemePID0 = "KeyboardPlayerID0", m_keyboardSchemePID1 = "KeyboardPlayerID1", m_keyboardSchemePID2 = "KeyboardPlayerID2", m_keyboardSchemePID3 = "KeyboardPlayerID3";
        private const string m_gamePadScheme = "Gamepad"/*, m_gamePadSchemePID0 = "GamepadPlayerID0", m_gamePadSchemePID1 = "GamepadPlayerID1", m_gamePadSchemePID2 = "GamepadPlayerID2", m_gamePadSchemePID3 = "GamepadPlayerID3"*/;

        internal static void GetPlayerControlScheme(int _playerIndex, InputDevice _assignedDevice, out string _controlScheme, out InputDevice[] devices)
        {
            if (_assignedDevice is Gamepad gamepad)
            {
                //_controlScheme = _playerIndex switch
                //{
                //    0 => m_gamePadSchemePID0,
                //    1 => m_gamePadSchemePID1,
                //    2 => m_gamePadSchemePID2,
                //    3 => m_gamePadSchemePID3,
                //    _ => m_gamePadScheme
                //};

                _controlScheme = m_gamePadScheme;
                devices = new InputDevice[] { gamepad };
            }
            else
            {
                _controlScheme = _playerIndex switch
                {
                    0 => m_keyboardSchemePID0,
                    1 => m_keyboardSchemePID1,
                    2 => m_keyboardSchemePID2,
                    3 => m_keyboardSchemePID3,
                    _ => m_keyboardMouseScheme
                };

                //_controlScheme = m_keyboardMouseScheme;
                devices = new InputDevice[] { Keyboard.current, Mouse.current };
            }
        }
    }
}