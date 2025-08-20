using System;

[Serializable]
public class GameSettingsData
{
    public VolumeSettingsData Volume = new();
    public GraphicSettingsData Graphic = new();
    public ControlSettingsData Control = new();
    public MatchSettingsData Match = new();
    //public NetworkSettingsData Network = new();
}