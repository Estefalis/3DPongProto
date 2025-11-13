using System;
using System.Collections.Generic;
using System.Linq;
using ThreeDeePongProto.Shared.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
//using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public enum ECameraModi
{
    None = 0,
    SingleCam = 1,
    Horizontal = 2,
    Vertical = 3,
    Quartet = 4
}

namespace ThreeDeePongProto.Shared.Settings
{
    public class GraphicSettings : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TMP_Dropdown m_qualityDropdown;
        [SerializeField] private TMP_Dropdown m_resolutionDropdown;
        [SerializeField] private Toggle m_fullScreenToggle;
        [SerializeField] private TextMeshProUGUI m_fullScreenToggleText;
        [SerializeField] private TMP_Dropdown m_screenSplitDropdown;
        [SerializeField] private Slider m_brightnessSlider;
        [SerializeField] private TextMeshProUGUI m_brightnessText;
        [SerializeField] private Toggle m_brightnessToggle;
        //[SerializeField] private Volume m_globalVolume; //URP.

        [SerializeField, Range(0.1f, 10.0f)] private float m_adjustSliderStep = 1.0f;

        private List<Resolution> m_availableResolutions = new();
        //private readonly float m_defaultBrightness = 0.0001f;

        private GraphicSettingsData m_graphicData;

        private void OnEnable()
        {
            m_graphicData = SettingsManager.Instance.CurrentSettings.Graphic;
            SettingsManager.Instance.OnSettingsChanged += SetUIElements;
            SetUIElements();    //Includes AddListeners();
        }

        private void OnDisable()
        {
            SettingsManager.Instance.OnSettingsChanged -= SetUIElements;
            RemoveListeners();
            //SettingsManager.Instance.SaveSettings();    //While there is no ApplyButton. Or none wanted.
        }

        #region UnRegister-Listener-Region
        private void AddListeners()
        {
            m_qualityDropdown.onValueChanged.AddListener(OnQualityChanged);
            m_resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
            m_fullScreenToggle.onValueChanged.AddListener(OnFullScreenChanged);
            m_screenSplitDropdown.onValueChanged.AddListener(OnCameraModeChanged);
            m_brightnessSlider.onValueChanged.AddListener(OnBrightnessChanged);
            m_brightnessToggle.onValueChanged.AddListener(OnBrightnessToggleChanged);
        }

        private void RemoveListeners()
        {
            m_qualityDropdown.onValueChanged.RemoveListener(OnQualityChanged);
            m_resolutionDropdown.onValueChanged.RemoveListener(OnResolutionChanged);
            m_fullScreenToggle.onValueChanged.RemoveListener(OnFullScreenChanged);
            m_screenSplitDropdown.onValueChanged.RemoveListener(OnCameraModeChanged);
            m_brightnessSlider.onValueChanged.RemoveListener(OnBrightnessChanged);
            m_brightnessToggle.onValueChanged.RemoveListener(OnBrightnessToggleChanged);
        }
        #endregion

        #region Listener-Methods
        private void OnQualityChanged(int index) => m_graphicData.QualityLevelIndex = index;
        private void OnResolutionChanged(int index) => m_graphicData.ResolutionString = m_availableResolutions[index].ToString();
        private void OnFullScreenChanged(bool isOn) => m_graphicData.FullScreenMode = isOn;
        private void OnCameraModeChanged(int index) => m_graphicData.splitDdValue = index;


        #region Public Button Methods
        public void DecreaseBrightness() => AdjustBrightness(false);
        public void IncreaseBrightness() => AdjustBrightness(true);
        #endregion 

        private void OnBrightnessChanged(float _value)
        {
            //SettingsManager valueChanges only.
            m_graphicData.Brightness = _value;

            //Slider > minValue = toggle value is false.
            if (m_graphicData.UseDefBrightness && _value > m_brightnessSlider.minValue)
                m_graphicData.UseDefBrightness = false;

            SetUIElements();
            ApplyBrightness();
        }

        private void OnBrightnessToggleChanged(bool _isOn)
        {
            m_graphicData.UseDefBrightness = _isOn;
            if (_isOn)
            {
                //Save current brightness before seting it to defaultValue.
                m_graphicData.BrightnessBeforeDefault = m_graphicData.Brightness;
                m_graphicData.Brightness = m_brightnessSlider.minValue;
            }
            else
            {
                //Reset saved brightnessValue to the saved amount.
                m_graphicData.Brightness = m_graphicData.BrightnessBeforeDefault;
            }

            SetUIElements();
            ApplyBrightness();
        }
        #endregion

