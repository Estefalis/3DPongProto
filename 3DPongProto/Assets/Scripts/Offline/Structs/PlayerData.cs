using System;
using UnityEngine.UI;

[Serializable]
public struct PlayerData
{
    public string PlayerName;
    public Image Avatar;
    public bool PlayerOnFrontline;
    public bool KeepNameOnLoad;
    public bool DefaultKeyboard;    //TODO: Implement a choice to prefer Keyboard and Mouse or Gamepad as main inputDevice.
}