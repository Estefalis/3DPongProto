using UnityEngine;

namespace ThreeDeePongProto.Offline.Settings
{
    public class LoadSettingsValues : MonoBehaviour
    {
        #region Scriptable Variables
        [Header("Scriptable Variables")]
        [SerializeField] private VolumeUIStates m_volumeUIStates;
        [SerializeField] private VolumeUIValues m_volumeUIValues;
        [SerializeField] private GraphicUIStates m_graphicUiStates;
        [SerializeField] private BasicFieldValues m_basicFieldValues;
        [SerializeField] private MatchUIStates m_matchUIStates;
        [SerializeField] private MatchValues m_matchValues;
        [Space]
        [SerializeField] private ControlUIStates[] m_controlUIStatesEP;
        [SerializeField] private ControlUIValues[] m_controlUIValuesEP;
        #endregion

        #region Serialization
        private readonly string m_settingStatesFolderPath = "/SaveData/Settings-States";
        private readonly string m_settingsValuesFolderPath = "/SaveData/Settings-Values";
        private readonly string m_fieldSettingsPath = "/SaveData/FieldSettings";
        private readonly string m_volumeFileName = "/Volume";
        private readonly string m_graphicFileName = "/Graphic";
        private readonly string m_controlFileName = "/ControlPlayer";
        private readonly string m_matchFileName = "/Match";
        private readonly string m_fileFormat = ".json";

        private readonly IPersistentData m_persistentData = new SerializingData();
        private readonly bool m_encryptionEnabled = false;
        #endregion

        private void Awake()
        {
            #region Fill Scriptable Objects with Information
            LoadGraphicSettings();
            LoadVolumeSettings();
            LoadMatchSettings();
            LoadControlSettings();
            #endregion
        }

        private void LoadGraphicSettings()
        {
            GraphicUISettingsStates uiIndices = m_persistentData.LoadData<GraphicUISettingsStates>(m_settingStatesFolderPath, m_graphicFileName, m_fileFormat, m_encryptionEnabled);
            m_graphicUiStates.QualityLevelIndex = uiIndices.QualityLevelIndex;
            m_graphicUiStates.SelectedResolutionIndex = uiIndices.SelectedResolutionIndex;
            m_graphicUiStates.FullScreenMode = uiIndices.FullScreenMode;
            m_graphicUiStates.SetCameraMode = uiIndices.SetCameraMode;
        }

        private void LoadVolumeSettings()
        {
            VolumeUISettingsStates uiIndices = m_persistentData.LoadData<VolumeUISettingsStates>(m_settingStatesFolderPath, m_volumeFileName, m_fileFormat, m_encryptionEnabled);
            VolumeUISettingsValues uiValues = m_persistentData.LoadData<VolumeUISettingsValues>(m_settingsValuesFolderPath, m_volumeFileName, m_fileFormat, m_encryptionEnabled);

            m_volumeUIStates.MasterMuteIsOn = uiIndices.MasterMuteIsOn;
            m_volumeUIStates.BGMMuteIsOn = uiIndices.BGMMuteIsOn;
            m_volumeUIStates.SFXMuteIsOn = uiIndices.SFXMuteIsOn;

            m_volumeUIValues.LatestMasterVolume = uiValues.LatestMasterVolume;
            m_volumeUIValues.LatestBGMVolume = uiValues.LatestBGMVolume;
            m_volumeUIValues.LatestSFXVolume = uiValues.LatestSFXVolume;
        }

        private void LoadMatchSettings()
        {
            MatchUISettingsStates uiIndices = m_persistentData.LoadData<MatchUISettingsStates>(m_settingStatesFolderPath, m_matchFileName, m_fileFormat, m_encryptionEnabled);
            m_matchUIStates.EPlayerAmount = uiIndices.EPlayerAmount;
            m_matchUIStates.RotationReset = uiIndices.RotationReset;
            m_matchUIStates.InfiniteMatch = uiIndices.InfiniteMatch;
            //In this special case, the DropdownIndex is equal to the set Rounds and MaxPoints, because Index 0 is set as "InfinityValue".
            m_matchUIStates.LastRoundDdIndex = uiIndices.LastRoundDdIndex;
            m_matchUIStates.LastMaxPointDdIndex = uiIndices.LastMaxPointDdIndex;
            m_matchUIStates.MaxRounds = uiIndices.MaxRounds;
            m_matchUIStates.MaxPoints = uiIndices.MaxPoints;

            m_matchUIStates.FixRatio = uiIndices.FixRatio;
            m_matchUIStates.LastFieldWidthDdIndex = uiIndices.LastFieldWidthDdIndex;
            m_matchUIStates.LastFieldLengthDdIndex = uiIndices.LastFieldLengthDdIndex;

            m_matchUIStates.TPOneBacklineDdIndex = uiIndices.TPOneBacklineDdIndex;
            m_matchUIStates.TPTwoBacklineDdIndex = uiIndices.TPTwoBacklineDdIndex;
            m_matchUIStates.TPOneFrontlineDdIndex = uiIndices.TPOneFrontlineDdIndex;
            m_matchUIStates.TPTwoFrontlineDdIndex = uiIndices.TPTwoFrontlineDdIndex;

            BasicFieldSetup basicFieldSetup = m_persistentData.LoadData<BasicFieldSetup>(m_fieldSettingsPath, m_matchFileName, m_fileFormat, m_encryptionEnabled);
            m_basicFieldValues.SetGroundWidth = basicFieldSetup.SetGroundWidth;
            m_basicFieldValues.SetGroundLength = basicFieldSetup.SetGroundLength;
            m_basicFieldValues.FrontlineAdjustment = basicFieldSetup.FrontLineAdjustment;
            m_basicFieldValues.BacklineAdjustment = basicFieldSetup.BackLineAdjustment;
        }

        private void LoadControlSettings()
        {
            for (int i = 0; i < m_controlUIStatesEP.Length; i++)
            {
                if (m_controlUIStatesEP[i] != null)
                {
                    ControlUISettingsStates controlUISettingsStates = m_persistentData.LoadData<ControlUISettingsStates>(m_settingStatesFolderPath, m_controlFileName + $"{i}", m_fileFormat, m_encryptionEnabled);

                    m_controlUIStatesEP[i].InvertXAxis = controlUISettingsStates.InvertXAxis;
                    m_controlUIStatesEP[i].InvertYAxis = controlUISettingsStates.InvertYAxis;
                    m_controlUIStatesEP[i].CustomXSensitivity = controlUISettingsStates.CustomXSensitivity;
                    m_controlUIStatesEP[i].CustomYSensitivity = controlUISettingsStates.CustomYSensitivity;
                }
            }

            for (int i = 0; i < m_controlUIValuesEP.Length; i++)
            {
                if (m_controlUIValuesEP[i] != null)
                {
                    ControlUISettingsValues controlUISettingsValues = m_persistentData.LoadData<ControlUISettingsValues>(m_settingsValuesFolderPath, m_controlFileName + $"{i}", m_fileFormat, m_encryptionEnabled);

                    m_controlUIValuesEP[i].LastXMoveSpeed = controlUISettingsValues.LastXMoveSpeed;
                    m_controlUIValuesEP[i].LastYRotSpeed = controlUISettingsValues.LastYRotSpeed;
                }
            }
        }
    } 
}