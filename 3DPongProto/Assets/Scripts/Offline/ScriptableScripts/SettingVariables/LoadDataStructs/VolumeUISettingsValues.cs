using System;

namespace ThreeDeePongProto.Shared.Settings
{
    [Serializable]
    public struct VolumeUISettingsValues
    {
        public float LatestMasterVolume;
        public float LatestBGMVolume;
        public float LatestSFXVolume;
    }
}