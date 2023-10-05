using System;

namespace ThreeDeePongProto.Offline.Settings
{
    [Serializable]
    public struct VolumeUISettingsStates
    {
        public bool MasterMuteIsOn;
        public bool BGMMuteIsOn;
        public bool SFXMuteIsOn;
    }
}