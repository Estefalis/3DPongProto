using System;

[Serializable]
public struct PlayerData
{
    public string PrefabName;
    public string PlayerName;
    public int PlayerId;
    public string Avatar;
    public bool KeepNameOnLoad;
    public bool PlayerOnFrontline;
    public bool DefaultKeyboard;    //TODO: Implement a choice to prefer Keyboard and Mouse or Gamepad as main inputDevice.
    public int ToggleID;

    public PlayerData(string _prefabName, string _playerName, int _playerID, string _avatarName, bool _keepNameOnLoad, bool _playerOnFrontLine, bool _defaultKeyboard, int _toggleID)
    {
        PrefabName = _prefabName;
        PlayerName = _playerName;
        PlayerId = _playerID;
        Avatar = _avatarName;
        KeepNameOnLoad = _keepNameOnLoad;
        PlayerOnFrontline = _playerOnFrontLine;
        DefaultKeyboard = _defaultKeyboard;
        ToggleID = _toggleID;
    }
}