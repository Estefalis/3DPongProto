using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum ECameraModi
{
    SingleCam,
    TwoVertical,
    TwoHorizontal,
    FourSplit,
    EndCount
}

namespace ThreeDeePongProto.Offline.Settings
{
    public class GraphicSettings : MonoBehaviour
    {
        #region Object-References
        [SerializeField] private TMP_Dropdown m_qualityDropdown;
        [SerializeField] private TMP_Dropdown m_resolutionDropdown;
        [SerializeField] private Toggle m_fullscreenToggle;
        [SerializeField] private TMP_Dropdown m_screenSplitDropdown;
        #endregion

        [SerializeField] private int m_systemQualityLevel;
        [SerializeField] private int m_currentResolutionIndex;
        [SerializeField] private bool m_defaultFullscreen = true;
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

        private IPersistentData m_persistentData = new SerializingData();
        private bool m_encryptionEnabled = false;
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
            //else LoadGraphicSettings(); moved to 'MenuOrganisation.cs'.       
        }

        private void Start()
        {
            //TODO: InitialUISetup check for nulled Scriptables.
            InitialUISetup();
        }

        private void OnDisable()
        {
            m_persistentData.SaveData(m_settingStatesFolderPath, m_graphicFileName, m_fileFormat, m_graphicUIStates, m_encryptionEnabled, true);
        }

        private void InitialUISetup()
        {
            m_qualityDropdown.value = m_graphicUIStates.QualityLevelIndex;
            m_resolutionDropdown.value = m_graphicUIStates.SelectedResolutionIndex;
            m_fullscreenToggle.isOn = m_graphicUIStates.FullScreenMode;
            m_screenSplitDropdown.value = (int)m_graphicUIStates.SetCameraMode;
        }

        private void GetAvailableResolutions()
        {
            m_screenResolutions = Screen.resolutions;

            m_resolutionDropdown.ClearOptions();

            //List for the variable Amount of available Resolution-Options on your system.
            List<string> resolutionOptionsList = new List<string>();

            int currentResolutionIndex = 0;
            for (int i = 0; i < m_screenResolutions.Length; i++)
            {
                //Creation of a formatted string to display the available system-resolutions in the UI-Dropdown.
                //Adding the refreshrate prevents confusion on double entries.
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
            m_maxScreenModiIndex = currentPlayer switch
            {
                2 => m_maxScreenModiIndex = (int)ECameraModi.EndCount - 1,  //m_maxScreenModiIndex = EPlayerAmount.Four
                4 => m_maxScreenModiIndex = (int)ECameraModi.EndCount,     //m_maxScreenModiIndex = EPlayerAmount.EndCount
                _ => m_maxScreenModiIndex = (int)ECameraModi.EndCount,
            };

            for (int i = 0; i < m_maxScreenModiIndex; i++)
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

        //Set by UI-SplitscreenDropdown.
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

        public void SetFullscreen(bool _setFullscreen)
        {
            Screen.fullScreen = _setFullscreen;
            m_fullscreenToggle.isOn = _setFullscreen;

            if (m_graphicUIStates != null)
                m_graphicUIStates.FullScreenMode = _setFullscreen;
        }

        public void ReSetDefault()
        {
            m_qualityDropdown.value = m_systemQualityLevel;
            //Index equal to your System-Resolution, set by 'GetAvailableResolutions();'.
            m_resolutionDropdown.value = m_currentResolutionIndex;
            m_fullscreenToggle.isOn = m_defaultFullscreen;
            m_screenSplitDropdown.value = (int)m_eCameraMode;
        }
    }
}