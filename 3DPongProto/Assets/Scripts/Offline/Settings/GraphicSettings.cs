using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum ECameraModi
{
    SingleCam,
    TwoHorizontal,
    TwoVertical,
    FourSplit
}

namespace ThreeDeePongProto.Offline.Settings
{
    public class GraphicSettings : MonoBehaviour
    {
        //[SerializeField] private MatchManager m_matchManager;

        #region Object-References
        [SerializeField] private TMP_Dropdown m_qualityDropdown;
        [SerializeField] private TMP_Dropdown m_resolutionDropdown;
        [SerializeField] private Toggle m_fullscreenToggle;
        [SerializeField] private TMP_Dropdown m_screenSplitDropdown;
        #endregion

        [SerializeField] private bool m_defaultFullscreen = true;
        [SerializeField] private ECameraModi m_eCameraMode;

        #region Scriptable Variables
        [Header("Scriptable Variables")]
        [SerializeField] private GraphicVariables m_graphicVariables;
        [SerializeField] private MatchVariables m_matchVariables;
        #endregion

        private Resolution[] m_screenResolutions;
        private int m_currentResolutionIndex;
        private int m_systemQualityLevel;

        public ECameraModi ECameraMode { get => m_eCameraMode; }

        private void Awake()
        {
            m_systemQualityLevel = QualitySettings.GetQualityLevel();

            GetAvailableResolutions();

            if (m_graphicVariables == null)
            {
#if UNITY_EDITOR
                Debug.LogWarning("GraphicSettings: Forgot to add a Scriptable Object in the Editor!");
#endif
                UseDefaultSettings();
            }
            else
            {
//#if UNITY_EDITOR
//                UseDefaultSettings();
//#else
                m_qualityDropdown.value = m_graphicVariables.QualityLevel;
                m_resolutionDropdown.value = m_graphicVariables.SelectedResolutionIndex;
                m_fullscreenToggle.isOn = m_graphicVariables.FullScreenMode;
                //m_screenSplitDropdown.value = (int)GameManager.Instance.ECameraMode;
                m_screenSplitDropdown.value = m_graphicVariables.ActiveCameraIndex;
//#endif
            }
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

        //TODO: Ggf. Methode in MatchManager transferieren.
        public void SetActiveCameras()
        {
            //No cast needed on saving an index.
            m_graphicVariables.ActiveCameraIndex = m_screenSplitDropdown.value;
            //GameManager.Instance.ECameraMode = (ECameraModi)m_screenSplitDropdown.value;
            m_matchVariables.SetCameraMode = (ECameraModi)m_screenSplitDropdown.value;
        }

        public void SetGraphicQuality(int _qualityIndex)
        {
            QualitySettings.SetQualityLevel(_qualityIndex);
            m_qualityDropdown.value = _qualityIndex;

            if (m_graphicVariables != null)
                m_graphicVariables.QualityLevel = _qualityIndex;
        }

        public void SetResolution(int _resolutionIndex)
        {
            Resolution resolution = m_screenResolutions[_resolutionIndex];
            Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
            m_resolutionDropdown.value = _resolutionIndex;

            if (m_graphicVariables != null)
                m_graphicVariables.SelectedResolutionIndex = _resolutionIndex;
        }

        public void SetFullscreen(bool _setFullscreen)
        {
            Screen.fullScreen = _setFullscreen;
            m_fullscreenToggle.isOn = _setFullscreen;

            if (m_graphicVariables != null)
                m_graphicVariables.FullScreenMode = _setFullscreen;
        }

        private void UseDefaultSettings()
        {
            m_qualityDropdown.value = m_systemQualityLevel;
            //Index equal to your System-Resolution, set by 'GetAvailableResolutions();'.
            m_resolutionDropdown.value = m_currentResolutionIndex;
            m_fullscreenToggle.isOn = m_defaultFullscreen;
            m_screenSplitDropdown.value = (int)m_eCameraMode;
        }
    }
}