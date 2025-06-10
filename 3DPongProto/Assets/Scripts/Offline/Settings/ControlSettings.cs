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
        private enum EAxisToggle
        {
            X = 0,
            Y = 1,
            Z = 2,
        }

        #region Axis Sensitivity
        //'_connectedToggle.isOn', 'm_moveToggleKey.isOn' and 'm_rotationToggleKey.isOn' need to be inverted together, to enable/disable reduce-/increaseButtons and slider-changes. Including InitialUISetup() switch.
        [Header("Movement Speed")]
        [SerializeField] private Slider m_MovespeedSliderX;
        [SerializeField] private TextMeshProUGUI m_MovespeedText;
        [SerializeField] private Toggle m_moveToggleKey;
        [SerializeField] private bool m_moveToggleXDefault;

        [Header("Rotation Speed")]
        [SerializeField] private Slider m_RotationSliderY;
        [SerializeField] private TextMeshProUGUI m_RotationText;
        [SerializeField] private Toggle m_rotationToggleKey;
        [SerializeField] private bool m_rotToggleYDefault;
        [Space]
        [SerializeField, Range(1, 20)] float m_moveSpeedDefaultX = 10f;
        [SerializeField, Range(1, 5)] float m_rotSpeedDefaultY = 2.5f;
        [SerializeField, Range(1, 10)] private float m_adjustSliderStep = 2.0f;

        private readonly Dictionary<Toggle, Slider> m_toggleSliderConnectX = new();
        private readonly Dictionary<Toggle, Slider> m_toggleSliderConnectY = new();

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
        internal static event Action<int> PlayerViewIndex;        //Subscriber: RebindManagerSP.
        internal static event Action ResetPlayerViewRebinds;      //Subscriber: RebindManagerSP.
        #endregion

        #region Axis Inversion
        [Header("Axis Inversion")]
        [SerializeField] private Toggle[] m_xAxisInvertToggles;
        [SerializeField] private TextMeshProUGUI[] m_xAxisTMPUGUI;
        [SerializeField] private Toggle[] m_yAxisInvertToggles;
        [SerializeField] private TextMeshProUGUI[] m_yAxisTMPUGUI;

        [SerializeField] private bool m_moveDirInvertDefault;
        [SerializeField] private bool m_rotDirInvertDefault;
        [Space]
        #endregion

        [Header("Key Rebind")]
        [SerializeField] private bool m_resetKeyBindsAlso = false;

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

            m_toggleSliderConnectX.Add(m_moveToggleKey, m_MovespeedSliderX);
            m_toggleSliderConnectY.Add(m_rotationToggleKey, m_RotationSliderY);

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
            m_MovespeedSliderX.onValueChanged.AddListener(MoveSpeedSliderChanges);
            m_RotationSliderY.onValueChanged.AddListener(RotationSpeedSliderChanges);

            m_moveToggleKey.onValueChanged.AddListener(MoveSpeedToggleChanges);
            m_rotationToggleKey.onValueChanged.AddListener(RotSpeedToggleChanges);

            for (int x = 0; x < m_xAxisInvertToggles.Length; x++)
                m_xAxisInvertToggles[x].onValueChanged.AddListener(MoveAxisXInversion);

            for (int y = 0; y < m_yAxisInvertToggles.Length; y++)
                m_yAxisInvertToggles[y].onValueChanged.AddListener(RotationAxisYInversion);
        }

        /// <summary>
        /// Unsubscribe VolumeControl-Elements from UnityEvents.
        /// </summary>
        private void RemoveSliderAndToggleListener()
        {
            m_MovespeedSliderX.onValueChanged.RemoveListener(MoveSpeedSliderChanges);
            m_RotationSliderY.onValueChanged.RemoveListener(RotationSpeedSliderChanges);

            m_moveToggleKey.onValueChanged.RemoveListener(MoveSpeedToggleChanges);
            m_rotationToggleKey.onValueChanged.RemoveListener(RotSpeedToggleChanges);

            for (int x = 0; x < m_xAxisInvertToggles.Length; x++)
                m_xAxisInvertToggles[x].onValueChanged.RemoveListener(MoveAxisXInversion);

            for (int y = 0; y < m_yAxisInvertToggles.Length; y++)
                m_yAxisInvertToggles[y].onValueChanged.RemoveListener(RotationAxisYInversion);
        }
        #endregion

        #region Listener-Methods
        private void MoveSpeedSliderChanges(float _sliderXValue)
        {
            if (m_controlUIValues.Length < 1)
                return;

            if (!m_moveToggleKey.isOn)
            {
                for (int x = 0; x < m_controlUIValues.Length; x++)
                {
                    if (m_controlUIValues[x] != null)
                        m_controlUIValues[x].LastXMoveSpeed = _sliderXValue; //One MoveSpeed for all! Player1-4 SO.
                }

                m_MovespeedText.text = $"{_sliderXValue:N2}";
            }
        }

        private void RotationSpeedSliderChanges(float _sliderYValue)
        {
            if (m_controlUIValues.Length < 1)
                return;

            if (!m_rotationToggleKey.isOn)
            {
                for (int y = 0; y < m_controlUIValues.Length; y++)
                {
                    if (m_controlUIValues[y] != null)
                        m_controlUIValues[y].LastYRotSpeed = _sliderYValue; //One RotationSpeed for all! Player1-4 SO.
                }

                m_RotationText.text = $"{_sliderYValue:N2}";
            }
        }

        private void MoveSpeedToggleChanges(bool _toggleX)
        {
            if (m_controlUIStates.Length < 1)
                return;

            for (int x = 0; x < m_controlUIStates.Length; x++)
            {
                if (m_controlUIStates[x] != null)
                    m_controlUIStates[x].CustomXSensitivity = _toggleX;
            }

            switch (_toggleX)
            {
                case false:
                {
                    m_MovespeedSliderX.value = m_controlUIValues[0].LastXMoveSpeed;
                    m_MovespeedText.text = $"{m_controlUIValues[0].LastXMoveSpeed:N2}";
                    m_MovespeedSliderX.interactable = true;
                    break;
                }
                case true:
                {
                    m_MovespeedSliderX.value = m_moveSpeedDefaultX; //MoveSlider X
                    m_MovespeedText.text = $"{m_moveSpeedDefaultX:N2}";
                    m_MovespeedSliderX.interactable = false;
                    break;
                }
            }
        }

        private void RotSpeedToggleChanges(bool _toggleY)
        {
            if (m_controlUIStates.Length < 1)
                return;

            for (int y = 0; y < m_controlUIStates.Length; y++)
                m_controlUIStates[y].CustomYSensitivity = _toggleY;

            switch (_toggleY)
            {
                case false:
                {
                    m_RotationSliderY.value = m_controlUIValues[0].LastYRotSpeed;
                    m_RotationText.text = $"{m_controlUIValues[0].LastYRotSpeed:N2}";
                    m_RotationSliderY.interactable = true;
                    break;
                }
                case true:
                {
                    m_RotationSliderY.value = m_rotSpeedDefaultY;  //RotSlider Y
                    m_RotationText.text = $"{m_rotSpeedDefaultY:N2}";
                    m_RotationSliderY.interactable = false;
                    break;
                }
            }
        }

        private void MoveAxisXInversion(bool _xAxisInversion)
        {
            if (m_controlUIStates.Length < 1)
                return;

            for (int i = 0; i < m_controlUIStates.Length; i++)
            {
                if (m_controlUIStates[i] != null && m_currentViewIndex == i)
                {
                    m_controlUIStates[i].InvertXAxis = _xAxisInversion;
                    SetAxisToggleText((int)EAxisToggle.X, m_currentViewIndex, _xAxisInversion);
                }
            }
        }

        private void RotationAxisYInversion(bool _yAxisInversion)
        {
            if (m_controlUIStates.Length < 1)
                return;

            for (int i = 0; i < m_controlUIStates.Length; i++)
            {
                if (m_controlUIStates[i] != null && m_currentViewIndex == i)
                {
                    m_controlUIStates[i].InvertYAxis = _yAxisInversion;
                    SetAxisToggleText((int)EAxisToggle.Y, m_currentViewIndex, _yAxisInversion);
                }
            }
        }

        private void SetAxisToggleText(int _enumAxis, int _currentPlayerIndex, bool _axisToggleIsOn)
        {
            switch (_enumAxis)
            {
                case 0: //Enum's X-Axis.
                {
                    m_xAxisTMPUGUI[_currentPlayerIndex].text = _axisToggleIsOn == false ? "Normal" : "Inverted";
                    break;
                }
                case 1: //Enum's Y-Axis.
                {
                    m_yAxisTMPUGUI[_currentPlayerIndex].text = _axisToggleIsOn == false ? "Normal" : "Inverted";
                    break;
                }
                case 2: //Enum's Z-Axis.
                {
                    break;
                }
                default:
                    break;
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
            if (m_controlUIStates == null)
            {
                m_MovespeedSliderX.value = m_moveSpeedDefaultX;    //MoveSlider X
                m_MovespeedText.text = $"{m_moveSpeedDefaultX:N2}";
                m_RotationSliderY.value = m_rotSpeedDefaultY;  //RotSlider Y
                m_RotationText.text = $"{m_rotSpeedDefaultY:N2}";

                m_moveToggleKey.isOn = m_moveToggleXDefault;
                m_rotationToggleKey.isOn = m_rotToggleYDefault;

                SetAxisToggleText((int)EAxisToggle.X, m_currentViewIndex, m_moveDirInvertDefault);
                SetAxisToggleText((int)EAxisToggle.Y, m_currentViewIndex, m_rotDirInvertDefault);
            }
            else
            {
                m_MovespeedSliderX.value = m_controlUIValues[0].LastXMoveSpeed;   //MoveSlider X
                m_MovespeedText.text = $"{m_controlUIValues[0].LastXMoveSpeed:N2}";
                m_RotationSliderY.value = m_controlUIValues[0].LastYRotSpeed;  //RotSlider Y
                m_RotationText.text = $"{m_controlUIValues[0].LastYRotSpeed:N2}";

                m_moveToggleKey.isOn = m_controlUIStates[0].CustomXSensitivity;
                m_rotationToggleKey.isOn = m_controlUIStates[0].CustomYSensitivity;

                m_xAxisInvertToggles[m_currentViewIndex].isOn = m_controlUIStates[m_currentViewIndex].InvertXAxis;
                m_yAxisInvertToggles[m_currentViewIndex].isOn = m_controlUIStates[m_currentViewIndex].InvertYAxis;

                SetAxisToggleText((int)EAxisToggle.X, m_currentViewIndex, m_controlUIStates[m_currentViewIndex].InvertXAxis);
                SetAxisToggleText((int)EAxisToggle.Y, m_currentViewIndex, m_controlUIStates[m_currentViewIndex].InvertYAxis);
            }
        }

        public void ReduceSliderValue(Toggle _connectedToggle)
        {
            //Get the corresponding Slider (Value) in the Dictionary, for each submitted Toggle (Key), by the Button inside Unity.
            Slider connectedSlider = null;

            bool moveToggleMatch = m_toggleSliderConnectX.ContainsKey(_connectedToggle) && _connectedToggle == m_moveToggleKey;
            bool rotationToggleMatch = m_toggleSliderConnectY.ContainsKey(_connectedToggle) && _connectedToggle == m_rotationToggleKey;

            if (moveToggleMatch)
                connectedSlider = m_toggleSliderConnectX[_connectedToggle];
            if (rotationToggleMatch)
                connectedSlider = m_toggleSliderConnectY[_connectedToggle];

            //Only if the submitted Toggle isn't on, then the Button can lower the SliderValue.
            if (!_connectedToggle.isOn)
                connectedSlider.value -= connectedSlider.maxValue * (0.01f * m_adjustSliderStep);
        }

        public void IncreaseSliderValue(Toggle _connectedToggle)
        {
            //Get the corresponding Slider (Value) in the Dictionary, for each submitted Toggle (Key), by the Button inside Unity.
            Slider connectedSlider = null;

            bool moveToggleMatch = m_toggleSliderConnectX.ContainsKey(_connectedToggle) && _connectedToggle == m_moveToggleKey;
            bool rotationToggleMatch = m_toggleSliderConnectY.ContainsKey(_connectedToggle) && _connectedToggle == m_rotationToggleKey;

            if (moveToggleMatch)
                connectedSlider = m_toggleSliderConnectX[_connectedToggle];
            if (rotationToggleMatch)
                connectedSlider = m_toggleSliderConnectY[_connectedToggle];

            //Only if the submitted Toggle isn't on, then the Button can increase the SliderValue.
            if (!_connectedToggle.isOn)
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
            m_MovespeedSliderX.value = m_moveSpeedDefaultX;
            m_RotationSliderY.value = m_rotSpeedDefaultY;
            m_moveToggleKey.isOn = m_rotToggleYDefault;
            m_rotationToggleKey.isOn = m_rotToggleYDefault;
            m_xAxisInvertToggles[m_currentViewIndex].isOn = m_moveDirInvertDefault;
            m_yAxisInvertToggles[m_currentViewIndex].isOn = m_rotDirInvertDefault;

            if (m_resetKeyBindsAlso)
                ResetPlayerViewRebinds?.Invoke();   //KeyRebindButtons in the currently active PlayerView shall reset their bindings.
        }
        #endregion
    }
}