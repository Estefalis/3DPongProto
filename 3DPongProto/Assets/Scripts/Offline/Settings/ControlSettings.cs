using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ThreeDeePongProto.Offline.Settings
{
    public class ControlSettings : MonoBehaviour
    {
        [Header("Axis Inversion")]
        [SerializeField] private Toggle m_xRotInvertToggle;
        [SerializeField] private Toggle m_yRotInvertToggle;
        [SerializeField] private bool m_xAxisInversion = false;
        [SerializeField] private bool m_yAxisInversion = false;

        [Header("Axis Sensitivity")]
        [SerializeField] private Toggle[] m_customToggleKeys;
        [SerializeField] private Slider[] m_sensitivitySliderValues;
        [Space]
        [SerializeField, Range(1, 20)] float m_xMovespeedDefault = 10f;
        [SerializeField, Range(1, 5)] float m_yRotationDefault = 2.5f;
        [SerializeField, Range(1, 10)] private float m_adjustSliderStep = 2.0f;
        [SerializeField] private TextMeshProUGUI m_xAxisValueText;
        [SerializeField] private TextMeshProUGUI m_yAxisValueText;
        [SerializeField] private bool m_customDefaults = false;

        [Header("Player Amount")]
        [SerializeField] private TMP_Dropdown m_playerDropdown;
        [SerializeField] private int m_playerIndex = 1;

        private Dictionary<Toggle, Slider> m_toggleSliderConnection = new Dictionary<Toggle, Slider>();

        #region Scriptable-References
        [Header("Scriptable Objects")]
        [SerializeField] private ControlUIStates m_controlUIStates;
        [SerializeField] private ControlUIValues m_controlUIValues;
        #endregion

        #region Serialization
        private readonly string m_settingsStatesFolderPath = "/SaveData/Settings-States";
        private readonly string m_settingsValuesFolderPath = "/SaveData/Settings-Values";
        private readonly string m_controlFileName = "/Control";
        private readonly string m_fileFormat = ".json";

        private IPersistentData m_persistentData = new SerializingData();
        private bool m_encryptionEnabled = false;
        #endregion

        private void Awake()
        {
            //Connect the Toggles and Sliders for each SensitivityGroup by a Dictionary (Key-Value-Pair).
            for (int i = 0; i < m_customToggleKeys.Length; i++)
                m_toggleSliderConnection.Add(m_customToggleKeys[i], m_sensitivitySliderValues[i]);

            if (m_controlUIStates == null || m_controlUIValues == null)
                ReSetDefault();
            //else LoadControlSettingsFromScriptableSave(); move to 'MenuOrganisation.cs'.!
        }

        private void OnEnable()
        {
            AddSliderAndToggleListener();
        }

        private void OnDisable()
        {
            RemoveSliderAndToggleListener();

            m_persistentData.SaveData(m_settingsStatesFolderPath, m_controlFileName, m_fileFormat, m_controlUIStates, m_encryptionEnabled, true);
            m_persistentData.SaveData(m_settingsValuesFolderPath, m_controlFileName, m_fileFormat, m_controlUIValues, m_encryptionEnabled, true);
        }

        private void Start()
        {
            //TODO: InitialUISetup and UpdateLineUpTMPs check for nulled Scriptables.
            InitialUISetup();
        }

        /// <summary>
        /// Subscribe VolumeControl-Elements to UnityEvents.
        /// </summary>
        #region UnRegister-Listener-Region
        private void AddSliderAndToggleListener()
        {
            m_xRotInvertToggle.onValueChanged.AddListener(XRotInversionChange);
            m_yRotInvertToggle.onValueChanged.AddListener(YRotInversionChange);

            m_sensitivitySliderValues[0].onValueChanged.AddListener(SensitivitySliderXValueChanges);
            m_sensitivitySliderValues[1].onValueChanged.AddListener(SensitivitySliderYValueChanges);

            m_customToggleKeys[0].onValueChanged.AddListener(CustomToggleXValueChanges);
            m_customToggleKeys[1].onValueChanged.AddListener(CustomToggleYValueChanges);
        }

        /// <summary>
        /// Unsubscribe VolumeControl-Elements from UnityEvents.
        /// </summary>
        private void RemoveSliderAndToggleListener()
        {
            m_sensitivitySliderValues[0].onValueChanged.RemoveListener(SensitivitySliderXValueChanges);
            m_sensitivitySliderValues[1].onValueChanged.RemoveListener(SensitivitySliderYValueChanges);

            m_customToggleKeys[0].onValueChanged.RemoveListener(CustomToggleXValueChanges);
            m_customToggleKeys[1].onValueChanged.RemoveListener(CustomToggleYValueChanges);
        }
        #endregion

        #region Listener-Methods
        private void XRotInversionChange(bool _xAxisInversion)
        {
            m_controlUIStates.InvertXAxis = _xAxisInversion;
        }

        private void YRotInversionChange(bool _yAxisInversion)
        {
            m_controlUIStates.InvertYAxis = _yAxisInversion;
        }

        private void SensitivitySliderXValueChanges(float _sliderXValue)
        {
            if (m_customToggleKeys[0].isOn)
            {
                m_controlUIValues.LastXMoveSpeed = _sliderXValue;
                m_xAxisValueText.text = $"{_sliderXValue:N2}";
            }
        }

        private void SensitivitySliderYValueChanges(float _sliderYValue)
        {
            if (m_customToggleKeys[1].isOn)
            {
                m_controlUIValues.LastYRotSpeed = _sliderYValue;
                m_yAxisValueText.text = $"{_sliderYValue:N2}";
            }
        }

        private void CustomToggleXValueChanges(bool _toggleX)
        {
            m_controlUIStates.CustomXSensitivity = _toggleX;

            switch (_toggleX)
            {
                case false:
                {
                    m_sensitivitySliderValues[0].value = m_xMovespeedDefault;
                    m_xAxisValueText.text = $"{m_xMovespeedDefault:N2}";
                    m_sensitivitySliderValues[0].interactable = false;
                    break;
                }
                case true:
                {
                    m_sensitivitySliderValues[0].value = m_controlUIValues.LastXMoveSpeed;
                    m_xAxisValueText.text = $"{m_controlUIValues.LastXMoveSpeed:N2}";
                    m_sensitivitySliderValues[0].interactable = true;
                    break;
                }
            }
        }

        private void CustomToggleYValueChanges(bool _toggleY)
        {
            m_controlUIStates.CustomYSensitivity = _toggleY;

            switch (_toggleY)
            {
                case false:
                {
                    m_controlUIValues.LastYRotSpeed = m_sensitivitySliderValues[1].value;
                    m_sensitivitySliderValues[1].value = m_yRotationDefault;
                    m_yAxisValueText.text = $"{m_yRotationDefault:N2}";
                    m_sensitivitySliderValues[1].interactable = false;
                    break;
                }
                case true:
                {
                    m_sensitivitySliderValues[1].value = m_controlUIValues.LastYRotSpeed;
                    m_yAxisValueText.text = $"{m_controlUIValues.LastYRotSpeed:N2}";
                    m_sensitivitySliderValues[1].interactable = true;
                    break;
                }
            }
        }
        #endregion

        #region Custom Methods
        private void InitialUISetup()
        {
            m_xRotInvertToggle.isOn = m_controlUIStates.InvertXAxis;
            m_yRotInvertToggle.isOn = m_controlUIStates.InvertYAxis;
            m_customToggleKeys[0].isOn = m_controlUIStates.CustomXSensitivity;
            m_customToggleKeys[1].isOn = m_controlUIStates.CustomYSensitivity;
            m_playerDropdown.value = m_controlUIStates.ShownPlayerIndex;

            switch (m_controlUIStates.CustomXSensitivity)
            {
                case false:
                {
                    m_sensitivitySliderValues[0].value = m_xMovespeedDefault;
                    m_xAxisValueText.text = $"{m_xMovespeedDefault:N2}";
                    break;
                }
                case true:
                    SensitivitySliderXValueChanges(m_controlUIValues.LastXMoveSpeed);
                    break;
            }

            switch (m_controlUIStates.CustomYSensitivity)
            {
                case false:
                {
                    m_sensitivitySliderValues[1].value = m_yRotationDefault;
                    m_yAxisValueText.text = $"{m_yRotationDefault:N2}";
                    break;
                }
                case true:
                    SensitivitySliderYValueChanges(m_controlUIValues.LastYRotSpeed);
                    break;
            }
        }

        public void LowerSliderValue(Toggle _connectedToggle)
        {
            //Get the corresponding Slider (Value) in the Dictionary, for each submitted Mute (Key), by the Button inside Unity.
            Slider connectedSlider = m_toggleSliderConnection[_connectedToggle];

            //Only if the submitted Mute isn't on, then the Button can lower the SliderValue.
            if (_connectedToggle.isOn)
                connectedSlider.value -= connectedSlider.maxValue * (0.01f * m_adjustSliderStep);
        }

        public void IncreaseSliderValue(Toggle _connectedToggle)
        {
            //Get the corresponding Slider (Value) in the Dictionary, for each submitted Mute (Key), by the Button inside Unity.
            Slider connectedSlider = m_toggleSliderConnection[_connectedToggle];

            //Commented out, to allow the 'LouderButton' to increase the SliderValue out of the muted state.
            if (_connectedToggle.isOn)
                connectedSlider.value += connectedSlider.maxValue * (0.01f * m_adjustSliderStep);
        }

        public void ReSetDefault()
        {
            m_xRotInvertToggle.isOn = m_xAxisInversion;
            m_yRotInvertToggle.isOn = m_yAxisInversion;
            m_customToggleKeys[0].isOn = m_customDefaults;
            m_customToggleKeys[1].isOn = m_customDefaults;

            m_playerDropdown.value = m_playerIndex;
            m_sensitivitySliderValues[0].value = m_xMovespeedDefault;
            m_sensitivitySliderValues[1].value = m_yRotationDefault;
        }
        #endregion
    }
}