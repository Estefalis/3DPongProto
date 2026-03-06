using System;
using System.Collections.Generic;
using System.Linq;
using ThreeDeePongProto.Shared.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ThreeDeePongProto.Shared.Settings
{
    public class ControlSettings : MonoBehaviour
    {
        private enum ESliderType
        {
            MoveSpeed,
            RotateSpeed
        }

        #region Axis Sensitivity
        [Header("Movement Speed")]
        [SerializeField] private Slider m_moveSpeedSlider;
        [SerializeField] private TextMeshProUGUI m_MovespeedText;
        [SerializeField] private Toggle m_moveSpeedToggle;

        [Header("Rotation Speed")]
        [SerializeField] private Slider m_rotateSpeedSlider;
        [SerializeField] private TextMeshProUGUI m_RotationText;
        [SerializeField] private Toggle m_rotateSpeedToggle;
        #endregion

        [SerializeField, Range(0.1f, 5.0f)] private float m_adjustSliderStep = 1.0f;

        [Header("Player-Specific Views")]
        [SerializeField] private Button[] m_playerTabButtons;
        [SerializeField] private Transform[] m_playerContentViews;

        [Header("Player-Specific Axis-Inversion")]
        #region Axis Inversion
        [SerializeField] private Toggle[] m_invertXAxisToggles;
        [SerializeField] private Toggle[] m_invertYAxisToggles;
        [SerializeField] private TextMeshProUGUI[] m_invertXAxisLabels;
        [SerializeField] private TextMeshProUGUI[] m_invertYAxisLabels;
        #endregion

        #region Changing-UI-Navigation
        [Header("Change UI Navigation")]
        [SerializeField] private Transform m_zoomGroup;             //Last group above playerButtons.
        private List<Selectable> m_zoomSelectables = new();

        [SerializeField] private Selectable[] m_lastKbSelectable;   //ScrollView 1-4
        [SerializeField] private Selectable[] m_lastGpSelectable;   //ScrollView 1-4
        [SerializeField] private Button m_resetButton;              //Reset to Defalut Button
        [SerializeField] private Button m_backButton;               //Rebind to BackButton
        #endregion

        [Header("Key Rebind")]
        [SerializeField] private bool m_resetKeyBindsAlso = false;
        internal static event Action ResetPlayerViewRebinds;        //Subscriber: KeyRebinding.

        private ControlSettingsData m_globalControlData;
        private List<PlayerProfileData> m_playerProfiles;

        private int m_currentViewIndex = 0;
        private readonly float m_defMoveSpeedX = 15.0f;
        private readonly float m_defRotSpeedY = 3.0f;         //Subscriber: RebindManagerSP.  //<---------------------------
        
        private void OnEnable()
        {
            m_globalControlData = SettingsManager.Instance.CurrentSettings.Control;
            m_playerProfiles = PlayerProfileManager.Instance.PlayerProfiles;

            SettingsManager.Instance.OnSettingsChanged += SetUIElements;

            SetUIElements();    //Includes AddListeners();

            SetActiveScrollView(m_currentViewIndex);

            //To update navigation.SelectOnDown on playerIndex change.
            m_zoomSelectables = m_zoomGroup.GetComponentsInChildren<Selectable>(true).ToList();
        }

        private void OnDisable()
        {
            SettingsManager.Instance.OnSettingsChanged -= SetUIElements;
            RemoveListeners();
            //IMPORTANT: Save Manager Data!
            SettingsManager.Instance.SaveSettings();
            PlayerProfileManager.Instance.SaveProfiles();
        }

        private void Update()
        {
            if (!EventSystem.current.currentSelectedGameObject.TryGetComponent<Button>(out var button) && !m_playerTabButtons.Contains(button))
                return;

            for (int i = 0; i < m_playerTabButtons.Length; i++)
            {
                if (m_playerTabButtons[i] == button)
                    SetActiveScrollView(m_currentViewIndex);
            }
        }

        /// <summary>
        /// Subscribe Control-Elements to UnityEvents.
        /// </summary>
        #region UnRegister-Listener-Region
        private void AddListeners()
        {
            m_moveSpeedSlider.onValueChanged.AddListener(OnMoveSpeedChanged);
            m_rotateSpeedSlider.onValueChanged.AddListener(OnRotateSpeedChanged);

            m_moveSpeedToggle.onValueChanged.AddListener(OnMoveSpeedToggleChanged);
            m_rotateSpeedToggle.onValueChanged.AddListener(OnRotateToggleChanged);

            for (int i = 0; i < m_invertXAxisToggles.Length; i++)
            {
                int playerIndex = i; //Important for the lambda expression!
                m_invertXAxisToggles[i].onValueChanged.AddListener((isOn) => OnMoveAxisXInverted(playerIndex, isOn));
                m_invertYAxisToggles[i].onValueChanged.AddListener((isOn) => OnRotateAxisYInverted(playerIndex, isOn));
            }
        }

        /// <summary>
        /// Unsubscribe Control-Elements from UnityEvents.
        /// </summary>
        private void RemoveListeners()
        {
            m_moveSpeedSlider.onValueChanged.RemoveListener(OnMoveSpeedChanged);
            m_rotateSpeedSlider.onValueChanged.RemoveListener(OnRotateSpeedChanged);

            m_moveSpeedToggle.onValueChanged.RemoveListener(OnMoveSpeedToggleChanged);
            m_rotateSpeedToggle.onValueChanged.RemoveListener(OnRotateToggleChanged);

            for (int i = 0; i < m_invertXAxisToggles.Length; i++)
            {
                int playerIndex = i; //Important for the lambda expression!
                m_invertXAxisToggles[i].onValueChanged.RemoveListener((isOn) => OnMoveAxisXInverted(playerIndex, isOn));
                m_invertYAxisToggles[i].onValueChanged.RemoveListener((isOn) => OnRotateAxisYInverted(playerIndex, isOn));
            }
        }
        #endregion

        #region Public Button Methods
        public void DecreaseMoveSpeed() => AdjustSensitivity(ESliderType.MoveSpeed, false);
        public void IncreaseMoveSpeed() => AdjustSensitivity(ESliderType.MoveSpeed, true);

        public void DecreaseRotSpeed() => AdjustSensitivity(ESliderType.RotateSpeed, false);
        public void IncreaseRotSpeed() => AdjustSensitivity(ESliderType.RotateSpeed, true);
        #endregion

        #region Listener-Methods

        private void OnMoveSpeedChanged(float _sliderXValue)
        {
            m_globalControlData.MoveSpeedX = _sliderXValue;
            m_MovespeedText.text = $"{_sliderXValue:N2}";
        }

        private void OnRotateSpeedChanged(float _sliderYValue)
        {
            m_globalControlData.RotSpeedY = _sliderYValue;
            m_RotationText.text = $"{_sliderYValue:N2}";
        }

        private void OnMoveSpeedToggleChanged(bool _xDefIsOn)  //ToggleMoveX
        {
            m_globalControlData.UseDefaultMoveSpeed = _xDefIsOn;

            if (_xDefIsOn)
                m_moveSpeedSlider.value = m_defMoveSpeedX;  //Triggers MoveSlider Listener method.
        }

        private void OnRotateToggleChanged(bool _yDefIsOn)   //ToggleRotateY
        {
            m_globalControlData.UseDefaultRotateSpeed = _yDefIsOn;

            if (_yDefIsOn)
                m_rotateSpeedSlider.value = m_defRotSpeedY; //Triggers RotateSlider Listener method.
        }

        private void OnMoveAxisXInverted(int playerIndex, bool isOn)
        {
            m_playerProfiles[playerIndex].InvertMoveAxisX = isOn;
            SetUIElements();
        }

        private void OnRotateAxisYInverted(int playerIndex, bool isOn)
        {
            m_playerProfiles[playerIndex].InvertRotAxisY = isOn;
            SetUIElements();
        }

        public void SetActiveScrollView(int _playerIndex)
        {
            m_currentViewIndex = _playerIndex;
            for (int i = 0; i < m_playerContentViews.Length; i++)
                m_playerContentViews[i].gameObject.SetActive(i == _playerIndex);

            UpdateNavigationOnSwitch();
        }
        #endregion

        #region Custom Methods
        private void SetUIElements()
        {
            RemoveListeners();

            m_moveSpeedToggle.isOn = m_globalControlData.UseDefaultMoveSpeed;
            m_rotateSpeedToggle.isOn = m_globalControlData.UseDefaultRotateSpeed;

            m_moveSpeedSlider.value = m_globalControlData.MoveSpeedX;     //MoveSlider X
            m_rotateSpeedSlider.value = m_globalControlData.RotSpeedY;    //RotSlider Y

            m_MovespeedText.text = $"{m_moveSpeedSlider.value:N2}";
            m_RotationText.text = $"{m_rotateSpeedSlider.value:N2}";

            for (int i = 0; i < m_playerProfiles.Count; i++)
            {
                //Set X-Axis Toggle and it's text from each playerProfile.
                bool invertX = m_playerProfiles[i].InvertMoveAxisX;
                m_invertXAxisToggles[i].isOn = invertX;
                m_invertXAxisLabels[i].text = invertX ? "Inverted" : "Normal";

                //Set Y-Axis Toggle and it's text from each playerProfile.
                bool invertY = m_playerProfiles[i].InvertRotAxisY;
                m_invertYAxisToggles[i].isOn = invertY;
                m_invertYAxisLabels[i].text = invertY ? "Inverted" : "Normal";
            }

            AddListeners();
        }

        private void AdjustSensitivity(ESliderType _type, bool _increase)
        {
            Toggle targetToggle = null;
            Slider targetSlider = null;

            switch (_type)
            {
                case ESliderType.MoveSpeed:
                {
                    targetToggle = m_moveSpeedToggle;
                    targetSlider = m_moveSpeedSlider;
                    break;
                }
                case ESliderType.RotateSpeed:
                {
                    targetToggle = m_rotateSpeedToggle;
                    targetSlider = m_rotateSpeedSlider;
                    break;
                }
            }

            if (targetToggle.isOn)
            {
                targetToggle.isOn = false; //Listener method gets triggered.
            }

            if (targetSlider != null)
            {
                float absoluteStep = targetSlider.maxValue * (m_adjustSliderStep / 100.0f); //1.0f means a sliderStep of 1%.
                float step = _increase ? absoluteStep : -absoluteStep;
                targetSlider.value += step; //Triggers OnValueChanged-Listener automaticly.
            }
        }

        private void UpdateNavigationOnSwitch()
        {
            foreach (Selectable selectable in m_zoomSelectables)
            {
                if (selectable.gameObject.activeInHierarchy)
                {
                    Navigation navigation = selectable.navigation;
                    navigation.selectOnDown = m_playerTabButtons[m_currentViewIndex];
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

        //Public method for the general ResetButton of ControlSettings.
        public void ResetSettings()
        {
            SettingsManager.Instance.ResetControlSettings();

            if (m_resetKeyBindsAlso)
                ResetPlayerViewRebinds?.Invoke();   //KeyRebindButtons in the currently active PlayerView shall reset their bindings.
        }
        #endregion
    }
}