using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace ThreeDeePongProto.Offline.Settings
{
    public class VolumeSettings : MonoBehaviour
    {
        private enum EMuteMode { BGM, SFX, Master, None }

        [SerializeField] private AudioMixer m_audioMixer;

        #region MasterVolume
        [Header("Master-Volume")]
        [SerializeField] private string m_masterVolumeParameter = "MasterVolume";
        [SerializeField] private Button m_quieterButtonMaster;
        [SerializeField] private Button m_louderButtonMaster;
        [SerializeField] private TextMeshProUGUI m_masterValueText;
        [SerializeField] private float m_defaultMasterVolume = 1.0f;
        [SerializeField] private bool m_defaultMasterMuteIsOn = false;

        //public bool MasterMuteShowsActive { get => m_muteMasterCheckboxShowsActive; private set => m_muteMasterCheckboxShowsActive = value; }
        private bool m_muteMasterCheckboxShowsActive = false;
        #endregion

        #region BGMVolume
        [Header("BGM-Volume")]
        [SerializeField] private string m_bgmVolumeParameter = "BGMVolume";
        [SerializeField] private Button m_quieterButtonBGM;
        [SerializeField] private Button m_louderButtonBGM;
        [SerializeField] private TextMeshProUGUI m_bgmValueText;
        [SerializeField] private float m_defaultBGMVolume = 0.9f;
        [SerializeField] private bool m_defaultBGMMuteIsOn = false;
        //public bool BGMMuteShowsActive { get => m_muteBGMCheckboxShowsActive; private set => m_muteBGMCheckboxShowsActive = value; }
        private bool m_muteBGMCheckboxShowsActive = false;
        #endregion

        #region SFXVolume
        [Header("SFX-Volume")]
        [SerializeField] private string m_sfxVolumeParameter = "SFXVolume";
        [SerializeField] private Button m_quieterButtonSFX;
        [SerializeField] private Button m_louderButtonSFX;
        [SerializeField] private TextMeshProUGUI m_sfxValueText;
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
        [SerializeField] private Toggle[] m_muteToggleKeys;
        [SerializeField] private Slider[] m_volumeSliderValues;
        private Dictionary<Toggle, Slider> m_toggleSliderConnection = new Dictionary<Toggle, Slider>();

        [SerializeField] private VolumeUIStates m_volumeUIStates;
        [SerializeField] private VolumeUIValues m_volumeUIValues;

        #region Serialization
        private readonly string m_settingsStatesFolderPath = "/SaveData/Settings-States";
        private readonly string m_settingsValuesFolderPath = "/SaveData/Settings-Values";
        private readonly string m_volumeFileName = "/Volume";
        private readonly string m_fileFormat = ".json";

        private IPersistentData m_persistentData = new SerializingData();
        private bool m_encryptionEnabled = false;
        #endregion

        private void Awake()
        {
            //Connect the Toggles and Sliders for each VolumeGroup by a Dictionary (Key-Value-Pair).
            for (int i = 0; i < m_muteToggleKeys.Length; i++)
                m_toggleSliderConnection.Add(m_muteToggleKeys[i], m_volumeSliderValues[i]);

            if (m_volumeUIStates == null || m_volumeUIValues == null)
                ReSetDefault();
            //else
            //    LoadVolumeSettings();
        }

        private void OnEnable()
        {
            AddSliderAndToggleListener();
        }

        private void Start()
        {
            InitialUISetup();
        }

        //private void LoadVolumeSettings()
        //{
        //    VolumeUISettingsStates uiIndices = m_persistentData.LoadData<VolumeUISettingsStates>(m_settingsStatesFolderPath, m_volumeFileName, m_fileFormat, m_encryptionEnabled);
        //    VolumeUISettingsValues uiValues = m_persistentData.LoadData<VolumeUISettingsValues>(m_SettingsValueFolderPath, m_volumeFileName, m_fileFormat, m_encryptionEnabled);

        //    m_volumeUIStates.MasterMuteIsOn = uiIndices.MasterMuteIsOn;
        //    m_volumeUIStates.BGMMuteIsOn = uiIndices.BGMMuteIsOn;
        //    m_volumeUIStates.SFXMuteIsOn = uiIndices.SFXMuteIsOn;

        //    m_volumeUIValues.LatestMasterVolume = uiValues.LatestMasterVolume;
        //    m_volumeUIValues.LatestBGMVolume = uiValues.LatestBGMVolume;
        //    m_volumeUIValues.LatestSFXVolume = uiValues.LatestSFXVolume;
        //}

        private void InitialUISetup()
        {
            switch (m_volumeUIStates.MasterMuteIsOn)
            {
                case true:
                    HandleMasterSliderValueChanges(m_volumeSliderValues[0].minValue);
                    break;
                case false:
                    HandleMasterSliderValueChanges(m_volumeUIValues.LatestMasterVolume);
                    break;
            }

            switch (m_volumeUIStates.BGMMuteIsOn)
            {
                case true:
                    HandleBGMSliderValueChanges(m_volumeSliderValues[1].minValue);
                    break;
                case false:
                    HandleBGMSliderValueChanges(m_volumeUIValues.LatestBGMVolume);
                    break;
            }

            switch (m_volumeUIStates.SFXMuteIsOn)
            {
                case true:
                    HandleSFXSliderValueChanges(m_volumeSliderValues[2].minValue);
                    break;
                case false:
                    HandleSFXSliderValueChanges(m_volumeUIValues.LatestSFXVolume);
                    break;
            }
        }

        private void OnDisable()
        {
            RemoveSliderAndToggleListener();

            m_persistentData.SaveData(m_settingsStatesFolderPath, m_volumeFileName, m_fileFormat, m_volumeUIStates, m_encryptionEnabled, true);
            m_persistentData.SaveData(m_settingsValuesFolderPath, m_volumeFileName, m_fileFormat, m_volumeUIValues, m_encryptionEnabled, true);
        }

        /// <summary>
        /// Subscribe VolumeControl-Elements to UnityEvents.
        /// </summary>
        #region Listener-Region
        private void AddSliderAndToggleListener()
        {
            m_volumeSliderValues[0].onValueChanged.AddListener(HandleMasterSliderValueChanges);
            m_volumeSliderValues[1].onValueChanged.AddListener(HandleBGMSliderValueChanges);
            m_volumeSliderValues[2].onValueChanged.AddListener(HandleSFXSliderValueChanges);

            m_muteToggleKeys[0].onValueChanged.AddListener(HandleMasterToggleValueChanges);
            m_muteToggleKeys[1].onValueChanged.AddListener(HandleBGMToggleValueChanges);
            m_muteToggleKeys[2].onValueChanged.AddListener(HandleSFXToggleValueChanges);
        }

        /// <summary>
        /// Unsubscribe VolumeControl-Elements from UnityEvents.
        /// </summary>
        private void RemoveSliderAndToggleListener()
        {
            m_volumeSliderValues[0].onValueChanged.RemoveListener(HandleMasterSliderValueChanges);
            m_volumeSliderValues[1].onValueChanged.RemoveListener(HandleBGMSliderValueChanges);
            m_volumeSliderValues[2].onValueChanged.RemoveListener(HandleSFXSliderValueChanges);

            m_muteToggleKeys[0].onValueChanged.RemoveListener(HandleMasterToggleValueChanges);
            m_muteToggleKeys[1].onValueChanged.RemoveListener(HandleBGMToggleValueChanges);
            m_muteToggleKeys[2].onValueChanged.RemoveListener(HandleSFXToggleValueChanges);
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
            m_muteToggleKeys[0].isOn = !(m_volumeSliderValues[0].value > m_volumeSliderValues[0].minValue);
            m_muteMasterCheckboxShowsActive = true;

            m_volumeSliderValues[0].value = _value;
            m_masterValueText.SetText($"{_value:P0}");

            //Also save the new value in the ScriptableObject.
            if (m_volumeSliderValues[0].value > m_volumeSliderValues[0].minValue)
                m_volumeUIValues.LatestMasterVolume = _value;

            //Saves the connected MuteState only if the value isn't equal.
            if (m_muteToggleKeys[0].isOn != m_volumeUIStates.MasterMuteIsOn)
                m_volumeUIStates.MasterMuteIsOn = m_muteToggleKeys[0].isOn;
        }

        private void HandleBGMSliderValueChanges(float _value)
        {
            //Set BackgroundVolume-floats with the audioMixer. (Also sets the Slider back to the saved amount on unmuting.)
            m_audioMixer.SetFloat(m_bgmVolumeParameter, Mathf.Log10(_value) * m_logarithmMultiplier);

            m_muteBGMCheckboxShowsActive = false;
            //BGMMute is on, when the corresponding SliderValue is the minimalValue.
            m_muteToggleKeys[1].isOn = !(m_volumeSliderValues[1].value > m_volumeSliderValues[1].minValue);
            m_muteBGMCheckboxShowsActive = true;

            m_volumeSliderValues[1].value = _value;
            m_bgmValueText.SetText($"{_value:P0}");

            //Also save the new value in the ScriptableObject.
            if (m_volumeSliderValues[1].value > m_volumeSliderValues[1].minValue)
                m_volumeUIValues.LatestBGMVolume = _value;

            //Saves the connected MuteState only if the value isn't equal.
            if (m_muteToggleKeys[1].isOn != m_volumeUIStates.BGMMuteIsOn)
                m_volumeUIStates.BGMMuteIsOn = m_muteToggleKeys[1].isOn;
        }

        private void HandleSFXSliderValueChanges(float _value)
        {
            //Set SoundeffectsVolume-floats with the audioMixer. (Also sets the Slider back to the saved amount on unmuting.)
            m_audioMixer.SetFloat(m_sfxVolumeParameter, Mathf.Log10(_value) * m_logarithmMultiplier);

            m_muteSFXCheckboxShowsActive = false;
            //SFXMute is on, when the corresponding SliderValue is the minimalValue.
            m_muteToggleKeys[2].isOn = !(m_volumeSliderValues[2].value > m_volumeSliderValues[2].minValue);
            m_muteSFXCheckboxShowsActive = true;

            m_volumeSliderValues[2].value = _value;
            m_sfxValueText.SetText($"{_value:P0}");

            //Also save the new value in the ScriptableObject.
            if (m_volumeSliderValues[2].value > m_volumeSliderValues[2].minValue)
                m_volumeUIValues.LatestSFXVolume = _value;

            //Saves the connected MuteState only if the value isn't equal.
            if (m_muteToggleKeys[2].isOn != m_volumeUIStates.SFXMuteIsOn)
                m_volumeUIStates.SFXMuteIsOn = m_muteToggleKeys[2].isOn;
        }
        #endregion

        #region MuteChanges
        /// <summary>
        /// Sets and Unsets All Slider to their minValue/lastestSavedValue and Toggles on/off.
        /// </summary>
        /// <param name="_mute"></param>
        private void HandleMasterToggleValueChanges(bool _mute)
        {
            if (!m_muteMasterCheckboxShowsActive)
                return;

            //If we activate the Toggle to mute the MasterVolume.
            if (_mute)
            {
                //Tempsave the corresponding SliderValue in a ScriptableObject.
                m_volumeUIValues.LatestMasterVolume = m_volumeSliderValues[0].value;
                //Set Sliders 'minValue' as new value, while muting.
                m_volumeSliderValues[0].value = m_volumeSliderValues[0].minValue;
                m_volumeSliderValues[1].value = m_volumeSliderValues[1].minValue;
                m_volumeSliderValues[2].value = m_volumeSliderValues[2].minValue;
                //Use the memberVariable to reduce the audioMixer-VolumeChannel by that amount.
                m_audioMixer.SetFloat(m_masterVolumeParameter, m_muteAmountVariable);
            }
            else
            {
                m_volumeSliderValues[0].value = m_volumeUIValues.LatestMasterVolume;
                m_volumeSliderValues[1].value = m_volumeUIValues.LatestBGMVolume;
                m_volumeSliderValues[2].value = m_volumeUIValues.LatestSFXVolume;
            }
        }

        /// <summary>
        /// (Un)Sets BGM-&MasterVolume minValues/savedValues & Toggles on/off.
        /// </summary>
        /// <param name="_mute"></param>
        private void HandleBGMToggleValueChanges(bool _mute)
        {
            if (!m_muteBGMCheckboxShowsActive)
                return;

            //If we activate the Toggle to mute the BGMVolume.
            if (_mute)
            {
                //Tempsave the corresponding SliderValue in a ScriptableObject.
                m_volumeUIValues.LatestBGMVolume = m_volumeSliderValues[1].value;
                //Set Sliders 'minValue' as new value, while muting.
                m_volumeSliderValues[1].value = m_volumeSliderValues[1].minValue;
                //Use the memberVariable to reduce the audioMixer-VolumeChannel by that amount.
                m_audioMixer.SetFloat(m_bgmVolumeParameter, m_muteAmountVariable);
            }
            else
            {
                m_volumeSliderValues[0].value = m_volumeUIValues.LatestMasterVolume;
                m_volumeSliderValues[1].value = m_volumeUIValues.LatestBGMVolume;
            }
        }

        /// <summary>
        /// (Un)Sets SFX-&MasterVolume minValues/savedValues & Toggles on/off.
        /// </summary>
        /// <param name="_mute"></param>
        private void HandleSFXToggleValueChanges(bool _mute)
        {
            if (!m_muteSFXCheckboxShowsActive)
                return;

            //If we activate the Toggle to mute the SFXVolume.
            if (_mute)
            {
                //Tempsave the corresponding SliderValue in a ScriptableObject.
                m_volumeUIValues.LatestSFXVolume = m_volumeSliderValues[2].value;
                //Set Sliders 'minValue' as new value, while muting.
                m_volumeSliderValues[2].value = m_volumeSliderValues[2].minValue;
                //Use the memberVariable to reduce the audioMixer-VolumeChannel by that amount.
                m_audioMixer.SetFloat(m_sfxVolumeParameter, m_muteAmountVariable);
            }
            else
            {
                m_volumeSliderValues[0].value = m_volumeUIValues.LatestMasterVolume;
                m_volumeSliderValues[2].value = m_volumeUIValues.LatestSFXVolume;
            }
        }
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
        #endregion

        #region Non-Listener-Methods
        public void ReSetDefault()
        {
            m_muteToggleKeys[0].isOn = m_defaultMasterMuteIsOn;
            m_muteToggleKeys[1].isOn = m_defaultBGMMuteIsOn;
            m_muteToggleKeys[2].isOn = m_defaultSFXMuteIsOn;

            m_volumeSliderValues[0].value = m_defaultMasterVolume;
            m_volumeSliderValues[1].value = m_defaultBGMVolume;
            m_volumeSliderValues[2].value = m_defaultSFXVolume;
        }
        #endregion
    }
}