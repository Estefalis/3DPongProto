using System;

namespace ThreeDeePongProto.Offline.Settings
{
    [Serializable]
    public struct VolumeUISettingsValues
    {
        public float LatestMasterVolume;
        public float LatestBGMVolume;
        public float LatestSFXVolume;
    }
}