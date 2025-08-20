using System;

[Serializable]
public class PlayerProfileData
{
    public string PlayerName;
    public bool KeepNameOnLoad = false;
    public bool DefaultKeyboard = false;    //Prefered Device
    public bool InvertMoveAxisX = false;    //Axis Invertion X
    public bool InvertRotAxisY = false;     //Axis Invertion Y
    public bool GamepadVibration = false;

    //Constructor for standardValues.
    public PlayerProfileData(int _playerIndex)
    {
        PlayerName = $"Player {_playerIndex + 1}";
        KeepNameOnLoad = false;
        DefaultKeyboard = (_playerIndex == 0);   //set first player (Index 0) to default Keyboard( & Mouse)
        InvertMoveAxisX = false;
        InvertRotAxisY = false;
        GamepadVibration = false;
    }
}