using System;

[Serializable]
public class VolumeSettingsData
{
    public float MasterVolume = 1.0f;
    public bool IsMasterMuted = false;

    public float BgmVolume = 0.9f;
    public bool IsBgmMuted = false;

    public float SfxVolume = 0.9f;
    public bool IsSfxMuted = false;

    [NonSerialized] public float MasterVolumeUnMuted = 1.0f;
    [NonSerialized] public float BgmVolumeUnMuted = 0.9f;
    [NonSerialized] public float SfxVolumeUnMuted = 0.9f;
}
