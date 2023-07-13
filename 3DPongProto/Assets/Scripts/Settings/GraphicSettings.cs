using System.Collections.Generic;
using ThreeDeePongProto.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ThreeDeePongProto.Settings
{
    public class GraphicSettings : MonoBehaviour
    {
        #region Object-References
        [SerializeField] private TMP_Dropdown m_qualityDropdown;
        [SerializeField] private TMP_Dropdown m_resolutionDropdown;
        [SerializeField] private Toggle m_fullscreenToggle;
        [SerializeField] private TMP_Dropdown m_screenSplitDropdown;
        #endregion

        #region Scriptable Variables
        [Header("Scriptable Variables")]
        [SerializeField] private GraphicVariables m_graphicVariables;
        //[SerializeField] private GraphicVariables m_graphicDefaults;
        #endregion

        private Resolution[] m_screenResolutions;

        private void Awake()
        {
#if UNITY_EDITOR
            if (m_graphicVariables == null)
                Debug.LogWarning("GraphicVariables are null! Loading Default-Values is required.");
#endif
            m_screenSplitDropdown.value = (int)GameManager.Instance.ECameraMode;

            GetAvailableResolutions();
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
                }
            }

            m_resolutionDropdown.AddOptions(resolutionOptionsList);
            m_resolutionDropdown.value = currentResolutionIndex;
            m_resolutionDropdown.RefreshShownValue();
        }

        //TODO: Ggf. Methode in LevelManager transferieren.
        public void SetActiveCameras()
        {
            GameManager.Instance.ECameraMode = (ECameraModi)m_screenSplitDropdown.value;
            m_graphicVariables.ActiveCameraIndex = m_screenSplitDropdown.value;
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
                m_graphicVariables.ScreenMode = _setFullscreen;
        }
    }
}