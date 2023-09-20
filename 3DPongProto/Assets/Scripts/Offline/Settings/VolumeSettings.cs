using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace ThreeDeePongProto.Offline.Settings
{
    public class VolumeSettings : MonoBehaviour
    {
        [SerializeField] private AudioMixer m_audioMixer;

        #region MasterVolume
        [Header("Master-Volume")]
        [SerializeField] private string m_masterVolumeParameter = "MasterVolume";
        [SerializeField] private Button m_quieterButtonMaster;
        [SerializeField] private Slider m_sliderMaster;
        [SerializeField] private Button m_louderButtonMaster;
        [SerializeField] private TextMeshProUGUI m_masterValueText;
        [SerializeField] private Toggle m_masterMute;
        [SerializeField] private float m_defaultMasterVolume = 1.0f;
        [SerializeField] private bool m_defaultMasterMuteIsOn = false;

        //public bool MasterMuteShowsActive { get => m_muteMasterCheckboxShowsActive; private set => m_muteMasterCheckboxShowsActive = value; }
        private bool m_muteMasterCheckboxShowsActive = false;
        #endregion

        #region BGMVolume
        [Header("BGM-Volume")]
        [SerializeField] private string m_bgmVolumeParameter = "BGMVolume";
        [SerializeField] private Button m_quieterButtonBGM;
        [SerializeField] private Slider m_sliderBGM;
        [SerializeField] private Button m_louderButtonBGM;
        [SerializeField] private TextMeshProUGUI m_bgmValueText;
        [SerializeField] private Toggle m_bGMMute;
        [SerializeField] private float m_defaultBGMVolume = 0.9f;
        [SerializeField] private bool m_defaultBGMMuteIsOn = false;
        //public bool BGMMuteShowsActive { get => m_muteBGMCheckboxShowsActive; private set => m_muteBGMCheckboxShowsActive = value; }
        private bool m_muteBGMCheckboxShowsActive = false;
        #endregion

        #region SFXVolume
        [Header("SFX-Volume")]
        [SerializeField] private string m_sfxVolumeParameter = "SFXVolume";
        [SerializeField] private Button m_quieterButtonSFX;
        [SerializeField] private Slider m_sliderSFX;
        [SerializeField] private Button m_louderButtonSFX;
        [SerializeField] private TextMeshProUGUI m_sfxValueText;
        [SerializeField] private Toggle m_sFXMute;
        [SerializeField] private float m_defaultSFXVolume = 1.0f;
        [SerializeField] private bool m_defaultSFXMuteIsOn = false;
        #endregion

        //public bool SFXMuteShowsActive { get => m_muteSFXCheckboxShowsActive; private set => m_muteSFXCheckboxShowsActive = value; }
        private bool m_muteSFXCheckboxShowsActive = false;

        #region Variables
        [Header("Variables")]
        //Mathf-Log(arithm)-Multiplier
        [SerializeField] private float m_adjustSliderStep = 0.05f;
        [SerializeField] private float m_logarithmMultiplier = 20f;
        [SerializeField] private float m_muteAmountVariable = -80f;
        #endregion

        [Space]
        [SerializeField] private Toggle[] m_muteToggle;
        [SerializeField] private Slider[] m_sliderType;
        private Dictionary<Toggle, Slider> m_toggleSliderConnection = new Dictionary<Toggle, Slider>();

        [SerializeField] private VolumeUIStates m_VolumeUIStates;
        [SerializeField] private VolumeUIValues m_volumeUIValues;

        private IPersistentData m_persistentData = new SerializingData();
        private bool m_encryptionEnabled = false;

        private void Awake()
        {
            //Connect the Toggles and Sliders for each VolumeGroup by a Dictionary (Key-Value-Pair).
            for (int i = 0; i < m_muteToggle.Length; i++)
                m_toggleSliderConnection.Add(m_muteToggle[i], m_sliderType[i]);
        }

        private void OnEnable()
        {
            //TODO: AddSliderAndToggleListener(); - LoadVolumeSettings();
            AddSliderAndToggleListener();
        }

        private void Start()
        {
            HandleMasterSliderValueChanges(m_volumeUIValues.LatestMasterVolume);
            HandleBGMSliderValueChanges(m_volumeUIValues.LatestBGMVolume);
            HandleSFXSliderValueChanges(m_volumeUIValues.LatestSFXVolume);
        }

        private void OnDisable()
        {
            //TODO: RemoveSliderAndToggleListener(); - SaveVolumeSettings();
            RemoveSliderAndToggleListener();

            m_persistentData.SaveData("/SaveData/UI-States", "/Volume", ".json", m_VolumeUIStates, m_encryptionEnabled, true);
            m_persistentData.SaveData("/SaveData/UI-Values", "/Volume", ".json", m_volumeUIValues, m_encryptionEnabled, true);
        }

        /// <summary>
        /// Subscribe VolumeControl-Elements to UnityEvents.
        /// </summary>
        #region Listener-Region
        private void AddSliderAndToggleListener()
        {
            m_sliderMaster.onValueChanged.AddListener(HandleMasterSliderValueChanges);
            m_sliderBGM.onValueChanged.AddListener(HandleBGMSliderValueChanges);
            m_sliderSFX.onValueChanged.AddListener(HandleSFXSliderValueChanges);

            m_masterMute.onValueChanged.AddListener(HandleMasterToggleValueChanges);
            m_bGMMute.onValueChanged.AddListener(HandleBGMToggleValueChanges);
            m_sFXMute.onValueChanged.AddListener(HandleSFXToggleValueChanges);
        }

        /// <summary>
        /// Unsubscribe VolumeControl-Elements from UnityEvents.
        /// </summary>
        private void RemoveSliderAndToggleListener()
        {
            m_sliderMaster.onValueChanged.RemoveListener(HandleMasterSliderValueChanges);
            m_sliderBGM.onValueChanged.RemoveListener(HandleBGMSliderValueChanges);
            m_sliderSFX.onValueChanged.RemoveListener(HandleSFXSliderValueChanges);

            m_masterMute.onValueChanged.RemoveListener(HandleMasterToggleValueChanges);
            m_bGMMute.onValueChanged.RemoveListener(HandleBGMToggleValueChanges);
            m_sFXMute.onValueChanged.RemoveListener(HandleSFXToggleValueChanges);
        }
        #endregion

        #region Listener-Methods
        #region SlidersChanges
        private void HandleMasterSliderValueChanges(float _value)
        {
            //Set MasterVolume-floats with the AudioMixer. (Also sets the Slider back to the saved amount on unmuting.)
            m_audioMixer.SetFloat(m_masterVolumeParameter, Mathf.Log10(_value) * m_logarithmMultiplier);

            m_muteMasterCheckboxShowsActive = false;
            //MasterMute is on, when the corresponding SliderValue is the minimalValue.
            m_masterMute.isOn = !(m_sliderMaster.value > m_sliderMaster.minValue);
            m_muteMasterCheckboxShowsActive = true;

            m_sliderMaster.value = _value;
            m_masterValueText.SetText($"{_value:P0}");

            //Also save the new value in the ScriptableObject.
            if (m_sliderMaster.value > m_sliderMaster.minValue)
                m_volumeUIValues.LatestMasterVolume = _value;
        }

        private void HandleBGMSliderValueChanges(float _value)
        {
            //Set BackgroundVolume-floats with the audioMixer. (Also sets the Slider back to the saved amount on unmuting.)
            m_audioMixer.SetFloat(m_bgmVolumeParameter, Mathf.Log10(_value) * m_logarithmMultiplier);

            m_muteBGMCheckboxShowsActive = false;
            //BGMMute is on, when the corresponding SliderValue is the minimalValue.
            m_bGMMute.isOn = !(m_sliderBGM.value > m_sliderBGM.minValue);
            m_muteBGMCheckboxShowsActive = true;

            m_sliderBGM.value = _value;
            m_bgmValueText.SetText($"{_value:P0}");

            //Also save the new value in the ScriptableObject.
            if (m_sliderBGM.value > m_sliderBGM.minValue)
                m_volumeUIValues.LatestBGMVolume = _value;
        }

        private void HandleSFXSliderValueChanges(float _value)
        {
            //Set SoundeffectsVolume-floats with the audioMixer. (Also sets the Slider back to the saved amount on unmuting.)
            m_audioMixer.SetFloat(m_sfxVolumeParameter, Mathf.Log10(_value) * m_logarithmMultiplier);

            m_muteSFXCheckboxShowsActive = false;
            //SFXMute is on, when the corresponding SliderValue is the minimalValue.
            m_sFXMute.isOn = !(m_sliderSFX.value > m_sliderSFX.minValue);
            m_muteSFXCheckboxShowsActive = true;

            m_sliderSFX.value = _value;
            m_sfxValueText.SetText($"{_value:P0}");

            //Also save the new value in the ScriptableObject.
            if (m_sliderSFX.value > m_sliderSFX.minValue)
                m_volumeUIValues.LatestSFXVolume = _value;
        }
        #endregion

        #region MuteChanges
        private void HandleMasterToggleValueChanges(bool _mute)
        {
            if (!m_muteMasterCheckboxShowsActive)
                return;

            //If we activate the Toggle to mute the MasterVolume.
            if (_mute)
            {
                //Tempsave the corresponding SliderValue in a ScriptableObject.
                m_volumeUIValues.LatestMasterVolume = m_sliderMaster.value;
                //Set Sliders 'minValue' as new value.
                m_sliderMaster.value = m_sliderMaster.minValue;
                //Use the memberVariable to reduce the audioMixer-VolumeChannel by that amount.
                m_audioMixer.SetFloat(m_masterVolumeParameter, m_muteAmountVariable);
            }
            else
            {
                m_sliderMaster.value = m_volumeUIValues.LatestMasterVolume;
            }
        }

        private void HandleBGMToggleValueChanges(bool _mute)
        {
            if (!m_muteBGMCheckboxShowsActive)
                return;

            //If we activate the Toggle to mute the BGMVolume.
            if (_mute)
            {
                //Tempsave the corresponding SliderValue in a ScriptableObject.
                m_volumeUIValues.LatestBGMVolume = m_sliderBGM.value;
                //Set Sliders 'minValue' as new value.
                m_sliderBGM.value = m_sliderBGM.minValue;
                //Use the memberVariable to reduce the audioMixer-VolumeChannel by that amount.
                m_audioMixer.SetFloat(m_bgmVolumeParameter, m_muteAmountVariable);
            }
            else
            {
                m_sliderBGM.value = m_volumeUIValues.LatestBGMVolume;
            }
        }

        private void HandleSFXToggleValueChanges(bool _mute)
        {
            if (!m_muteSFXCheckboxShowsActive)
                return;

            //If we activate the Toggle to mute the SFXVolume.
            if (_mute)
            {
                //Tempsave the corresponding SliderValue in a ScriptableObject.
                m_volumeUIValues.LatestSFXVolume = m_sliderSFX.value;
                //Set Sliders 'minValue' as new value.
                m_sliderSFX.value = m_sliderSFX.minValue;
                //Use the memberVariable to reduce the audioMixer-VolumeChannel by that amount.
                m_audioMixer.SetFloat(m_sfxVolumeParameter, m_muteAmountVariable);
            }
            else
            {
                m_sliderSFX.value = m_volumeUIValues.LatestSFXVolume;
            }
        }
        #endregion
        #endregion

        public void LowerSliderValue(Toggle _connectedMute)
        {
            //Get the corresponding Slider (Value) in the Dictionary, for each submitted Mute (Key), by the Button inside Unity.
            Slider connectedSlider = m_toggleSliderConnection[_connectedMute];

            //Only if the submitted Mute isn't on, then the Button can lower the SliderValue.
            if (!_connectedMute.isOn)
                connectedSlider.value -= m_adjustSliderStep;
        }

        public void IncreaseSliderValue(Toggle _connectedMute)
        {
            //Get the corresponding Slider (Value) in the Dictionary, for each submitted Mute (Key), by the Button inside Unity.
            Slider connectedSlider = m_toggleSliderConnection[_connectedMute];

            //Commented out, to allow the 'LouderButton' to increase the SliderValue out of the muted state.
            //if (!_connectedMute.isOn)
            connectedSlider.value += m_adjustSliderStep;
        }

        public void ReSetDefault()
        {
            m_masterMute.isOn = m_defaultMasterMuteIsOn;
            m_bGMMute.isOn = m_defaultBGMMuteIsOn;
            m_sFXMute.isOn = m_defaultSFXMuteIsOn;

            m_sliderMaster.value = m_defaultMasterVolume;
            m_sliderBGM.value = m_defaultBGMVolume;
            m_sliderSFX.value= m_defaultSFXVolume;
        }
    }
}