using ThreeDeePongProto.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ThreeDeePongProto.Settings
{
    public class GraphicSettings : MonoBehaviour
    {
        [SerializeField] private TMP_Dropdown m_qualityDropdown;
        [SerializeField] private TMP_Dropdown m_resolutionDropdown;
        [SerializeField] private Toggle m_fullscreenToggle;
        [SerializeField] private TMP_Dropdown m_screenSplitDropdown;

        private void Awake()
        {
            m_screenSplitDropdown.value = (int)GameManager.Instance.ECameraMode;
        }

        public void SetActiveCameras()
        {
            GameManager.Instance.ECameraMode = (ECameraModi)m_screenSplitDropdown.value;
        }
    }
}