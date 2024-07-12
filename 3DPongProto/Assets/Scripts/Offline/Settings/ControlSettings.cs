using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ThreeDeePongProto.Offline.Settings
{
    public class ControlSettings : MonoBehaviour
    {
        [Header("Content Views")]
        [SerializeField] private Button[] m_playerButtons;
        [SerializeField] private Transform[] m_contentSubTransforms;
        [SerializeField, Range(0.1f, 0.9f)] private float m_reducedAlphaValue = 0.5f;
        [SerializeField, Range(0.5f, 1f)] private float m_maxAlphaValue = 1f;
        private int m_currentViewIndex;

        [Header("Axis Inversion")]
        [SerializeField] private Toggle[] m_playerXRotInvertToggles; //<-----------------------------TODO: switch by index also!
        [SerializeField] private Toggle[] m_playerYRotInvertToggles; //<-----------------------------TODO: switch by index also!

        [SerializeField] private bool[] m_xRotInvertDefaults;
        [SerializeField] private bool[] m_yRotInvertDefaults;
        [Space]
        #region Old Toggles and Bools
        //[Header("Old Toggles and Bools")]
        //[SerializeField] private Toggle m_xRotInvertToggle; //<-----------------------------
        //[SerializeField] private Toggle m_yRotInvertToggle; //<-----------------------------
        //[SerializeField] private bool m_xAxisDefault = false;
        //[SerializeField] private bool m_yAxisDefault = false;
        #endregion

        [Header("Axis Sensitivity")]
        [SerializeField] private Slider[] m_sensitivitySliderValuesP1;
        [SerializeField] private Slider[] m_sensitivitySliderValuesP2;
        [SerializeField] private Slider[] m_sensitivitySliderValuesP3;
        [SerializeField] private Slider[] m_sensitivitySliderValuesP4;
        [SerializeField] private TextMeshProUGUI[] m_xAxisValueTextPs;
        [SerializeField] private TextMeshProUGUI[] m_yAxisValueTextPs;
        [SerializeField] private Toggle[] m_customToggleKeysP1;
        [SerializeField] private Toggle[] m_customToggleKeysP2;
        [SerializeField] private Toggle[] m_customToggleKeysP3;
        [SerializeField] private Toggle[] m_customToggleKeysP4;
        [SerializeField] private bool[] m_customToggleDefaults;
        #region Old Keys and Values
        //[Header("Old Keys and Values")]
        //[SerializeField] private Toggle[] m_customToggleKeys;
        //[SerializeField] private Slider[] m_sensitivitySliderValues;
        #endregion
        [Space]
        #region Old Axis Value Texts
        //[Header("Old Axis Value Texts")]
        //[SerializeField] private TextMeshProUGUI m_xAxisValueText;
        //[SerializeField] private TextMeshProUGUI m_yAxisValueText;
        #endregion
        [SerializeField, Range(1, 20)] float m_xMovespeedDefault = 10f;
        [SerializeField, Range(1, 5)] float m_yRotationDefault = 2.5f;
        [SerializeField, Range(1, 10)] private float m_adjustSliderStep = 2.0f;

        //private Dictionary<Toggle, Slider> m_toggleSliderConnection = new Dictionary<Toggle, Slider>();
        private Dictionary<Toggle, Slider>[] m_toggleSliderConnectEP = new Dictionary<Toggle, Slider>[]
        {
            new Dictionary<Toggle, Slider>(),
            new Dictionary<Toggle, Slider>(),
            new Dictionary<Toggle, Slider>(),
            new Dictionary<Toggle, Slider>()
        };

        //private Dictionary<Toggle, Slider> m_toggleSliderConnectP1 = new Dictionary<Toggle, Slider>();
        //private Dictionary<Toggle, Slider> m_toggleSliderConnectP2 = new Dictionary<Toggle, Slider>();
        //private Dictionary<Toggle, Slider> m_toggleSliderConnectP3 = new Dictionary<Toggle, Slider>();
        //private Dictionary<Toggle, Slider> m_toggleSliderConnectP4 = new Dictionary<Toggle, Slider>();

        #region Scriptable-References
        [Header("Scriptable Objects")]
        [SerializeField] private ControlUIStates[] m_controlUIStatesEP;
        [SerializeField] private ControlUIValues[] m_controlUIValuesEP;
        #region Old Scriptable Objects
        [Header("Old Scriptable Objects")]
        //[SerializeField] private ControlUIStates m_controlUIStates;
        //[SerializeField] private ControlUIValues m_controlUIValues;
        #endregion
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
            m_currentViewIndex = 0;
            SetActivePlayerView(m_playerButtons[m_currentViewIndex]);

            for (int j = 0; j < m_customToggleKeysP1.Length; j++)
                m_toggleSliderConnectEP[0].Add(m_customToggleKeysP1[j], m_sensitivitySliderValuesP1[j]);
            for (int k = 0; k < m_customToggleKeysP2.Length; k++)
                m_toggleSliderConnectEP[1].Add(m_customToggleKeysP2[k], m_sensitivitySliderValuesP2[k]);
            for (int l = 0; l < m_customToggleKeysP2.Length; l++)
                m_toggleSliderConnectEP[2].Add(m_customToggleKeysP3[l], m_sensitivitySliderValuesP3[l]);
            for (int m = 0; m < m_customToggleKeysP2.Length; m++)
                m_toggleSliderConnectEP[3].Add(m_customToggleKeysP4[m], m_sensitivitySliderValuesP4[m]);

            //for (int i = 0; i < m_contentSubTransforms.Length; i++)
            //{
            //    if (m_contentSubTransforms[i] != null)
            //    {
            //        m_toggleSliderConnectEP[i].Add(m_customToggleKeysP1[i], m_sensitivitySliderValuesP1[i]);
            //    }
            //}

            //if (m_controlUIStates == null || m_controlUIValues == null)
            //    ReSetDefault();
            //else LoadControlSettingsFromScriptableSave(); move to 'MenuOrganisation.cs'.!
        }

        private void OnEnable()
        {
            AddSliderAndToggleListener();
        }

        private void OnDisable()
        {
            RemoveSliderAndToggleListener();

            SetActivePlayerView(m_playerButtons[0]);

            //m_persistentData.SaveData(m_settingsStatesFolderPath, m_controlFileName, m_fileFormat, m_controlUIStates, m_encryptionEnabled, true);
            //m_persistentData.SaveData(m_settingsValuesFolderPath, m_controlFileName, m_fileFormat, m_controlUIValues, m_encryptionEnabled, true);

            for (int i = 0; i < m_controlUIStatesEP.Length; i++)
            {
                m_persistentData.SaveData(m_settingsStatesFolderPath + $"{i}", m_controlFileName, m_fileFormat, m_controlUIStatesEP[i], m_encryptionEnabled, true);
            }

            for (int j = 0; j < m_controlUIValuesEP.Length; j++)
            {
                m_persistentData.SaveData(m_settingsValuesFolderPath + $"{j}", m_controlFileName, m_fileFormat, m_controlUIValuesEP[j], m_encryptionEnabled, true);
            }
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
            m_playerXRotInvertToggles[0].onValueChanged.AddListener(XRotInversionChange);
            m_playerXRotInvertToggles[1].onValueChanged.AddListener(XRotInversionChange);
            m_playerXRotInvertToggles[2].onValueChanged.AddListener(XRotInversionChange);
            m_playerXRotInvertToggles[3].onValueChanged.AddListener(XRotInversionChange);

            m_playerYRotInvertToggles[0].onValueChanged.AddListener(YRotInversionChange);
            m_playerYRotInvertToggles[1].onValueChanged.AddListener(YRotInversionChange);
            m_playerYRotInvertToggles[2].onValueChanged.AddListener(YRotInversionChange);
            m_playerYRotInvertToggles[3].onValueChanged.AddListener(YRotInversionChange);

            //m_xRotInvertToggle.onValueChanged.AddListener(XRotInversionChange);
            //m_yRotInvertToggle.onValueChanged.AddListener(YRotInversionChange);

            m_sensitivitySliderValuesP1[0].onValueChanged.AddListener(SensitivitySliderXValueChanges);
            m_sensitivitySliderValuesP1[1].onValueChanged.AddListener(SensitivitySliderYValueChanges);
            m_sensitivitySliderValuesP2[0].onValueChanged.AddListener(SensitivitySliderXValueChanges);
            m_sensitivitySliderValuesP2[1].onValueChanged.AddListener(SensitivitySliderYValueChanges);
            m_sensitivitySliderValuesP3[0].onValueChanged.AddListener(SensitivitySliderXValueChanges);
            m_sensitivitySliderValuesP3[1].onValueChanged.AddListener(SensitivitySliderYValueChanges);
            m_sensitivitySliderValuesP4[0].onValueChanged.AddListener(SensitivitySliderXValueChanges);
            m_sensitivitySliderValuesP4[1].onValueChanged.AddListener(SensitivitySliderYValueChanges);

            m_customToggleKeysP1[0].onValueChanged.AddListener(CustomToggleXValueChanges);
            m_customToggleKeysP1[1].onValueChanged.AddListener(CustomToggleYValueChanges);
            m_customToggleKeysP2[0].onValueChanged.AddListener(CustomToggleXValueChanges);
            m_customToggleKeysP2[1].onValueChanged.AddListener(CustomToggleYValueChanges);
            m_customToggleKeysP3[0].onValueChanged.AddListener(CustomToggleXValueChanges);
            m_customToggleKeysP3[1].onValueChanged.AddListener(CustomToggleYValueChanges);
            m_customToggleKeysP4[0].onValueChanged.AddListener(CustomToggleXValueChanges);
            m_customToggleKeysP4[1].onValueChanged.AddListener(CustomToggleYValueChanges);

        }

        /// <summary>
        /// Unsubscribe VolumeControl-Elements from UnityEvents.
        /// </summary>
        private void RemoveSliderAndToggleListener()
        {
            m_playerXRotInvertToggles[0].onValueChanged.RemoveListener(XRotInversionChange);
            m_playerXRotInvertToggles[1].onValueChanged.RemoveListener(XRotInversionChange);
            m_playerXRotInvertToggles[2].onValueChanged.RemoveListener(XRotInversionChange);
            m_playerXRotInvertToggles[3].onValueChanged.RemoveListener(XRotInversionChange);

            m_playerYRotInvertToggles[0].onValueChanged.RemoveListener(YRotInversionChange);
            m_playerYRotInvertToggles[1].onValueChanged.RemoveListener(YRotInversionChange);
            m_playerYRotInvertToggles[2].onValueChanged.RemoveListener(YRotInversionChange);
            m_playerYRotInvertToggles[3].onValueChanged.RemoveListener(YRotInversionChange);

            //m_xRotInvertToggle.onValueChanged.RemoveListener(XRotInversionChange);
            //m_yRotInvertToggle.onValueChanged.RemoveListener(YRotInversionChange);

            m_sensitivitySliderValuesP1[0].onValueChanged.RemoveListener(SensitivitySliderXValueChanges);
            m_sensitivitySliderValuesP1[1].onValueChanged.RemoveListener(SensitivitySliderYValueChanges);
            m_sensitivitySliderValuesP2[0].onValueChanged.RemoveListener(SensitivitySliderXValueChanges);
            m_sensitivitySliderValuesP2[1].onValueChanged.RemoveListener(SensitivitySliderYValueChanges);
            m_sensitivitySliderValuesP3[0].onValueChanged.RemoveListener(SensitivitySliderXValueChanges);
            m_sensitivitySliderValuesP3[1].onValueChanged.RemoveListener(SensitivitySliderYValueChanges);
            m_sensitivitySliderValuesP4[0].onValueChanged.RemoveListener(SensitivitySliderXValueChanges);
            m_sensitivitySliderValuesP4[1].onValueChanged.RemoveListener(SensitivitySliderYValueChanges);

            m_customToggleKeysP1[0].onValueChanged.RemoveListener(CustomToggleXValueChanges);
            m_customToggleKeysP1[1].onValueChanged.RemoveListener(CustomToggleYValueChanges);
            m_customToggleKeysP2[0].onValueChanged.RemoveListener(CustomToggleXValueChanges);
            m_customToggleKeysP2[1].onValueChanged.RemoveListener(CustomToggleYValueChanges);
            m_customToggleKeysP3[0].onValueChanged.RemoveListener(CustomToggleXValueChanges);
            m_customToggleKeysP3[1].onValueChanged.RemoveListener(CustomToggleYValueChanges);
            m_customToggleKeysP4[0].onValueChanged.RemoveListener(CustomToggleXValueChanges);
            m_customToggleKeysP4[1].onValueChanged.RemoveListener(CustomToggleYValueChanges);
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
            //m_controlUIStates.InvertXAxis = _xAxisInversion;
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
            //m_controlUIStates.InvertYAxis = _yAxisInversion;
        }

        private void SensitivitySliderXValueChanges(float _sliderXValue)
        {
            switch (m_currentViewIndex)
            {
                case 0:
                {
                    if (m_customToggleKeysP1[0].isOn)   //Player1 0 = XToggle!
                    {
                        m_controlUIValuesEP[0].LastXMoveSpeed = _sliderXValue;  //Player1 SO.
                        m_xAxisValueTextPs[0].text = $"{_sliderXValue:N2}";     //Player1 XText.
                    }
                    break;
                }
                case 1:
                {
                    if (m_customToggleKeysP2[0].isOn)   //Player2 0 = XToggle!
                    {
                        m_controlUIValuesEP[1].LastXMoveSpeed = _sliderXValue;  //Player2 SO.
                        m_xAxisValueTextPs[1].text = $"{_sliderXValue:N2}";     //Player2 XText.
                    }
                    break;
                }
                case 2:
                    if (m_customToggleKeysP3[0].isOn)   //Player3 0 = XToggle!
                    {
                        m_controlUIValuesEP[2].LastXMoveSpeed = _sliderXValue;  //Player3 SO.
                        m_xAxisValueTextPs[2].text = $"{_sliderXValue:N2}";     //Player3 XText.
                    }
                    break;
                case 3:
                    if (m_customToggleKeysP4[0].isOn)   //Player4 0 = XToggle!
                    {
                        m_controlUIValuesEP[3].LastXMoveSpeed = _sliderXValue;  //Player4 SO.
                        m_xAxisValueTextPs[3].text = $"{_sliderXValue:N2}";     //Player4 XText.
                    }
                    break;
                default:
                    break;
            }

            //if (m_customToggleKeys[0].isOn)
            //{
            //    m_controlUIValues.LastXMoveSpeed = _sliderXValue;
            //    m_xAxisValueText.text = $"{_sliderXValue:N2}";
            //}
        }

        private void SensitivitySliderYValueChanges(float _sliderYValue)
        {
            switch (m_currentViewIndex)
            {
                case 0:
                {
                    if (m_customToggleKeysP1[1].isOn)   //Player1 1 = YToggle!
                    {
                        m_controlUIValuesEP[0].LastYRotSpeed = _sliderYValue;  //Player1 SO.
                        m_yAxisValueTextPs[0].text = $"{_sliderYValue:N2}";    //Player1 YText.
                    }
                    break;
                }
                case 1:
                {
                    if (m_customToggleKeysP2[1].isOn)   //Player2 1 = YToggle!
                    {
                        m_controlUIValuesEP[1].LastYRotSpeed = _sliderYValue;  //Player2 SO.
                        m_yAxisValueTextPs[1].text = $"{_sliderYValue:N2}";    //Player2 YText.
                    }
                    break;
                }
                case 2:
                {
                    if (m_customToggleKeysP3[1].isOn)   //Player3 1 = YToggle!
                    {
                        m_controlUIValuesEP[2].LastYRotSpeed = _sliderYValue;  //Player3 SO.
                        m_yAxisValueTextPs[2].text = $"{_sliderYValue:N2}";    //Player3 YText.
                    }
                    break;
                }
                case 3:
                {
                    if (m_customToggleKeysP4[1].isOn)   //Player4 1 = YToggle!
                    {
                        m_controlUIValuesEP[3].LastYRotSpeed = _sliderYValue;  //Player4 SO.
                        m_yAxisValueTextPs[3].text = $"{_sliderYValue:N2}";    //Player4 YText.
                    }
                    break;
                }
                default:
                    break;
            }

            //if (m_customToggleKeys[1].isOn)
            //{
            //    m_controlUIValues.LastYRotSpeed = _sliderYValue;
            //    m_yAxisValueText.text = $"{_sliderYValue:N2}";
            //}
        }

        private void CustomToggleXValueChanges(bool _toggleX)
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
                            m_sensitivitySliderValuesP1[0].value = m_xMovespeedDefault;
                            m_xAxisValueTextPs[0].text = $"{m_xMovespeedDefault:N2}";
                            m_sensitivitySliderValuesP1[0].interactable = false;
                            break;
                        }
                        case true:
                        {
                            m_sensitivitySliderValuesP1[0].value = m_controlUIValuesEP[0].LastXMoveSpeed;
                            m_xAxisValueTextPs[0].text = $"{m_controlUIValuesEP[0].LastXMoveSpeed:N2}";
                            m_sensitivitySliderValuesP1[0].interactable = true;
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
                            m_sensitivitySliderValuesP2[0].value = m_xMovespeedDefault;
                            m_xAxisValueTextPs[1].text = $"{m_xMovespeedDefault:N2}";
                            m_sensitivitySliderValuesP2[0].interactable = false;
                            break;
                        }
                        case true:
                        {
                            m_sensitivitySliderValuesP2[0].value = m_controlUIValuesEP[1].LastXMoveSpeed;
                            m_xAxisValueTextPs[1].text = $"{m_controlUIValuesEP[1].LastXMoveSpeed:N2}";
                            m_sensitivitySliderValuesP2[0].interactable = true;
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
                            m_sensitivitySliderValuesP3[0].value = m_xMovespeedDefault;
                            m_xAxisValueTextPs[2].text = $"{m_xMovespeedDefault:N2}";
                            m_sensitivitySliderValuesP3[0].interactable = false;
                            break;
                        }
                        case true:
                        {
                            m_sensitivitySliderValuesP3[0].value = m_controlUIValuesEP[2].LastXMoveSpeed;
                            m_xAxisValueTextPs[2].text = $"{m_controlUIValuesEP[2].LastXMoveSpeed:N2}";
                            m_sensitivitySliderValuesP3[0].interactable = true;
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
                            m_sensitivitySliderValuesP4[0].value = m_xMovespeedDefault;
                            m_xAxisValueTextPs[3].text = $"{m_xMovespeedDefault:N2}";
                            m_sensitivitySliderValuesP4[0].interactable = false;
                            break;
                        }
                        case true:
                        {
                            m_sensitivitySliderValuesP4[0].value = m_controlUIValuesEP[3].LastXMoveSpeed;
                            m_xAxisValueTextPs[3].text = $"{m_controlUIValuesEP[3].LastXMoveSpeed:N2}";
                            m_sensitivitySliderValuesP4[0].interactable = true;
                            break;
                        }
                    }
                    break;
                }
                default:
                    break;
            }

            //m_controlUIStates.CustomXSensitivity = _toggleX;

            //switch (_toggleX)
            //{
            //    case false:
            //    {
            //        m_sensitivitySliderValues[0].value = m_xMovespeedDefault;
            //        m_xAxisValueText.text = $"{m_xMovespeedDefault:N2}";
            //        m_sensitivitySliderValues[0].interactable = false;
            //        break;
            //    }
            //    case true:
            //    {
            //        m_sensitivitySliderValues[0].value = m_controlUIValues.LastXMoveSpeed;
            //        m_xAxisValueText.text = $"{m_controlUIValues.LastXMoveSpeed:N2}";
            //        m_sensitivitySliderValues[0].interactable = true;
            //        break;
            //    }
            //}
        }

        private void CustomToggleYValueChanges(bool _toggleY)
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
                            m_controlUIValuesEP[0].LastYRotSpeed = m_sensitivitySliderValuesP1[1].value;
                            m_sensitivitySliderValuesP1[1].value = m_yRotationDefault;
                            m_yAxisValueTextPs[0].text = $"{m_yRotationDefault:N2}";
                            m_sensitivitySliderValuesP1[1].interactable = false;
                            break;
                        }
                        case true:
                        {
                            m_sensitivitySliderValuesP1[1].value = m_controlUIValuesEP[0].LastYRotSpeed;
                            m_yAxisValueTextPs[0].text = $"{m_controlUIValuesEP[0].LastYRotSpeed:N2}";
                            m_sensitivitySliderValuesP1[1].interactable = true;
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
                            m_controlUIValuesEP[1].LastYRotSpeed = m_sensitivitySliderValuesP2[1].value;
                            m_sensitivitySliderValuesP2[1].value = m_yRotationDefault;
                            m_yAxisValueTextPs[1].text = $"{m_yRotationDefault:N2}";
                            m_sensitivitySliderValuesP2[1].interactable = false;
                            break;
                        }
                        case true:
                        {
                            m_sensitivitySliderValuesP2[1].value = m_controlUIValuesEP[1].LastYRotSpeed;
                            m_yAxisValueTextPs[1].text = $"{m_controlUIValuesEP[1].LastYRotSpeed:N2}";
                            m_sensitivitySliderValuesP2[1].interactable = true;
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
                            m_controlUIValuesEP[2].LastYRotSpeed = m_sensitivitySliderValuesP3[1].value;
                            m_sensitivitySliderValuesP3[1].value = m_yRotationDefault;
                            m_yAxisValueTextPs[2].text = $"{m_yRotationDefault:N2}";
                            m_sensitivitySliderValuesP3[1].interactable = false;
                            break;
                        }
                        case true:
                        {
                            m_sensitivitySliderValuesP3[1].value = m_controlUIValuesEP[2].LastYRotSpeed;
                            m_yAxisValueTextPs[2].text = $"{m_controlUIValuesEP[2].LastYRotSpeed:N2}";
                            m_sensitivitySliderValuesP3[1].interactable = true;
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
                            m_controlUIValuesEP[3].LastYRotSpeed = m_sensitivitySliderValuesP4[1].value;
                            m_sensitivitySliderValuesP4[1].value = m_yRotationDefault;
                            m_yAxisValueTextPs[3].text = $"{m_yRotationDefault:N2}";
                            m_sensitivitySliderValuesP4[1].interactable = false;
                            break;
                        }
                        case true:
                        {
                            m_sensitivitySliderValuesP4[1].value = m_controlUIValuesEP[3].LastYRotSpeed;
                            m_yAxisValueTextPs[3].text = $"{m_controlUIValuesEP[3].LastYRotSpeed:N2}";
                            m_sensitivitySliderValuesP4[1].interactable = true;
                            break;
                        }
                    }
                    break;
                }
                default:
                    break;
            }

            //m_controlUIStates.CustomYSensitivity = _toggleY;

            //switch (_toggleY)
            //{
            //    case false:
            //    {
            //        m_controlUIValues.LastYRotSpeed = m_sensitivitySliderValues[1].value;
            //        m_sensitivitySliderValues[1].value = m_yRotationDefault;
            //        m_yAxisValueText.text = $"{m_yRotationDefault:N2}";
            //        m_sensitivitySliderValues[1].interactable = false;
            //        break;
            //    }
            //    case true:
            //    {
            //        m_sensitivitySliderValues[1].value = m_controlUIValues.LastYRotSpeed;
            //        m_yAxisValueText.text = $"{m_controlUIValues.LastYRotSpeed:N2}";
            //        m_sensitivitySliderValues[1].interactable = true;
            //        break;
            //    }
            //}
        }

        public void SetActivePlayerView(Button _sender)
        {
            for (int i = 0; i < m_playerButtons.Length; i++)
            {
                if (_sender == m_playerButtons[i])
                {
                    m_contentSubTransforms[i].gameObject.SetActive(true);
                    Color tempAlpha1 = m_playerButtons[i].image.color;
                    tempAlpha1.a = m_maxAlphaValue;
                    m_playerButtons[i].image.color = tempAlpha1;
                    m_currentViewIndex = i; //Routes the Default Button Resets.
                }
                else
                {
                    m_contentSubTransforms[i].gameObject.SetActive(false);
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
            m_playerXRotInvertToggles[0].isOn = m_controlUIStatesEP[0].InvertXAxis;
            m_playerXRotInvertToggles[1].isOn = m_controlUIStatesEP[1].InvertXAxis;
            m_playerXRotInvertToggles[2].isOn = m_controlUIStatesEP[2].InvertXAxis;
            m_playerXRotInvertToggles[3].isOn = m_controlUIStatesEP[3].InvertXAxis;

            m_playerYRotInvertToggles[0].isOn = m_controlUIStatesEP[0].InvertYAxis;
            m_playerYRotInvertToggles[1].isOn = m_controlUIStatesEP[1].InvertYAxis;
            m_playerYRotInvertToggles[2].isOn = m_controlUIStatesEP[2].InvertYAxis;
            m_playerYRotInvertToggles[3].isOn = m_controlUIStatesEP[3].InvertYAxis;
            //m_xRotInvertToggle.isOn = m_controlUIStates.InvertXAxis;
            //m_yRotInvertToggle.isOn = m_controlUIStates.InvertYAxis;

            m_customToggleKeysP1[0].isOn = m_controlUIStatesEP[0].CustomXSensitivity;
            m_customToggleKeysP2[0].isOn = m_controlUIStatesEP[1].CustomXSensitivity;
            m_customToggleKeysP3[0].isOn = m_controlUIStatesEP[2].CustomXSensitivity;
            m_customToggleKeysP4[0].isOn = m_controlUIStatesEP[3].CustomXSensitivity;

            m_customToggleKeysP1[1].isOn = m_controlUIStatesEP[0].CustomYSensitivity;
            m_customToggleKeysP2[1].isOn = m_controlUIStatesEP[1].CustomYSensitivity;
            m_customToggleKeysP3[1].isOn = m_controlUIStatesEP[2].CustomYSensitivity;
            m_customToggleKeysP4[1].isOn = m_controlUIStatesEP[3].CustomYSensitivity;

            switch (m_controlUIStatesEP[0].CustomXSensitivity)
            {
                case false:
                {
                    m_sensitivitySliderValuesP1[0].value = m_xMovespeedDefault;
                    m_xAxisValueTextPs[0].text = $"{m_xMovespeedDefault:N2}";
                    break;
                }
                case true:
                {
                    m_sensitivitySliderValuesP1[0].value = m_controlUIValuesEP[0].LastXMoveSpeed;
                    m_xAxisValueTextPs[0].text = $"{m_controlUIValuesEP[0].LastXMoveSpeed:N2}";
                    //SensitivitySliderXValueChanges(m_controlUIValuesEP[0].LastXMoveSpeed);
                    break;
                }
            }

            switch (m_controlUIStatesEP[1].CustomXSensitivity)
            {
                case false:
                {
                    m_sensitivitySliderValuesP2[0].value = m_xMovespeedDefault;
                    m_xAxisValueTextPs[1].text = $"{m_xMovespeedDefault:N2}";
                    break;
                }
                case true:
                {
                    m_sensitivitySliderValuesP2[0].value = m_controlUIValuesEP[1].LastXMoveSpeed;
                    m_xAxisValueTextPs[1].text = $"{m_controlUIValuesEP[1].LastXMoveSpeed:N2}";
                    //SensitivitySliderXValueChanges(m_controlUIValuesEP[1].LastXMoveSpeed);
                    break;
                }
            }

            switch (m_controlUIStatesEP[2].CustomXSensitivity)
            {
                case false:
                {
                    m_sensitivitySliderValuesP3[0].value = m_xMovespeedDefault;
                    m_xAxisValueTextPs[2].text = $"{m_xMovespeedDefault:N2}";
                    break;
                }
                case true:
                {
                    m_sensitivitySliderValuesP3[0].value = m_controlUIValuesEP[2].LastXMoveSpeed;
                    m_xAxisValueTextPs[2].text = $"{m_controlUIValuesEP[2].LastXMoveSpeed:N2}";
                    //SensitivitySliderXValueChanges(m_controlUIValuesEP[2].LastXMoveSpeed);
                    break;
                }
            }

            switch (m_controlUIStatesEP[3].CustomXSensitivity)
            {
                case false:
                {
                    m_sensitivitySliderValuesP4[0].value = m_xMovespeedDefault;
                    m_xAxisValueTextPs[3].text = $"{m_xMovespeedDefault:N2}";
                    break;
                }
                case true:
                {
                    m_sensitivitySliderValuesP4[0].value = m_controlUIValuesEP[3].LastXMoveSpeed;
                    m_xAxisValueTextPs[3].text = $"{m_controlUIValuesEP[3].LastXMoveSpeed:N2}";
                    //SensitivitySliderXValueChanges(m_controlUIValuesEP[3].LastXMoveSpeed);
                    break;
                }
            }

            switch (m_controlUIStatesEP[0].CustomYSensitivity)
            {
                case false:
                {
                    m_sensitivitySliderValuesP1[1].value = m_yRotationDefault;
                    m_yAxisValueTextPs[0].text = $"{m_yRotationDefault:N2}";
                    break;
                }
                case true:
                {
                    m_sensitivitySliderValuesP1[1].value = m_controlUIValuesEP[0].LastYRotSpeed;
                    m_yAxisValueTextPs[0].text = $"{m_controlUIValuesEP[0].LastYRotSpeed:N2}";
                    //SensitivitySliderYValueChanges(m_controlUIValuesEP[0].LastYRotSpeed);
                    break;
                }
            }

            switch (m_controlUIStatesEP[1].CustomYSensitivity)
            {
                case false:
                {
                    m_sensitivitySliderValuesP2[1].value = m_yRotationDefault;
                    m_yAxisValueTextPs[1].text = $"{m_yRotationDefault:N2}";
                    break;
                }
                case true:
                {
                    m_sensitivitySliderValuesP2[1].value = m_controlUIValuesEP[1].LastYRotSpeed;
                    m_yAxisValueTextPs[1].text = $"{m_controlUIValuesEP[1].LastYRotSpeed:N2}";
                    //SensitivitySliderYValueChanges(m_controlUIValuesEP[1].LastYRotSpeed);
                    break;
                }
            }

            switch (m_controlUIStatesEP[2].CustomYSensitivity)
            {
                case false:
                {
                    m_sensitivitySliderValuesP3[1].value = m_yRotationDefault;
                    m_yAxisValueTextPs[2].text = $"{m_yRotationDefault:N2}";
                    break;
                }
                case true:
                {
                    m_sensitivitySliderValuesP3[1].value = m_controlUIValuesEP[2].LastYRotSpeed;
                    m_yAxisValueTextPs[2].text = $"{m_controlUIValuesEP[2].LastYRotSpeed:N2}";
                    //SensitivitySliderYValueChanges(m_controlUIValuesEP[2].LastYRotSpeed);
                }
                    break;
            }

            switch (m_controlUIStatesEP[3].CustomYSensitivity)
            {
                case false:
                {
                    m_sensitivitySliderValuesP4[1].value = m_yRotationDefault;
                    m_yAxisValueTextPs[3].text = $"{m_yRotationDefault:N2}";
                    break;
                }
                case true:
                {
                    m_sensitivitySliderValuesP4[1].value = m_controlUIValuesEP[3].LastYRotSpeed;
                    m_yAxisValueTextPs[3].text = $"{m_controlUIValuesEP[3].LastYRotSpeed:N2}";
                    //SensitivitySliderYValueChanges(m_controlUIValuesEP[3].LastYRotSpeed);
                    break;
                }
            }
        }

        public void LowerSliderValue(Toggle _connectedToggle)
        {
            Slider connectedSlider = null;

            switch (m_currentViewIndex)
            {
                case 0:
                {
                    connectedSlider = m_toggleSliderConnectEP[0].GetValueOrDefault(_connectedToggle);
                    break;
                }
                case 1:
                {
                    connectedSlider = m_toggleSliderConnectEP[1].GetValueOrDefault(_connectedToggle);
                    break;
                }
                case 2:
                {
                    connectedSlider = m_toggleSliderConnectEP[2].GetValueOrDefault(_connectedToggle);
                    break;
                }
                case 3:
                {
                    connectedSlider = m_toggleSliderConnectEP[3].GetValueOrDefault(_connectedToggle);
                    break;
                }
            }
            //Get the corresponding Slider (Value) in the Dictionary, for each submitted Mute (Key), by the Button inside Unity.
            //Slider connectedSlider = m_toggleSliderConnection[_connectedToggle];

            //Only if the submitted Mute isn't on, then the Button can lower the SliderValue.
            if (_connectedToggle.isOn)
                connectedSlider.value -= connectedSlider.maxValue * (0.01f * m_adjustSliderStep);
        }

        public void IncreaseSliderValue(Toggle _connectedToggle)
        {
            Slider connectedSlider = null;

            switch (m_currentViewIndex)
            {
                case 0:
                {
                    connectedSlider = m_toggleSliderConnectEP[0].GetValueOrDefault(_connectedToggle);
                    break;
                }
                case 1:
                {
                    connectedSlider = m_toggleSliderConnectEP[1].GetValueOrDefault(_connectedToggle);
                    break;
                }
                case 2:
                {
                    connectedSlider = m_toggleSliderConnectEP[2].GetValueOrDefault(_connectedToggle);
                    break;
                }
                case 3:
                {
                    connectedSlider = m_toggleSliderConnectEP[3].GetValueOrDefault(_connectedToggle);
                    break;
                }
            }

            ////Get the corresponding Slider (Value) in the Dictionary, for each submitted Mute (Key), by the Button inside Unity.
            //Slider connectedSlider = m_toggleSliderConnection[_connectedToggle];

            //Commented out, to allow the 'LouderButton' to increase the SliderValue out of the muted state.
            if (_connectedToggle.isOn)
                connectedSlider.value += connectedSlider.maxValue * (0.01f * m_adjustSliderStep);
        }

        public void ReSetDefault()
        {
            switch (m_currentViewIndex)
            {
                case 0:
                {
                    m_playerXRotInvertToggles[m_currentViewIndex].isOn = m_xRotInvertDefaults[m_currentViewIndex];
                    m_playerYRotInvertToggles[m_currentViewIndex].isOn = m_yRotInvertDefaults[m_currentViewIndex];
                    m_customToggleKeysP1[0].isOn = m_customToggleDefaults[0];
                    m_customToggleKeysP1[1].isOn = m_customToggleDefaults[0];
                    m_sensitivitySliderValuesP1[0].value = m_xMovespeedDefault;
                    m_sensitivitySliderValuesP1[1].value = m_yRotationDefault;
                    break;
                }
                case 1:
                {
                    m_playerXRotInvertToggles[m_currentViewIndex].isOn = m_xRotInvertDefaults[m_currentViewIndex];
                    m_playerYRotInvertToggles[m_currentViewIndex].isOn = m_yRotInvertDefaults[m_currentViewIndex];
                    m_customToggleKeysP2[0].isOn = m_customToggleDefaults[1];
                    m_customToggleKeysP2[1].isOn = m_customToggleDefaults[1];
                    m_sensitivitySliderValuesP2[0].value = m_xMovespeedDefault;
                    m_sensitivitySliderValuesP2[1].value = m_yRotationDefault;
                    break;
                }
                case 2:
                {
                    m_playerXRotInvertToggles[m_currentViewIndex].isOn = m_xRotInvertDefaults[m_currentViewIndex];
                    m_playerYRotInvertToggles[m_currentViewIndex].isOn = m_yRotInvertDefaults[m_currentViewIndex];
                    m_customToggleKeysP3[0].isOn = m_customToggleDefaults[2];
                    m_customToggleKeysP3[1].isOn = m_customToggleDefaults[2];
                    m_sensitivitySliderValuesP3[0].value = m_xMovespeedDefault;
                    m_sensitivitySliderValuesP3[1].value = m_yRotationDefault;
                    break;
                }
                case 3:
                {
                    m_playerXRotInvertToggles[m_currentViewIndex].isOn = m_xRotInvertDefaults[m_currentViewIndex];
                    m_playerYRotInvertToggles[m_currentViewIndex].isOn = m_yRotInvertDefaults[m_currentViewIndex];
                    m_customToggleKeysP4[0].isOn = m_customToggleDefaults[3];
                    m_customToggleKeysP4[1].isOn = m_customToggleDefaults[3];
                    m_sensitivitySliderValuesP4[0].value = m_xMovespeedDefault;
                    m_sensitivitySliderValuesP4[1].value = m_yRotationDefault;
                    break;
                }
            }

            //m_xRotInvertToggle.isOn = m_xAxisDefault;
            //m_yRotInvertToggle.isOn = m_yAxisDefault;
            //m_customToggleKeys[0].isOn = m_customToggleDefaults;
            //m_customToggleKeys[1].isOn = m_customToggleDefaults;

            //m_sensitivitySliderValues[0].value = m_xMovespeedDefault;
            //m_sensitivitySliderValues[1].value = m_yRotationDefault;
        }
        #endregion
    }
}