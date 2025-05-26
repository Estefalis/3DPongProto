using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum ECameraModi
{
    None = 0,
    SingleCam = 1,
    Vertical,
    Horizontal,
    Quartet = 4
}

namespace ThreeDeePongProto.Shared.Settings
{
    public class GraphicSettings : MonoBehaviour
    {
        #region Object-References
        [SerializeField] private TMP_Dropdown m_qualityDropdown;
        [SerializeField] private TMP_Dropdown m_resolutionDropdown;
        [SerializeField] private Toggle m_fullScreenToggle;                 //Listener method is IN Unity!
        [SerializeField] private TextMeshProUGUI m_fullScreenToggleText;
        [SerializeField] private TMP_Dropdown m_screenSplitDropdown;
        #endregion

        [SerializeField] private int m_systemQualityLevel;
        [SerializeField] private int m_currentResolutionIndex;
        [SerializeField] private bool m_defaultFullScreen = true;
        [SerializeField] private ECameraModi m_eCameraMode;

        private Resolution[] m_screenResolutions;
        public ECameraModi ECameraMode { get => m_eCameraMode; }

        private int m_maxScreenModiIndex;
        private List<string> m_screenModiList;

        #region Scriptable Variables
        [Header("Scriptable Objects")]
        [SerializeField] private GraphicUIStates m_graphicUIStates;
        [SerializeField] private MatchValues m_matchValues;
        [SerializeField] private MatchUIStates m_matchUIStates;
        #endregion

        #region Serialization
        private readonly string m_settingStatesFolderPath = "/SaveData/Settings-States";
        private readonly string m_graphicFileName = "/Graphic";
        private readonly string m_fileFormat = ".json";

        private readonly IPersistentData m_persistentData = new SerializingData();
        private readonly bool m_encryptionEnabled = false;
        #endregion

        private void Awake()
        {
            m_systemQualityLevel = QualitySettings.GetQualityLevel();
            GetAvailableResolutions();

            SetupSplitDropdown();

            if (m_graphicUIStates == null)
            {
#if UNITY_EDITOR
                Debug.LogWarning("GraphicSettings: Forgot to add a Scriptable Object in the Editor!");
#endif
                ReSetDefault();
            }
            //else LoadGraphicSettings(); moved to 'MenuNavigation.cs'.       
        }

        private void Start()
        {
            //TODO: InitialUISetup check for nulled Scriptable.
            InitialUISetup();
        }

        private void OnDisable()
        {
            m_persistentData.SaveData(m_settingStatesFolderPath, m_graphicFileName, m_fileFormat, m_graphicUIStates, m_encryptionEnabled, true);
        }

        private void GetAvailableResolutions()
        {
            m_screenResolutions = Screen.resolutions;

            m_resolutionDropdown.ClearOptions();

            //List for the variable Amount of available Resolution-Options on your system.
            List<string> resolutionOptionsList = new();

            int currentResolutionIndex = 0;
            for (int i = 0; i < m_screenResolutions.Length; i++)
            {
                //Creation of a formatted string to display the available system-resolutions in the UI-Dropdown.
                //Adding the refreshRate prevents confusion on double entries.
                string resolution = $"{m_screenResolutions[i].width} x {m_screenResolutions[i].height} @{m_screenResolutions[i].refreshRate}hz";
                resolutionOptionsList.Add(resolution);

                //Screen-Width and -Height have to be handled separate. 
                if (m_screenResolutions[i].width == Screen.currentResolution.width && m_screenResolutions[i].height == Screen.currentResolution.height)
                {
                    currentResolutionIndex = i;
                    //System-ResolutionIndex.
                    m_currentResolutionIndex = i;
                }
            }

            m_resolutionDropdown.AddOptions(resolutionOptionsList);
            m_resolutionDropdown.value = currentResolutionIndex;
            m_resolutionDropdown.RefreshShownValue();
        }

