using System;
using UnityEngine;

namespace ThreeDeePongProto.Shared.Managers
{
    public class SettingsManager : PersistentSingleton<SettingsManager>
    {
        //Holds the currently active game settings to access via SettingsManager.Instance.CurrentSettings.
        public GameSettingsData CurrentSettings { get; private set; }

        public int MaxRounds { get => m_maxRounds; }
        public int MaxRoundPoints { get => m_maxPointsEachRound; }
        private const int m_maxRounds = 5, m_maxPointsEachRound = 25;

        private static readonly string m_fileName = "gameSettings.json";
        private const string m_volume = "Volume", m_graphic = "Graphic", m_control = "Control", m_match = "Match", m_network = "Network";

        public event Action OnSettingsChanged;

        private IPersistentData<GameSettingsData> m_saveSystem;

        protected override void Awake()
        {
            base.Awake();

            m_saveSystem = new SerializingData<GameSettingsData>(m_fileName);   //StoreData Save System.

            LoadSettings();
        }

        private void LoadSettings()
        {
            CurrentSettings = m_saveSystem.Load();
        }

        public void SaveSettings()
        {
            m_saveSystem.Save(CurrentSettings);

            //Notifiy the UI to update itself.
            OnSettingsChanged?.Invoke();
        }

        public void ResetVolumeSettings()
        {
            CurrentSettings.Volume = new VolumeSettingsData();
            ResetByCategory(m_volume);
        }

        public void ResetGraphicSettings()
        {
            CurrentSettings.Graphic = new GraphicSettingsData();
            ResetByCategory(m_graphic);
        }

        public void ResetControlSettings()
        {
            CurrentSettings.Control = new ControlSettingsData();
            ResetByCategory(m_control);
        }

        public void ResetMatchSettings()
        {
            CurrentSettings.Match = new MatchSettingsData();
            ResetByCategory(m_match);
        }

        private void ResetByCategory(string _categorie)
        {
            SaveSettings(); //Save the latest settings.
#if UNITY_EDITOR
            Debug.Log($"{_categorie} settings has been reset to default.");
#endif
        }
    }
}