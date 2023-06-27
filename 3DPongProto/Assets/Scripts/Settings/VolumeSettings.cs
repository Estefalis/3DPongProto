using System.Collections.Generic;
using ThreeDeePongProto.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace ThreeDeePongProto.Settings
{
    public class VolumeSettings : MonoBehaviour
    {
        [SerializeField] private AudioMixer m_audioMixer;
        [SerializeField] private VolumeVariables m_volumeVariables;

        #region MasterVolume
        [Header("Master-Volume")]
        [SerializeField] private string m_masterVolumeParameter = "MasterVolume";
        [SerializeField] private Button m_quieterButtonMaster;
        [SerializeField] private Slider m_sliderMaster;
        [SerializeField] private Button m_louderButtonMaster;
        [SerializeField] private Toggle m_muteMaster;
        #endregion

        #region BGMVolume
        [Header("BGM-Volume")]
        [SerializeField] private string m_bgmVolumeParameter = "BGMVolume";
        [SerializeField] private Button m_quieterButtonBGM;
        [SerializeField] private Slider m_sliderBGM;
        [SerializeField] private Button m_louderButtonBGM;
        [SerializeField] private Toggle m_muteBGM;
        #endregion

        #region SFXVolume
        [Header("SFX-Volume")]
        [SerializeField] private string m_sfxVolumeParameter = "SFXVolume";
        [SerializeField] private Button m_quieterButtonSFX;
        [SerializeField] private Slider m_sliderSFX;
        [SerializeField] private Button m_louderButtonSFX;
        [SerializeField] private Toggle m_muteSFX;
        #endregion

        #region Variables
        [Header("Variables")]
        //Mathf-Log(arithm)-Multiplier
        [SerializeField] private float m_adjustSliderStep = 0.05f;
        [SerializeField] private float m_logarithmMultiplier = 20f;
        [SerializeField] private float m_muteAmountVariable = -80f;
        #endregion

        private void OnEnable()
        {
            //TODO: AddSliderAndToggleListener(); - LoadVolumeSettings();
        }

        private void OnDisable()
        {
            //TODO: RemoveSliderAndToggleListener(); - SaveVolumeSettings();
        }

        public void LowerSliderValue(Slider _sliderType)
        {
            switch (_sliderType.name)
            {
                case "SliderMasterVolume":
                    LowerSliderByType(m_sliderMaster, m_muteMaster);
                    break;
                case "SliderBGMVolume":
                    LowerSliderByType(m_sliderBGM, m_muteBGM);
                    break;
                case "SliderSFXVolume":
                    LowerSliderByType(m_sliderSFX, m_muteSFX);
                    break;
            }
        }

        public void IncreaseSliderValue(Slider _sliderType)
        {
            switch (_sliderType.name)
            {
                case "SliderMasterVolume":
                    IncreaseSliderByType(m_sliderMaster, m_muteMaster);
                    break;
                case "SliderBGMVolume":
                    IncreaseSliderByType(m_sliderBGM, m_muteBGM);
                    break;
                case "SliderSFXVolume":
                    IncreaseSliderByType(m_sliderSFX, m_muteSFX);
                    break;
            }
        }

        private void LowerSliderByType(Slider _sliderType, Toggle _mute)
        {
            if (!_mute.isOn)
                _sliderType.value -= m_adjustSliderStep;
        }

        private void IncreaseSliderByType(Slider _sliderType, Toggle _mute)
        {
            if (!_mute.isOn)
                _sliderType.value += m_adjustSliderStep;
        }
    }
}