        private void SetupSplitDropdown()
        {
            m_screenModiList = new();
            uint currentPlayer = (uint)m_matchUIStates.EPlayerAmount;
            //Automatize the shown ECameraModi in the 'm_screenSplitDropdown' based on the player in the match.

            switch (currentPlayer)
            {
                case 0:
                {
                    m_eCameraMode = ECameraModi.None;
                    m_maxScreenModiIndex = 0;
                    break;
                }
                case 1:
                {
                    m_eCameraMode = ECameraModi.SingleCam;
                    m_maxScreenModiIndex = 1;
                    break;
                }
                case 2:
                {
                    //TODO: Switch based on PlayerSetting.
                    m_eCameraMode = ECameraModi.Horizontal;
                    m_maxScreenModiIndex = 2;
                    break;
                }
                case 4:
                {
                    m_eCameraMode = ECameraModi.Quartet;
                    m_maxScreenModiIndex = 4;
                    break;
                }
                default:
                {
                    m_eCameraMode = ECameraModi.SingleCam;
                    m_maxScreenModiIndex = 1;
                    break;
                }
            }

            for (int i = 0; i < m_maxScreenModiIndex + 1; i++)
            {
                m_screenModiList.Add($"{(ECameraModi)i}");
            }

            m_screenSplitDropdown.ClearOptions();
            m_screenSplitDropdown.AddOptions(m_screenModiList);

            if (m_graphicUIStates != null)
                m_screenSplitDropdown.value = (int)m_graphicUIStates.SetCameraMode;
            else
                m_screenSplitDropdown.value = (int)m_eCameraMode;

            m_screenSplitDropdown.RefreshShownValue();
        }

        private void InitialUISetup()
        {
            m_qualityDropdown.value = m_graphicUIStates.QualityLevelIndex;
            m_resolutionDropdown.value = m_graphicUIStates.SelectedResolutionIndex;
            m_fullScreenToggle.isOn = m_graphicUIStates.FullScreenMode;
            SetFullScreenText(m_fullScreenToggle.isOn);
            m_screenSplitDropdown.value = (int)m_graphicUIStates.SetCameraMode;
        }

        private void SetFullScreenText(bool _fullScreen)
        {
            switch (_fullScreen)
            {
                case true:
                    m_fullScreenToggleText.text = "On";
                    break;
                case false:
                    m_fullScreenToggleText.text = "Off";
                    break;
            }
        }

        //Set by UI-SplitScreenDropdown.
        public void SetActiveCameras()
        {
            m_graphicUIStates.SetCameraMode = (ECameraModi)m_screenSplitDropdown.value;
        }

        public void SetGraphicQuality(int _qualityIndex)
        {
            QualitySettings.SetQualityLevel(_qualityIndex);
            m_qualityDropdown.value = _qualityIndex;

            if (m_graphicUIStates != null)
                m_graphicUIStates.QualityLevelIndex = _qualityIndex;
        }

        public void SetResolution(int _resolutionIndex)
        {
            Resolution resolution = m_screenResolutions[_resolutionIndex];
            Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
            m_resolutionDropdown.value = _resolutionIndex;

            if (m_graphicUIStates != null)
                m_graphicUIStates.SelectedResolutionIndex = _resolutionIndex;
        }

        public void SetFullScreen(bool _setFullScreen)
        {
            Screen.fullScreen = _setFullScreen;
            m_fullScreenToggle.isOn = _setFullScreen;
            SetFullScreenText(m_fullScreenToggle.isOn);

            if (m_graphicUIStates != null)
                m_graphicUIStates.FullScreenMode = _setFullScreen;
        }

        public void ReSetDefault()
        {
            m_qualityDropdown.value = m_systemQualityLevel;
            //Index equal to your System-Resolution, set by 'GetAvailableResolutions();'.
            m_resolutionDropdown.value = m_currentResolutionIndex;
            m_fullScreenToggle.isOn = m_defaultFullScreen;
            SetFullScreenText(m_fullScreenToggle.isOn);
            m_screenSplitDropdown.value = (int)m_eCameraMode;
        }
    }
}