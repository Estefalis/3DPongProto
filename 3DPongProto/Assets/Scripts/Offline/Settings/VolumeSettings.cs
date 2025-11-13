using ThreeDeePongProto.Shared.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace ThreeDeePongProto.Shared.Settings
{
    public class VolumeSettings : MonoBehaviour
    {
        public enum VolumeType { Master, BGM, SFX }

        [Header("System References")]
        [SerializeField] private AudioMixer m_audioMixer;

        #region MasterVolume
        [Header("UI Reference")]
        [SerializeField] private Slider m_masterSlider;
        [SerializeField] private Toggle m_masterMuteToggle;
        [SerializeField] private TextMeshProUGUI m_masterValueText;

        [SerializeField] private Slider m_bgmSlider;
        [SerializeField] private Toggle m_bgmMuteToggle;
        [SerializeField] private TextMeshProUGUI m_bgmValueText;

        [SerializeField] private Slider m_sfxSlider;
        [SerializeField] private Toggle m_sfxMuteToggle;
        [SerializeField] private TextMeshProUGUI m_sfxValueText;
        #endregion

        private VolumeSettingsData m_volumeData;

        #region Audio-Mixer_Constants
        private const float MUTE_VOLUME_DB = -80f;
        private const float LOG_MULTIPLIER = 20f;
        private const string MASTER_PARAM = "MasterVolume";
        private const string BGM_PARAM = "BGMVolume";
        private const string SFX_PARAM = "SFXVolume";
        #endregion

        [SerializeField, Range(0.1f, 10.0f)] private float m_adjustSliderStep = 1.0f;

        #region Public Button Methods
        public void DecreaseMaster() => AdjustVolume(VolumeType.Master, false);
        public void IncreaseMaster() => AdjustVolume(VolumeType.Master, true);

        public void DecreaseBGM() => AdjustVolume(VolumeType.BGM, false);
        public void IncreaseBGM() => AdjustVolume(VolumeType.BGM, true);

        public void DecreaseSFX() => AdjustVolume(VolumeType.SFX, false);
        public void IncreaseSFX() => AdjustVolume(VolumeType.SFX, true);
        #endregion

        private void OnEnable()
        {
            m_volumeData = SettingsManager.Instance.CurrentSettings.Volume;

            SettingsManager.Instance.OnSettingsChanged += UpdateUIAndAudio;
            SetUIElements();    //Includes AddListeners();
            SetMixerValues();
        }

        private void OnDisable()
        {
            SettingsManager.Instance.OnSettingsChanged -= UpdateUIAndAudio;
            RemoveListeners();
            SettingsManager.Instance.SaveSettings();
        }

        /// <summary>
        /// Subscribe VolumeControl-Elements to UnityEvents.
        /// </summary>
        #region UnRegister-Listener-Region
        private void AddListeners()
        {
            m_masterSlider.onValueChanged.AddListener(OnMasterSliderChanged);
            m_bgmSlider.onValueChanged.AddListener(OnBGMSliderChanged);
            m_sfxSlider.onValueChanged.AddListener(OnSFXSliderChanged);

            m_masterMuteToggle.onValueChanged.AddListener(OnMasterToggleChanged);
            m_bgmMuteToggle.onValueChanged.AddListener(OnBGMToggleChanged);
            m_sfxMuteToggle.onValueChanged.AddListener(OnSFXToggleOnChanged);
        }

        /// <summary>
        /// Unsubscribe VolumeControl-Elements from UnityEvents.
        /// </summary>
        private void RemoveListeners()
        {
            m_masterSlider.onValueChanged.RemoveListener(OnMasterSliderChanged);
            m_bgmSlider.onValueChanged.RemoveListener(OnBGMSliderChanged);
            m_sfxSlider.onValueChanged.RemoveListener(OnSFXSliderChanged);

            m_masterMuteToggle.onValueChanged.RemoveListener(OnMasterToggleChanged);
            m_bgmMuteToggle.onValueChanged.RemoveListener(OnBGMToggleChanged);
            m_sfxMuteToggle.onValueChanged.RemoveListener(OnSFXToggleOnChanged);
        }
        #endregion

        #region Listener Methods
        #region SlidersChanges
        private void OnMasterSliderChanged(float _value)
        {
            m_volumeData.MasterVolume = _value;
            //Slider > minValue = unmuted.
            if (m_volumeData.IsMasterMuted && _value > m_masterSlider.minValue)
            {
                m_volumeData.IsMasterMuted = false;
                m_volumeData.MasterVolumeUnMuted = _value;
            }

            UpdateUIAndAudio();
        }

        private void OnBGMSliderChanged(float _value)
        {
            m_volumeData.BgmVolume = _value;
            //Slider > minValue = unmuted.
            if (m_volumeData.IsBgmMuted && _value > m_bgmSlider.minValue)
            {
                m_volumeData.IsBgmMuted = false;
                m_volumeData.BgmVolumeUnMuted = _value;
            }

            UpdateUIAndAudio();
        }

        private void OnSFXSliderChanged(float _value)
        {
            m_volumeData.SfxVolume = _value;
            //Slider > minValue = unmuted.
            if (m_volumeData.IsSfxMuted && _value > m_sfxSlider.minValue)
            {
                m_volumeData.IsSfxMuted = false;
                m_volumeData.SfxVolumeUnMuted = _value;
            }

            UpdateUIAndAudio();
        }
        #endregion

        #region Mute Listener Changes
        /// <summary>
        /// Sets and Unsets All Slider to their minValue/latestSavedValue and Toggles on/off.
        /// </summary>
        /// <param name="_isMuted"></param>
        private void OnMasterToggleChanged(bool _isMuted)
        {
            m_volumeData.IsMasterMuted = _isMuted;
            //Save current sliderValue, if muteToggle is set to true.
            if (_isMuted)
            {
                m_volumeData.MasterVolumeUnMuted = m_volumeData.MasterVolume;
            }

            UpdateUIAndAudio();
        }

        /// <summary>
        /// (Un)Sets BGM-&MasterVolume minValues/savedValues & Toggles on/off.
        /// </summary>
        /// <param name="_isMuted"></param>
        private void OnBGMToggleChanged(bool _isMuted)
        {
            m_volumeData.IsBgmMuted = _isMuted;
            //Save current sliderValue, if muteToggle is set to true.
            if (_isMuted)
            {
                m_volumeData.BgmVolumeUnMuted = m_volumeData.BgmVolume;
            }

            UpdateUIAndAudio();
        }

        /// <summary>
        /// (Un)Sets Diegetic-&MasterVolume minValues/savedValues & Toggles on/off.
        /// </summary>
        /// <param name="_isMuted"></param>
        private void OnSFXToggleOnChanged(bool _isMuted)
        {
            m_volumeData.IsSfxMuted = _isMuted;
            //Save current sliderValue, if muteToggle is set to true.
            if (_isMuted)
            {
                m_volumeData.BgmVolumeUnMuted = m_volumeData.BgmVolume;
            }

            UpdateUIAndAudio();
        }
        #endregion
        #endregion

        #region Custom Methods
        private void SetUIElements()
        {
            RemoveListeners();

            m_masterMuteToggle.isOn = m_volumeData.IsMasterMuted;
            m_masterSlider.value = m_volumeData.IsMasterMuted ? m_masterSlider.minValue : m_volumeData.MasterVolume;
            m_masterValueText.text = $"{m_masterSlider.value:P0}";

            m_bgmMuteToggle.isOn = m_volumeData.IsBgmMuted;
            var masterOrBgmMuted = m_volumeData.IsMasterMuted ? m_volumeData.IsMasterMuted : m_volumeData.IsBgmMuted;
            m_bgmSlider.value = masterOrBgmMuted ? m_bgmSlider.minValue : m_volumeData.BgmVolume;
            m_bgmValueText.text = $"{m_bgmSlider.value:P0}";

            m_sfxMuteToggle.isOn = m_volumeData.IsSfxMuted;
            var masterOrSfxMuted = m_volumeData.IsMasterMuted ? m_volumeData.IsMasterMuted : m_volumeData.IsSfxMuted;
            m_sfxSlider.value = masterOrSfxMuted ? m_sfxSlider.minValue : m_volumeData.SfxVolume;
            m_sfxValueText.text = $"{m_sfxSlider.value:P0}";

            AddListeners();
        }

        private void SetMixerValues()
        {
            SetMixerVolume(MASTER_PARAM, m_volumeData.MasterVolume, m_volumeData.IsMasterMuted);
            //Sub-channels are muted if they are muted OR if the master is muted.
            SetMixerVolume(BGM_PARAM, m_volumeData.BgmVolume, m_volumeData.IsBgmMuted || m_volumeData.IsMasterMuted);
            SetMixerVolume(SFX_PARAM, m_volumeData.SfxVolume, m_volumeData.IsSfxMuted || m_volumeData.IsMasterMuted);
        }

        private void SetMixerVolume(string _parameter, float _volume, bool _isMuted)
        {
            //If Master is muted, all other will be also.
            if (_parameter != MASTER_PARAM && m_volumeData.IsMasterMuted)
            {
                _isMuted = true;
            }

            float dbValue = _isMuted ? MUTE_VOLUME_DB : Mathf.Log10(_volume) * LOG_MULTIPLIER;
            m_audioMixer.SetFloat(_parameter, dbValue);
        }

        private void UpdateUIAndAudio()
        {
            SetUIElements();
            SetMixerValues();
        }

        private void AdjustVolume(VolumeType _type, bool _increase)
        {
            Slider targetSlider = null;
            switch (_type)
            {
                case VolumeType.Master:
                    targetSlider = m_masterSlider;
                    break;
                case VolumeType.BGM:
                    targetSlider = m_bgmSlider;
                    break;
                case VolumeType.SFX:
                    targetSlider = m_sfxSlider;
                    break;
                default:
                    break;
            }

            if (targetSlider != null)
            {
                float absoluteStep = targetSlider.maxValue * (m_adjustSliderStep / 100.0f); //1.0f means a sliderStep of 1%.
                float step = _increase ? absoluteStep : -absoluteStep;
                targetSlider.value += step; //Triggers OnValueChanged-Listener automaticly.
            }
        }

        //Public method for the general ResetButton of VolumeSettings.
        public void ResetSettings()
        {
            SettingsManager.Instance.ResetVolumeSettings();
        }
        #endregion
    }
}