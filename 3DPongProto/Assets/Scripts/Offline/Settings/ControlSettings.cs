using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ThreeDeePongProto.Offline.Settings
{
    public class ControlSettings : MonoBehaviour
    {
        #region Content Views
        [Header("Content Views")]
        [SerializeField] private Button[] m_playerButtons;
        [SerializeField] private Transform[] m_contentSubTransforms;
        [SerializeField] private Transform[] m_menuButtonTransforms;
        [SerializeField, Range(0.1f, 0.9f)] private float m_reducedAlphaValue = 0.5f;
        [SerializeField, Range(0.5f, 1f)] private float m_maxAlphaValue = 1f;

        private int m_currentViewIndex;
        public static event Action<int> PlayerViewIndex;
        #endregion

        #region Axis Inversion
        [Header("Axis Inversion")]
        [SerializeField] private Toggle[] m_playerXRotInvertToggles;
        [SerializeField] private Toggle[] m_playerYRotInvertToggles;

        [SerializeField] private bool[] m_xRotInvertDefaults;
        [SerializeField] private bool[] m_yRotInvertDefaults;
        [Space]
        #endregion

        #region Axis Sensitivity
        [Header("Axis Sensitivity")]
        [SerializeField] private Slider[] m_MoveValueSliderXEP;
        [SerializeField] private Slider[] m_RotValueSliderYEP;
        [SerializeField] private TextMeshProUGUI[] m_xAxisValueTextPs;
        [SerializeField] private TextMeshProUGUI[] m_yAxisValueTextPs;
        [SerializeField] private Toggle[] m_moveToggleKeysEP;
        [SerializeField] private Toggle[] m_rotToggleKeysEP;
        [SerializeField] private bool[] m_customToggleDefaults;
        [Space]
        [SerializeField, Range(1, 20)] float m_xMovespeedDefault = 10f;
        [SerializeField, Range(1, 5)] float m_yRotationDefault = 2.5f;
        [SerializeField, Range(1, 10)] private float m_adjustSliderStep = 2.0f;

        private Dictionary<Toggle, Slider> m_toggleSliderConnectXEP = new Dictionary<Toggle, Slider>();
        private Dictionary<Toggle, Slider> m_toggleSliderConnectYEP = new Dictionary<Toggle, Slider>();

        #region A Dictionary-Array of Dictionaries. INTERESTING enough to keep it.
        //private Dictionary<Toggle, Slider>[] m_toggleSliderXYConnectEP = new Dictionary<Toggle, Slider>[]
        //{
        //    new Dictionary<Toggle, Slider>(),
        //    new Dictionary<Toggle, Slider>(),
        //    new Dictionary<Toggle, Slider>(),
        //    new Dictionary<Toggle, Slider>()
        //};
        #endregion
        #endregion

        #region Scriptable-References
        [Header("Scriptable Objects")]
        [SerializeField] private ControlUIStates[] m_controlUIStatesEP; //Old: ControlUIStates m_controlUIStates;
        [SerializeField] private ControlUIValues[] m_controlUIValuesEP; //Old: ControlUIValues m_controlUIValues;
        #endregion

        #region Serialization
        private readonly string m_settingsStatesFolderPath = "/SaveData/Settings-States";
        private readonly string m_settingsValuesFolderPath = "/SaveData/Settings-Values";
        private readonly string m_controlFileName = "/ControlPlayer";
        private readonly string m_fileFormat = ".json";

        private readonly IPersistentData m_persistentData = new SerializingData();
        private readonly bool m_encryptionEnabled = false;
        #endregion

        private void Awake()
        {
            m_currentViewIndex = 0;
            SetActivePlayerView(m_playerButtons[m_currentViewIndex]);

            for (int i = 0; i < m_contentSubTransforms.Length; i++)
            {
                if (m_contentSubTransforms[i] != null)
                {
                    m_toggleSliderConnectXEP.Add(m_moveToggleKeysEP[i], m_MoveValueSliderXEP[i]);
                    m_toggleSliderConnectYEP.Add(m_rotToggleKeysEP[i], m_RotValueSliderYEP[i]);
                }
            }

            for (int i = 0; i < m_contentSubTransforms.Length; i++)
            {
                if (m_controlUIStatesEP[i] == null || m_controlUIValuesEP[i] == null)
                {
                    m_currentViewIndex = i;
                    ReSetDefault();
                }
            }
            //else LoadControlSettingsFromScriptableSave(); move to 'MenuOrganisation.cs'.!
        }

        private void OnEnable()
        {
            InitialUISetup();
            AddSliderAndToggleListener();
        }

        private void OnDisable()
        {
            RemoveSliderAndToggleListener();

            for (int i = 0; i < m_controlUIStatesEP.Length; i++)
            {
                m_persistentData.SaveData(m_settingsStatesFolderPath, m_controlFileName + $"{i}", m_fileFormat, m_controlUIStatesEP[i], m_encryptionEnabled, true);
            }

            for (int j = 0; j < m_controlUIValuesEP.Length; j++)
            {
                m_persistentData.SaveData(m_settingsValuesFolderPath, m_controlFileName + $"{j}", m_fileFormat, m_controlUIValuesEP[j], m_encryptionEnabled, true);
            }
        }

        /// <summary>
        /// Subscribe VolumeControl-Elements to UnityEvents.
        /// </summary>
        #region UnRegister-Listener-Region
        private void AddSliderAndToggleListener()
        {
            for (int i = 0; i < m_controlUIStatesEP.Length; i++)
            {
                if (m_controlUIStatesEP[i] != null)
                {
                    m_playerXRotInvertToggles[i].onValueChanged.AddListener(XRotInversionChange);
                    m_playerYRotInvertToggles[i].onValueChanged.AddListener(YRotInversionChange);
                    m_moveToggleKeysEP[i].onValueChanged.AddListener(MoveToggleXValueChanges);
                    m_rotToggleKeysEP[i].onValueChanged.AddListener(RotToggleYValueChanges);
                }
            }

            for (int j = 0; j < m_controlUIValuesEP.Length; j++)
            {
                if (m_controlUIValuesEP[j] != null)
                {
                    m_MoveValueSliderXEP[j].onValueChanged.AddListener(SensitivitySliderXValueChanges);
                    m_RotValueSliderYEP[j].onValueChanged.AddListener(SensitivitySliderYValueChanges);
                }
            }

            m_playerXRotInvertToggles[0].onValueChanged.AddListener(XRotInversionChange);
            m_playerXRotInvertToggles[1].onValueChanged.AddListener(XRotInversionChange);
            m_playerXRotInvertToggles[2].onValueChanged.AddListener(XRotInversionChange);
            m_playerXRotInvertToggles[3].onValueChanged.AddListener(XRotInversionChange);

            m_playerYRotInvertToggles[0].onValueChanged.AddListener(YRotInversionChange);
            m_playerYRotInvertToggles[1].onValueChanged.AddListener(YRotInversionChange);
            m_playerYRotInvertToggles[2].onValueChanged.AddListener(YRotInversionChange);
            m_playerYRotInvertToggles[3].onValueChanged.AddListener(YRotInversionChange);
        }

        /// <summary>
        /// Unsubscribe VolumeControl-Elements from UnityEvents.
        /// </summary>
        private void RemoveSliderAndToggleListener()
        {
            for (int i = 0; i < m_controlUIStatesEP.Length; i++)
            {
                if (m_controlUIStatesEP[i] != null)
                {
                    m_playerXRotInvertToggles[i].onValueChanged.RemoveListener(XRotInversionChange);
                    m_playerYRotInvertToggles[i].onValueChanged.RemoveListener(YRotInversionChange);
                    m_moveToggleKeysEP[i].onValueChanged.RemoveListener(MoveToggleXValueChanges);
                    m_rotToggleKeysEP[i].onValueChanged.RemoveListener(RotToggleYValueChanges);
                }
            }

            for (int j = 0; j < m_controlUIValuesEP.Length; j++)
            {
                if (m_controlUIValuesEP[j] != null)
                {
                    m_MoveValueSliderXEP[j].onValueChanged.RemoveListener(SensitivitySliderXValueChanges);
                    m_RotValueSliderYEP[j].onValueChanged.RemoveListener(SensitivitySliderYValueChanges);
                }
            }

            m_playerXRotInvertToggles[0].onValueChanged.RemoveListener(XRotInversionChange);
            m_playerXRotInvertToggles[1].onValueChanged.RemoveListener(XRotInversionChange);
            m_playerXRotInvertToggles[2].onValueChanged.RemoveListener(XRotInversionChange);
            m_playerXRotInvertToggles[3].onValueChanged.RemoveListener(XRotInversionChange);

            m_playerYRotInvertToggles[0].onValueChanged.RemoveListener(YRotInversionChange);
            m_playerYRotInvertToggles[1].onValueChanged.RemoveListener(YRotInversionChange);
            m_playerYRotInvertToggles[2].onValueChanged.RemoveListener(YRotInversionChange);
            m_playerYRotInvertToggles[3].onValueChanged.RemoveListener(YRotInversionChange);
        }
        #endregion

        #region Listener-Methods
        private void XRotInversionChange(bool _xAxisInversion)
        {
            switch (m_currentViewIndex)
            {
                case 0:
                    m_controlUIStatesEP[0].InvertXAxis = _xAxisInversion;
                    break;
                case 1:
                    m_controlUIStatesEP[1].InvertXAxis = _xAxisInversion;
                    break;
                case 2:
                    m_controlUIStatesEP[2].InvertXAxis = _xAxisInversion;
                    break;
                case 3:
                    m_controlUIStatesEP[3].InvertXAxis = _xAxisInversion;
                    break;
            }
        }

        private void YRotInversionChange(bool _yAxisInversion)
        {
            switch (m_currentViewIndex)
            {
                case 0:
                    m_controlUIStatesEP[0].InvertYAxis = _yAxisInversion;
                    break;
                case 1:
                    m_controlUIStatesEP[1].InvertYAxis = _yAxisInversion;
                    break;
                case 2:
                    m_controlUIStatesEP[2].InvertYAxis = _yAxisInversion;
                    break;
                case 3:
                    m_controlUIStatesEP[3].InvertYAxis = _yAxisInversion;
                    break;
            }
        }

        private void SensitivitySliderXValueChanges(float _sliderXValue)
        {
            switch (m_currentViewIndex)
            {
                case 0:
                    if (m_moveToggleKeysEP[0].isOn)   //Player1 XToggle!
                    {
                        m_controlUIValuesEP[0].LastXMoveSpeed = _sliderXValue;  //Player1 SO.
                        m_xAxisValueTextPs[0].text = $"{_sliderXValue:N2}";     //Player1 XText.
                    }
                    break;
                case 1:
                    if (m_moveToggleKeysEP[1].isOn)   //Player2 XToggle!
                    {
                        m_controlUIValuesEP[1].LastXMoveSpeed = _sliderXValue;  //Player2 SO.
                        m_xAxisValueTextPs[1].text = $"{_sliderXValue:N2}";     //Player2 XText.
                    }
                    break;
                case 2:
                    if (m_moveToggleKeysEP[2].isOn)   //Player3 XToggle!
                    {
                        m_controlUIValuesEP[2].LastXMoveSpeed = _sliderXValue;  //Player3 SO.
                        m_xAxisValueTextPs[2].text = $"{_sliderXValue:N2}";     //Player3 XText.
                    }
                    break;
                case 3:
                    if (m_moveToggleKeysEP[3].isOn)   //Player4 XToggle!
                    {
                        m_controlUIValuesEP[3].LastXMoveSpeed = _sliderXValue;  //Player4 SO.
                        m_xAxisValueTextPs[3].text = $"{_sliderXValue:N2}";     //Player4 XText.
                    }
                    break;
                default:
                    break;
            }
        }

        private void SensitivitySliderYValueChanges(float _sliderYValue)
        {
            switch (m_currentViewIndex)
            {
                case 0:
                    if (m_rotToggleKeysEP[0].isOn)   //Player1 YToggle!
                    {
                        m_controlUIValuesEP[0].LastYRotSpeed = _sliderYValue;  //Player1 SO.
                        m_yAxisValueTextPs[0].text = $"{_sliderYValue:N2}";    //Player1 YText.
                    }
                    break;
                case 1:
                    if (m_rotToggleKeysEP[1].isOn)   //Player2 YToggle!
                    {
                        m_controlUIValuesEP[1].LastYRotSpeed = _sliderYValue;
                        m_yAxisValueTextPs[1].text = $"{_sliderYValue:N2}";
                    }
                    break;
                case 2:
                    if (m_rotToggleKeysEP[2].isOn)   //Player3 YToggle!
                    {
                        m_controlUIValuesEP[2].LastYRotSpeed = _sliderYValue;
                        m_yAxisValueTextPs[2].text = $"{_sliderYValue:N2}";
                    }
                    break;
                case 3:
                    if (m_rotToggleKeysEP[3].isOn)      //Player4 YToggle!
                    {
                        m_controlUIValuesEP[3].LastYRotSpeed = _sliderYValue;
                        m_yAxisValueTextPs[3].text = $"{_sliderYValue:N2}";
                    }
                    break;
                default:
                    break;
            }
        }

        private void MoveToggleXValueChanges(bool _toggleX)
        {
            switch (m_currentViewIndex)
            {
                case 0: //Player 1
                {
                    m_controlUIStatesEP[0].CustomXSensitivity = _toggleX;

                    switch (_toggleX)
                    {
                        case false:
                        {
                            m_MoveValueSliderXEP[0].value = m_xMovespeedDefault; //MoveSlider X
                            m_xAxisValueTextPs[0].text = $"{m_xMovespeedDefault:N2}";
                            m_MoveValueSliderXEP[0].interactable = false;
                            break;
                        }
                        case true:
                        {
                            m_MoveValueSliderXEP[0].value = m_controlUIValuesEP[0].LastXMoveSpeed;
                            m_xAxisValueTextPs[0].text = $"{m_controlUIValuesEP[0].LastXMoveSpeed:N2}";
                            m_MoveValueSliderXEP[0].interactable = true;
                            break;
                        }
                    }
                    break;
                }
                case 1: //Player 2
                {
                    m_controlUIStatesEP[1].CustomXSensitivity = _toggleX;

                    switch (_toggleX)
                    {
                        case false:
                        {
                            m_MoveValueSliderXEP[1].value = m_xMovespeedDefault; //MoveSlider X
                            m_xAxisValueTextPs[1].text = $"{m_xMovespeedDefault:N2}";
                            m_MoveValueSliderXEP[1].interactable = false;
                            break;
                        }
                        case true:
                        {
                            m_MoveValueSliderXEP[1].value = m_controlUIValuesEP[1].LastXMoveSpeed;
                            m_xAxisValueTextPs[1].text = $"{m_controlUIValuesEP[1].LastXMoveSpeed:N2}";
                            m_MoveValueSliderXEP[1].interactable = true;
                            break;
                        }
                    }
                    break;
                }
                case 2: //Player 3
                {
                    m_controlUIStatesEP[2].CustomXSensitivity = _toggleX;

                    switch (_toggleX)
                    {
                        case false:
                        {
                            m_MoveValueSliderXEP[2].value = m_xMovespeedDefault; //MoveSlider X
                            m_xAxisValueTextPs[2].text = $"{m_xMovespeedDefault:N2}";
                            m_MoveValueSliderXEP[2].interactable = false;
                            break;
                        }
                        case true:
                        {
                            m_MoveValueSliderXEP[2].value = m_controlUIValuesEP[2].LastXMoveSpeed;
                            m_xAxisValueTextPs[2].text = $"{m_controlUIValuesEP[2].LastXMoveSpeed:N2}";
                            m_MoveValueSliderXEP[2].interactable = true;
                            break;
                        }
                    }
                    break;
                }
                case 3: //Player 4
                {
                    m_controlUIStatesEP[3].CustomXSensitivity = _toggleX;

                    switch (_toggleX)
                    {
                        case false:
                        {
                            m_MoveValueSliderXEP[3].value = m_xMovespeedDefault; //MoveSlider X
                            m_xAxisValueTextPs[3].text = $"{m_xMovespeedDefault:N2}";
                            m_MoveValueSliderXEP[3].interactable = false;
                            break;
                        }
                        case true:
                        {
                            m_MoveValueSliderXEP[3].value = m_controlUIValuesEP[3].LastXMoveSpeed;
                            m_xAxisValueTextPs[3].text = $"{m_controlUIValuesEP[3].LastXMoveSpeed:N2}";
                            m_MoveValueSliderXEP[3].interactable = true;
                            break;
                        }
                    }
                    break;
                }
                default:
                    break;
            }
        }

        private void RotToggleYValueChanges(bool _toggleY)
        {
            switch (m_currentViewIndex)
            {
                case 0: //Player 1
                {
                    m_controlUIStatesEP[0].CustomYSensitivity = _toggleY;

                    switch (_toggleY)
                    {
                        case false:
                        {
                            m_RotValueSliderYEP[0].value = m_yRotationDefault;  //RotSlider Y
                            m_yAxisValueTextPs[0].text = $"{m_yRotationDefault:N2}";
                            m_RotValueSliderYEP[0].interactable = false;
                            break;
                        }
                        case true:
                        {
                            m_RotValueSliderYEP[0].value = m_controlUIValuesEP[0].LastYRotSpeed;
                            m_yAxisValueTextPs[0].text = $"{m_controlUIValuesEP[0].LastYRotSpeed:N2}";
                            m_RotValueSliderYEP[0].interactable = true;
                            break;
                        }
                    }
                    break;
                }
                case 1: //Player 2
                {
                    m_controlUIStatesEP[1].CustomYSensitivity = _toggleY;

                    switch (_toggleY)
                    {
                        case false:
                        {
                            m_RotValueSliderYEP[1].value = m_yRotationDefault;  //RotSlider Y
                            m_yAxisValueTextPs[1].text = $"{m_yRotationDefault:N2}";
                            m_RotValueSliderYEP[1].interactable = false;
                            break;
                        }
                        case true:
                        {
                            m_RotValueSliderYEP[1].value = m_controlUIValuesEP[1].LastYRotSpeed;
                            m_yAxisValueTextPs[1].text = $"{m_controlUIValuesEP[1].LastYRotSpeed:N2}";
                            m_RotValueSliderYEP[1].interactable = true;
                            break;
                        }
                    }
                    break;
                }
                case 2: //Player 3
                {
                    m_controlUIStatesEP[2].CustomYSensitivity = _toggleY;

                    switch (_toggleY)
                    {
                        case false:
                        {
                            m_RotValueSliderYEP[2].value = m_yRotationDefault;  //RotSlider Y
                            m_yAxisValueTextPs[2].text = $"{m_yRotationDefault:N2}";
                            m_RotValueSliderYEP[2].interactable = false;
                            break;
                        }
                        case true:
                        {
                            m_RotValueSliderYEP[2].value = m_controlUIValuesEP[2].LastYRotSpeed;
                            m_yAxisValueTextPs[2].text = $"{m_controlUIValuesEP[2].LastYRotSpeed:N2}";
                            m_RotValueSliderYEP[2].interactable = true;
                            break;
                        }
                    }
                    break;
                }
                case 3: //Player 4
                {
                    m_controlUIStatesEP[3].CustomYSensitivity = _toggleY;

                    switch (_toggleY)
                    {
                        case false:
                        {
                            m_RotValueSliderYEP[3].value = m_yRotationDefault;
                            m_yAxisValueTextPs[3].text = $"{m_yRotationDefault:N2}";
                            m_RotValueSliderYEP[3].interactable = false;
                            break;
                        }
                        case true:
                        {
                            m_RotValueSliderYEP[3].value = m_controlUIValuesEP[3].LastYRotSpeed;
                            m_yAxisValueTextPs[3].text = $"{m_controlUIValuesEP[3].LastYRotSpeed:N2}";
                            m_RotValueSliderYEP[3].interactable = true;
                            break;
                        }
                    }
                    break;
                }
                default:
                    break;
            }
        }

        public void SetActivePlayerView(Button _sender)
        {
            for (int i = 0; i < m_playerButtons.Length; i++)
            {
                if (_sender == m_playerButtons[i])
                {
                    m_contentSubTransforms[i].gameObject.SetActive(true);
                    m_menuButtonTransforms[i].gameObject.SetActive(true);
                    Color tempAlpha1 = m_playerButtons[i].image.color;
                    tempAlpha1.a = m_maxAlphaValue;
                    m_playerButtons[i].image.color = tempAlpha1;
                    m_currentViewIndex = i; //Routes the Default Button Resets.
                    PlayerViewIndex?.Invoke(m_currentViewIndex);
                }
                else
                {
                    m_contentSubTransforms[i].gameObject.SetActive(false);
                    m_menuButtonTransforms[i].gameObject.SetActive(false);
                    Color tempAlpha05 = m_playerButtons[i].image.color;
                    tempAlpha05.a = m_reducedAlphaValue;
                    m_playerButtons[i].image.color = tempAlpha05;
                }
            }
        }
        #endregion

        #region Custom Methods
        private void InitialUISetup()
        {
            for (int i = 0; i < m_contentSubTransforms.Length; i++)
            {
                if (m_contentSubTransforms[i] != null)
                {
                    m_playerXRotInvertToggles[i].isOn = m_controlUIStatesEP[i].InvertXAxis;
                    m_playerYRotInvertToggles[i].isOn = m_controlUIStatesEP[i].InvertYAxis;
                    m_moveToggleKeysEP[i].isOn = m_controlUIStatesEP[i].CustomXSensitivity;
                    m_rotToggleKeysEP[i].isOn = m_controlUIStatesEP[i].CustomYSensitivity;

                    switch (m_controlUIStatesEP[i].CustomXSensitivity)
                    {
                        case false:
                        {
                            m_MoveValueSliderXEP[i].value = m_xMovespeedDefault;    //MoveSlider X
                            m_xAxisValueTextPs[i].text = $"{m_xMovespeedDefault:N2}";
                            break;
                        }
                        case true:
                        {
                            m_MoveValueSliderXEP[i].value = m_controlUIValuesEP[i].LastXMoveSpeed;   //MoveSlider X
                            m_xAxisValueTextPs[i].text = $"{m_controlUIValuesEP[i].LastXMoveSpeed:N2}";
                            break;
                        }
                    }

                    switch (m_controlUIStatesEP[i].CustomYSensitivity)
                    {
                        case false:
                        {
                            m_RotValueSliderYEP[i].value = m_yRotationDefault;  //RotSlider Y
                            m_yAxisValueTextPs[i].text = $"{m_yRotationDefault:N2}";
                            break;
                        }
                        case true:
                        {
                            m_RotValueSliderYEP[i].value = m_controlUIValuesEP[i].LastYRotSpeed;  //RotSlider Y
                            m_yAxisValueTextPs[i].text = $"{m_controlUIValuesEP[i].LastYRotSpeed:N2}";
                            break;
                        }
                    }
                }
            }
        }

        public void LowerSliderValue(Toggle _connectedToggle)
        {
            //Get the corresponding Slider (Value) in the Dictionary, for each submitted Toggle (Key), by the Button inside Unity.
            Slider connectedSlider = null;
            bool toggleFound = m_toggleSliderConnectXEP.ContainsKey(_connectedToggle);

            switch (toggleFound)
            {
                case true:
                    connectedSlider = m_toggleSliderConnectXEP[_connectedToggle];
                    break;
                case false:
                    connectedSlider = m_toggleSliderConnectYEP[_connectedToggle];
                    break;
            }

            //Only if the submitted Toggle isn't on, then the Button can lower the SliderValue.
            if (_connectedToggle.isOn)
                connectedSlider.value -= connectedSlider.maxValue * (0.01f * m_adjustSliderStep);
        }

        public void IncreaseSliderValue(Toggle _connectedToggle)
        {
            //Get the corresponding Slider (Value) in the Dictionary, for each submitted Toggle (Key), by the Button inside Unity.
            Slider connectedSlider = null;
            bool toggleFound = m_toggleSliderConnectXEP.ContainsKey(_connectedToggle);

            switch (toggleFound)
            {
                case true:
                    connectedSlider = m_toggleSliderConnectXEP[_connectedToggle];
                    break;
                case false:
                    connectedSlider = m_toggleSliderConnectYEP[_connectedToggle];
                    break;
            }

            //Only if the submitted Toggle isn't on, then the Button can increase the SliderValue.
            if (_connectedToggle.isOn)
                connectedSlider.value += connectedSlider.maxValue * (0.01f * m_adjustSliderStep);
        }

        public void ReSetDefault()
        {
            m_playerXRotInvertToggles[m_currentViewIndex].isOn = m_xRotInvertDefaults[m_currentViewIndex];
            m_playerYRotInvertToggles[m_currentViewIndex].isOn = m_yRotInvertDefaults[m_currentViewIndex];
            m_moveToggleKeysEP[m_currentViewIndex].isOn = m_customToggleDefaults[m_currentViewIndex];
            m_rotToggleKeysEP[m_currentViewIndex].isOn = m_customToggleDefaults[m_currentViewIndex];
            m_MoveValueSliderXEP[m_currentViewIndex].value = m_xMovespeedDefault;
            m_RotValueSliderYEP[m_currentViewIndex].value = m_yRotationDefault;
        }
        #endregion
    }
}