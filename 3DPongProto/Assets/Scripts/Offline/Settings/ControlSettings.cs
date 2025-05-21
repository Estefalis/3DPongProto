using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ThreeDeePongProto.Shared.Settings
{
    public class ControlSettings : MonoBehaviour
    {
        #region Content Views
        [Header("Content Views")]
        [SerializeField] private Button[] m_playerButtons;
        [SerializeField] private Transform[] m_contentSubTransforms;

        [Header("Change Button Navigation")]
        [SerializeField] private Selectable[] m_lastKbSelectable;
        [SerializeField] private Selectable[] m_lastGpSelectable;
        [SerializeField] private Transform m_zoomGroup;         //Or any other last group about the playerButtons.
        [SerializeField] private Button m_resetButton;
        [SerializeField] private Button m_backButton;

        private int m_currentViewIndex;
        private List<Selectable> m_zoomSelectables = new();
        internal static event Action<int> PlayerViewIndex;        //Subscriber: RebindManager.
        internal static event Action ResetPlayerViewRebinds;      //Subscriber: RebindManager.
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
        [SerializeField] private Slider m_MoveValueSliderXEP;
        [SerializeField] private Slider m_RotValueSliderYEP;
        [SerializeField] private TextMeshProUGUI m_MoveValueTextPs;
        [SerializeField] private TextMeshProUGUI m_RotValueTextPs;
        [SerializeField] private Toggle m_moveToggleKeysEP;
        [SerializeField] private Toggle m_rotToggleKeysEP;
        [SerializeField] private bool m_customToggleDefaults;
        [Space]
        [SerializeField, Range(1, 20)] float m_xMoveSpeedDefault = 10f;
        [SerializeField, Range(1, 5)] float m_yRotationDefault = 2.5f;
        [SerializeField, Range(1, 10)] private float m_adjustSliderStep = 2.0f;

        private readonly Dictionary<Toggle, Slider> m_toggleSliderConnectXEP = new();
        private readonly Dictionary<Toggle, Slider> m_toggleSliderConnectYEP = new();

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
        [SerializeField] private ControlUIStates[] m_controlUIStates;
        [SerializeField] private ControlUIValues[] m_controlUIValues;
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
            //To update navigation.SelectOnDown on playerIndex change.
            m_zoomSelectables = m_zoomGroup.GetComponentsInChildren<Selectable>(true).ToList();
            SetActiveRebindScrollView(m_playerButtons[m_currentViewIndex]);

            m_toggleSliderConnectXEP.Add(m_moveToggleKeysEP, m_MoveValueSliderXEP);
            m_toggleSliderConnectYEP.Add(m_rotToggleKeysEP, m_RotValueSliderYEP);

            for (int i = 0; i < m_contentSubTransforms.Length; i++)
            {
                if (m_controlUIStates[i] == null || m_controlUIValues[i] == null)
                {
                    m_currentViewIndex = i;
                    ReSetDefault();
                }
            }
            //else LoadControlSettingsFromScriptableSave(); move to 'MenuNavigation.cs'.!
        }

        private void OnEnable()
        {
            InitialUISetup();
            AddSliderAndToggleListener();
        }

        private void OnDisable()
        {
            RemoveSliderAndToggleListener();

            for (int i = 0; i < m_controlUIStates.Length; i++)
            {
                m_persistentData.SaveData(m_settingsStatesFolderPath, m_controlFileName + $"{i}", m_fileFormat, m_controlUIStates[i], m_encryptionEnabled, true);
            }

            for (int j = 0; j < m_controlUIValues.Length; j++)
            {
                m_persistentData.SaveData(m_settingsValuesFolderPath, m_controlFileName + $"{j}", m_fileFormat, m_controlUIValues[j], m_encryptionEnabled, true);
            }
        }

        private void Update()
        {
            if (!EventSystem.current.currentSelectedGameObject.TryGetComponent<Button>(out var button) && !m_playerButtons.Contains(button))
                return;

            for (int i = 0; i < m_playerButtons.Length; i++)
            {
                if (m_playerButtons[i] == button)
                    SetActiveRebindScrollView(m_playerButtons[i]);
            }
        }

        /// <summary>
        /// Subscribe VolumeControl-Elements to UnityEvents.
        /// </summary>
        #region UnRegister-Listener-Region
        private void AddSliderAndToggleListener()
        {
            for (int i = 0; i < m_controlUIStates.Length; i++)
            {
                if (m_controlUIStates[i] != null)
                {
                    m_playerXRotInvertToggles[i].onValueChanged.AddListener(XRotInversionChange);
                    m_playerYRotInvertToggles[i].onValueChanged.AddListener(YRotInversionChange);
                    m_moveToggleKeysEP.onValueChanged.AddListener(MoveToggleXValueChanges);
                    m_rotToggleKeysEP.onValueChanged.AddListener(RotToggleYValueChanges);
                }
            }

            for (int j = 0; j < m_controlUIValues.Length; j++)
            {
                if (m_controlUIValues[j] != null)
                {
                    m_MoveValueSliderXEP.onValueChanged.AddListener(SensitivitySliderXValueChanges);
                    m_RotValueSliderYEP.onValueChanged.AddListener(SensitivitySliderYValueChanges);
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
            for (int i = 0; i < m_controlUIStates.Length; i++)
            {
                if (m_controlUIStates[i] != null)
                {
                    m_playerXRotInvertToggles[i].onValueChanged.RemoveListener(XRotInversionChange);
                    m_playerYRotInvertToggles[i].onValueChanged.RemoveListener(YRotInversionChange);
                    m_moveToggleKeysEP.onValueChanged.RemoveListener(MoveToggleXValueChanges);
                    m_rotToggleKeysEP.onValueChanged.RemoveListener(RotToggleYValueChanges);
                }
            }

            for (int j = 0; j < m_controlUIValues.Length; j++)
            {
                if (m_controlUIValues[j] != null)
                {
                    m_MoveValueSliderXEP.onValueChanged.RemoveListener(SensitivitySliderXValueChanges);
                    m_RotValueSliderYEP.onValueChanged.RemoveListener(SensitivitySliderYValueChanges);
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
                    m_controlUIStates[0].InvertXAxis = _xAxisInversion;
                    break;
                case 1:
                    m_controlUIStates[1].InvertXAxis = _xAxisInversion;
                    break;
                case 2:
                    m_controlUIStates[2].InvertXAxis = _xAxisInversion;
                    break;
                case 3:
                    m_controlUIStates[3].InvertXAxis = _xAxisInversion;
                    break;
            }
        }

        private void YRotInversionChange(bool _yAxisInversion)
        {
            switch (m_currentViewIndex)
            {
                case 0:
                    m_controlUIStates[0].InvertYAxis = _yAxisInversion;
                    break;
                case 1:
                    m_controlUIStates[1].InvertYAxis = _yAxisInversion;
                    break;
                case 2:
                    m_controlUIStates[2].InvertYAxis = _yAxisInversion;
                    break;
                case 3:
                    m_controlUIStates[3].InvertYAxis = _yAxisInversion;
                    break;
            }
        }

        private void SensitivitySliderXValueChanges(float _sliderXValue)
        {
            if (m_moveToggleKeysEP.isOn)
            {
                m_controlUIValues[0].LastXMoveSpeed = _sliderXValue;  //Player1 SO.
                m_controlUIValues[1].LastXMoveSpeed = _sliderXValue;  //Player2 SO.
                m_controlUIValues[2].LastXMoveSpeed = _sliderXValue;  //Player3 SO.
                m_controlUIValues[3].LastXMoveSpeed = _sliderXValue;  //Player4 SO.
                m_MoveValueTextPs.text = $"{_sliderXValue:N2}";
            }
        }

        private void SensitivitySliderYValueChanges(float _sliderYValue)
        {
            if (m_rotToggleKeysEP.isOn)
            {
                m_controlUIValues[0].LastYRotSpeed = _sliderYValue; //Player1 SO.
                m_controlUIValues[1].LastYRotSpeed = _sliderYValue; //Player2 SO.
                m_controlUIValues[2].LastYRotSpeed = _sliderYValue; //Player3 SO.
                m_controlUIValues[3].LastYRotSpeed = _sliderYValue; //Player4 SO.
                m_RotValueTextPs.text = $"{_sliderYValue:N2}";
            }
        }

        private void MoveToggleXValueChanges(bool _toggleX)
        {
            m_controlUIStates[0].CustomXSensitivity = _toggleX;
            m_controlUIStates[1].CustomXSensitivity = _toggleX;
            m_controlUIStates[2].CustomXSensitivity = _toggleX;
            m_controlUIStates[3].CustomXSensitivity = _toggleX;

            switch (_toggleX)
            {
                case false:
                {
                    m_MoveValueSliderXEP.value = m_xMoveSpeedDefault; //MoveSlider X
                    m_MoveValueTextPs.text = $"{m_xMoveSpeedDefault:N2}";
                    m_MoveValueSliderXEP.interactable = false;
                    break;
                }
                case true:
                {
                    m_MoveValueSliderXEP.value = m_controlUIValues[0].LastXMoveSpeed;
                    m_MoveValueTextPs.text = $"{m_controlUIValues[0].LastXMoveSpeed:N2}";
                    m_MoveValueSliderXEP.interactable = true;
                    break;
                }
            }
        }

        private void RotToggleYValueChanges(bool _toggleY)
        {
            m_controlUIStates[0].CustomYSensitivity = _toggleY;
            m_controlUIStates[1].CustomYSensitivity = _toggleY;
            m_controlUIStates[2].CustomYSensitivity = _toggleY;
            m_controlUIStates[3].CustomYSensitivity = _toggleY;

            switch (_toggleY)
            {
                case false:
                {
                    m_RotValueSliderYEP.value = m_yRotationDefault;  //RotSlider Y
                    m_RotValueTextPs.text = $"{m_yRotationDefault:N2}";
                    m_RotValueSliderYEP.interactable = false;
                    break;
                }
                case true:
                {
                    m_RotValueSliderYEP.value = m_controlUIValues[0].LastYRotSpeed;
                    m_RotValueTextPs.text = $"{m_controlUIValues[0].LastYRotSpeed:N2}";
                    m_RotValueSliderYEP.interactable = true;
                    break;
                }
            }
        }

        public void SetActiveRebindScrollView(Button _sender)
        {
            for (int i = 0; i < m_playerButtons.Length; i++)
            {
                if (_sender == m_playerButtons[i])
                {
                    m_contentSubTransforms[i].gameObject.SetActive(true);
                    m_currentViewIndex = i; //Routes the Default Button Resets.
                    PlayerViewIndex?.Invoke(m_currentViewIndex);
                    UpdateNavigationOnSwitch();
                }
                else
                {
                    m_contentSubTransforms[i].gameObject.SetActive(false);
                }
            }
        }
        #endregion

        #region Custom Methods
        private void InitialUISetup()
        {
            m_moveToggleKeysEP.isOn = m_controlUIStates[0].CustomXSensitivity;
            m_rotToggleKeysEP.isOn = m_controlUIStates[0].CustomYSensitivity;

            switch (m_controlUIStates[0].CustomXSensitivity)
            {
                case false:
                {
                    m_MoveValueSliderXEP.value = m_xMoveSpeedDefault;    //MoveSlider X
                    m_MoveValueTextPs.text = $"{m_xMoveSpeedDefault:N2}";
                    break;
                }
                case true:
                {
                    m_MoveValueSliderXEP.value = m_controlUIValues[0].LastXMoveSpeed;   //MoveSlider X
                    m_MoveValueTextPs.text = $"{m_controlUIValues[0].LastXMoveSpeed:N2}";
                    break;
                }
            }

            switch (m_controlUIStates[0].CustomYSensitivity)
            {
                case false:
                {
                    m_RotValueSliderYEP.value = m_yRotationDefault;  //RotSlider Y
                    m_RotValueTextPs.text = $"{m_yRotationDefault:N2}";
                    break;
                }
                case true:
                {
                    m_RotValueSliderYEP.value = m_controlUIValues[0].LastYRotSpeed;  //RotSlider Y
                    m_RotValueTextPs.text = $"{m_controlUIValues[0].LastYRotSpeed:N2}";
                    break;
                }
            }

            for (int i = 0; i < m_contentSubTransforms.Length; i++)
            {
                if (m_contentSubTransforms[i] != null)
                {
                    m_playerXRotInvertToggles[i].isOn = m_controlUIStates[i].InvertXAxis;
                    m_playerYRotInvertToggles[i].isOn = m_controlUIStates[i].InvertYAxis;
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

        private void UpdateNavigationOnSwitch()
        {
            foreach (Selectable selectable in m_zoomSelectables)
            {
                if (selectable.gameObject.activeInHierarchy)
                {
                    Navigation navigation = selectable.navigation;
                    navigation.selectOnDown = m_playerButtons[m_currentViewIndex];
                    selectable.navigation = navigation;
                }
            }

            Navigation resetButtonNav = m_resetButton.GetComponent<Button>().navigation;
            foreach (Selectable selectable in m_lastKbSelectable)
            {
                if (selectable.gameObject.activeInHierarchy)
                {
                    resetButtonNav.selectOnUp = m_lastKbSelectable[m_currentViewIndex];
                    m_resetButton.navigation = resetButtonNav;
                    break;
                }
            }

            Navigation backButtonNav = m_backButton.GetComponent<Button>().navigation;
            foreach (Selectable selectable in m_lastGpSelectable)
            {
                if (selectable.gameObject.activeInHierarchy)
                {
                    backButtonNav.selectOnUp = m_lastGpSelectable[m_currentViewIndex];
                    m_backButton.navigation = backButtonNav;
                    break;
                }
            }
        }

        public void ReSetDefault()
        {
            m_MoveValueSliderXEP.value = m_xMoveSpeedDefault;
            m_RotValueSliderYEP.value = m_yRotationDefault;
            m_moveToggleKeysEP.isOn = m_customToggleDefaults;
            m_rotToggleKeysEP.isOn = m_customToggleDefaults;
            m_playerXRotInvertToggles[m_currentViewIndex].isOn = m_xRotInvertDefaults[m_currentViewIndex];
            m_playerYRotInvertToggles[m_currentViewIndex].isOn = m_yRotInvertDefaults[m_currentViewIndex];

            ResetPlayerViewRebinds?.Invoke();   //Active KeyRebindButtons in the currently active PlayerView shall reset their bindings.
        }
        #endregion
    }
}