        private void ApplyBrightness()
        {
            //if (m_globalVolume != null && m_globalVolume.profile.TryGet<ColorAdjustments>(out var colorAdjustments))
            //{
            //    colorAdjustments.postExposure.value = m_graphicData.Brightness;
            //}
        }

        private void SetUIElements()
        {
            RemoveListeners();

            //Quality
            m_qualityDropdown.value = m_graphicData.QualityLevelIndex;

            //Fullscreen
            m_fullScreenToggle.isOn = m_graphicData.FullScreenMode;
            m_fullScreenToggleText.text = m_graphicData.FullScreenMode ? "On" : "Off";

            m_brightnessToggle.isOn = m_graphicData.UseDefBrightness;
            m_brightnessSlider.value = m_graphicData.Brightness;
            m_brightnessText.text = $"{m_brightnessSlider.value:P0}";

            //Resolution & SplitScreen
            SetResolutionDropdown();
            SetSplitScreenDropdown();

            AddListeners();
        }

        private void SetResolutionDropdown()
        {
            m_availableResolutions = Screen.resolutions.ToList();
            m_resolutionDropdown.ClearOptions();

            //List for the variable Amount of available Resolution-Options on your system.
            List<string> resolutionOptions = new();
            int currentResolutionIndex = 0;

            for (int i = 0; i < m_availableResolutions.Count; i++)
            {
                resolutionOptions.Add(m_availableResolutions[i].ToString());

                //Find the Index of the saved resolution.
                if (m_availableResolutions[i].ToString() == m_graphicData.ResolutionString)
                {
                    currentResolutionIndex = i;
                }
            }

            m_resolutionDropdown.AddOptions(resolutionOptions);
            m_resolutionDropdown.value = currentResolutionIndex;
            m_resolutionDropdown.RefreshShownValue();
        }

        private void SetSplitScreenDropdown()
        {
            //Get Data from the SettingsManager.
            int playerCount = SettingsManager.Instance.CurrentSettings.Match.PlayerCount;
            m_screenSplitDropdown.interactable = playerCount == 2;
            m_screenSplitDropdown.ClearOptions();
            var options = new List<string>();

            //Create a new options list, based on the current playerCount.
            switch (playerCount)
            {
                case 1:
                    options.Add(ECameraModi.SingleCam.ToString());
                    break;
                case 2:
                    options.Add(ECameraModi.Vertical.ToString());
                    options.Add(ECameraModi.Horizontal.ToString());
                    break;
                case 4:
                    options.Add(ECameraModi.Quartet.ToString());
                    break;
                default: //For 0 Player or any other cases
                    options.Add(ECameraModi.None.ToString());
                    break;
            }

            //Fill the dropdown with thew new options.
            m_screenSplitDropdown.AddOptions(options);
            
            int ddValue = playerCount == 2 ? m_graphicData.splitDdValue : 0;
            m_screenSplitDropdown.SetValueWithoutNotify(ddValue);
            m_screenSplitDropdown.RefreshShownValue();
        }

        private void AdjustBrightness(bool _increase)
        {
            if (m_brightnessSlider == null)
                return;

            //If defaultBrightness-Mode is on, deactivate it. And start from slider's minValue.
            if (m_graphicData.UseDefBrightness)
            {
                m_graphicData.UseDefBrightness = false;

                m_brightnessSlider.value = m_brightnessSlider.minValue;

                //Notify the Toggle, but without triggering it's listener.
                m_brightnessToggle.SetIsOnWithoutNotify(false);
            }

            float absoluteStep = m_brightnessSlider.maxValue * (m_adjustSliderStep / 100.0f); //1.0f means a sliderStep of 1%.
            float step = _increase ? absoluteStep : -absoluteStep;
            m_brightnessSlider.value += step; //Triggers OnValueChanged-Listener automaticly.
        }

        //Set by UI-SplitScreenDropdown.
        public void SetActiveCameras()
        {
            m_graphicData.splitDdValue = m_screenSplitDropdown.value;    //Previous ECameraModi.
        }

        public void ApplyAndSave()
        {
            //Apply System-Settings.
            QualitySettings.SetQualityLevel(m_graphicData.QualityLevelIndex);
            Screen.fullScreen = m_graphicData.FullScreenMode;

            //Find and set the saved resolution.
            Resolution resToSet = m_availableResolutions.Find(r => r.ToString() == m_graphicData.ResolutionString);
            Screen.SetResolution(resToSet.width, resToSet.height, m_graphicData.FullScreenMode);

            //Save settings permanently.
            SettingsManager.Instance.SaveSettings();
        }

        //Public method for the general ResetButton of GraphicSettings.
        public void ResetSettings()
        {
            SettingsManager.Instance.ResetGraphicSettings();
        }
    }
}
