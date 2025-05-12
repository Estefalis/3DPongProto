using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Indices set equal to the desired amount of players participating in matches of int 2 and/or 4.
/// </summary>
public enum EPlayerAmount
{
    One = 1,
    Two = 2,
    Four = 4
}

namespace ThreeDeePongProto.Shared.UI
{
    public class PreparationWindow : MonoBehaviour
    {
        #region SerializeField-Member-Variables
        //TODO: Implementing RoomName for Lan-/Net-Games!
        [SerializeField] private Transform m_playerTwoGroup;
        [SerializeField] private TMP_Dropdown m_playerAmountDd;
        [SerializeField] private EPlayerAmount m_registeredPlayers = EPlayerAmount.Two;

        [Header("InputField-Group")]
        [SerializeField] private TMP_InputField[] m_nameInputFields;
        [Space]
        [SerializeField] private Toggle[] m_inputFieldToggles;
        [Space]
        [SerializeField] private Toggle[] m_deviceToggles;
        [Space]
        [SerializeField] private Transform m_playerThreeIFGroup;
        [SerializeField] private Transform m_playerFourIFGroup;

        [SerializeField] private Button m_startButton;
        [SerializeField] private Button m_joinButton;

        #region Scriptable-Objects
        [Header("Scriptable Objects")]
        [SerializeField] private MatchUIStates m_matchUIStates;
        [SerializeField] private MatchValues m_matchValues;
        [SerializeField] private GraphicUIStates m_graphicUiStates;
        [SerializeField] private PlayerSOData[] m_playerSOData;
        #endregion
        #endregion

        private const uint m_MINPLAYER = 1;
        private int m_currentPlayers;
        private List<string> m_maxPlayerAmount;

        #region Serialization
        private readonly string m_settingsStatesFolderPath = "/SaveData/Settings-States";
        private readonly string m_playerDataFolderPath = "/SaveData/PlayerData";
        private readonly string m_playerDataSubPath = "/Player";
        private readonly string m_graphicFileName = "/Graphic";
        private readonly string m_matchFileName = "/Match";
        private readonly string m_fileFormat = ".json";

        private readonly IPersistentData m_persistentData = new SerializingData();
        private readonly bool m_encryptionEnabled = false;
        #endregion

        private static event Action<int> AToggleButtonAccess;

        private void Awake()
        {
            if (m_playerSOData.Length > 0)
            {
                for (int i = 0; i < m_playerSOData.Length; i++)
                {
                    if (m_playerSOData[i] != null)
                        SetPlayerUIData(i);
                }
            }
        }

        private void OnEnable()
        {
            if (m_playerSOData.Length < 1)   //0, while Lan- and Net-Preparation-Windows are just dummies.
                return;

            AToggleButtonAccess += ToggleButtonAccess;
                AddGroupListener();
        }

        private void OnDisable()
        {
            if (m_playerSOData.Length < 1)   //0, while Lan- and Net-Preparation-Windows are just dummies.
                return;

            AToggleButtonAccess -= ToggleButtonAccess;
                RemoveGroupListener();
        }

        private void Start()
        {
            if (m_playerSOData.Length < 1)   //0, while Lan- and Net-Preparation-Windows are just dummies.
                return;

            var currentPlayers = m_matchUIStates.EPlayerAmount;
                SetupWindow(m_matchUIStates.EGameConnectModi, currentPlayers);

            SetupMatchDropdowns();

            m_currentPlayers = (int)currentPlayers;
            ToggleButtonAccess(m_currentPlayers);
        }

        private void AddGroupListener()
        {
            m_playerAmountDd.onValueChanged.AddListener(delegate
            { OnPlayerAmountChanged(m_playerAmountDd); });

            m_inputFieldToggles[0].onValueChanged.AddListener(InputFieldOneToggleChanges);
            m_inputFieldToggles[1].onValueChanged.AddListener(InputFieldTwoToggleChanges);
            m_inputFieldToggles[2].onValueChanged.AddListener(InputFieldThreeToggleChanges);
            m_inputFieldToggles[3].onValueChanged.AddListener(InputFieldFourToggleChanges);

            m_deviceToggles[0].onValueChanged.AddListener(DeviceToggleOneChanges);
            m_deviceToggles[1].onValueChanged.AddListener(DeviceToggleTwoChanges);
            m_deviceToggles[2].onValueChanged.AddListener(DeviceToggleThreeChanges);

            m_deviceToggles[3].onValueChanged.AddListener(DeviceToggleFourChanges);
        }

        private void RemoveGroupListener()
        {
            m_playerAmountDd.onValueChanged.RemoveListener(delegate
        { OnPlayerAmountChanged(m_playerAmountDd); });

            m_inputFieldToggles[0].onValueChanged.RemoveListener(InputFieldOneToggleChanges);
            m_inputFieldToggles[1].onValueChanged.RemoveListener(InputFieldTwoToggleChanges);
            m_inputFieldToggles[2].onValueChanged.RemoveListener(InputFieldThreeToggleChanges);
            m_inputFieldToggles[3].onValueChanged.RemoveListener(InputFieldFourToggleChanges);

            m_deviceToggles[0].onValueChanged.RemoveListener(DeviceToggleOneChanges);
            m_deviceToggles[1].onValueChanged.RemoveListener(DeviceToggleTwoChanges);
            m_deviceToggles[2].onValueChanged.RemoveListener(DeviceToggleThreeChanges);

            m_deviceToggles[3].onValueChanged.RemoveListener(DeviceToggleFourChanges);
        }

        #region Start_Setup
        private void SetPlayerUIData(int _playerIndex)
        {
            if (m_nameInputFields[_playerIndex] != null)
                m_nameInputFields[_playerIndex].text = m_playerSOData[_playerIndex].PlayerName;
            if (m_inputFieldToggles[_playerIndex] != null)
                m_inputFieldToggles[_playerIndex].isOn = m_playerSOData[_playerIndex].KeepNameOnLoad;
            if (m_deviceToggles[_playerIndex] != null)
                m_deviceToggles[_playerIndex].isOn = m_playerSOData[_playerIndex].DefaultKeyboard;
        }

        private void SetupWindow(EGameConnectionModi _connectMode, EPlayerAmount _ePlayerAmount)
        {
            switch (_connectMode)
            {
                case EGameConnectionModi.LocalGame:
                {
                    switch (_ePlayerAmount)
                    {
                        //TODO: PlayerAmount 1 vs NPC in Shared mode?
                        case EPlayerAmount.One:
                            ObjectsToHide(false, false, false);
                            break;
                        case EPlayerAmount.Two:
                        {
                            //PlayerCharacter 3 invisible, PlayerCharacter 4 invisible, Group 2 visible, TextWidths large.
                            ObjectsToHide(false, false, true);
                            break;
                        }
                        case EPlayerAmount.Four:
                        {
                            //PlayerCharacter 3 visible, PlayerCharacter 4 visible, Group 2 visible, TextWidths small.
                            ObjectsToHide(true, true, true);
                            break;
                        }
                        default:
                            ObjectsToHide(false, false, true);
                            break;
                    }
                    break;
                }
                case EGameConnectionModi.LanGame:
                case EGameConnectionModi.NetGame:
                {
                    switch (_ePlayerAmount)
                    {
                        case EPlayerAmount.One:
                        {
                            //Only PlayerCharacter 1 visible for Lan 1 vs 1 Matches.
                            ObjectsToHide(false, false, false);
                            break;
                        }
                        case EPlayerAmount.Two:
                        {
                            //PlayerCharacter 3 invisible, PlayerCharacter 4 invisible, Group 2 visible, TextWidths large.
                            ObjectsToHide(false, false, true);
                            break;
                        }
                        //No 'EPlayerAmount.4', since the max playerAmount shall be 2 x 2 = 4.
                        default:
                            ObjectsToHide(false, false, false);
                            break;
                    }
                    break;
                }
            }
        }

        private void SetupMatchDropdowns()
        {
            //PlayerAmount
            m_maxPlayerAmount = new();

            for (uint i = m_MINPLAYER; i < m_matchValues.MaxPlayerInGame + m_MINPLAYER; i++)
            {
                if (i == 1)
                    m_maxPlayerAmount.Add("1");
                if (i % 2 == 0)
                    m_maxPlayerAmount.Add($"{i}");
            }

            m_playerAmountDd.ClearOptions();
            m_playerAmountDd.AddOptions(m_maxPlayerAmount);

            if (m_matchUIStates != null)
            {
                //Modifier to ensure that values EPlayerAmount.Two & EPlayerAmount:Four set the correct dropdownIndex.
                int dropdownValueModifier = (int)m_matchUIStates.EPlayerAmount / 2 /*- 1*/;
                m_playerAmountDd.value = dropdownValueModifier;
            }
            else
            {
                ResetDefault();
            }

            m_playerAmountDd.RefreshShownValue();

            OnPlayerAmountChanged(m_playerAmountDd);
        }
        #endregion

        private void ObjectsToHide(bool _IFThree, bool _IFFour, bool _playerGroupTwo)
        {
            //Old adjustment values: 110.0f & 437.0f.
            m_playerThreeIFGroup.gameObject.SetActive(_IFThree);
            m_playerFourIFGroup.gameObject.SetActive(_IFFour);
            m_playerTwoGroup.gameObject.SetActive(_playerGroupTwo);
        }

        #region OnValueChanged
        private void OnPlayerAmountChanged(TMP_Dropdown _dropdown)
        {
            switch (_dropdown.value)
            {
                case 0:
                {
                    //TODO: Change into 'EPlayerAmount.One', if implementing AI/NPC.
                    m_matchUIStates.EPlayerAmount = EPlayerAmount.One;
                    m_graphicUiStates.SetCameraMode = ECameraModi.SingleCam;
                    ObjectsToHide(false, false, false);
                    break;
                }
                case 1:
                {
                    m_matchUIStates.EPlayerAmount = EPlayerAmount.Two;
                    m_graphicUiStates.SetCameraMode = ECameraModi.TwoHorizontal;
                    ObjectsToHide(false, false, true);
                    break;
                }
                case 2:
                {
                    m_matchUIStates.EPlayerAmount = EPlayerAmount.Four;
                    m_graphicUiStates.SetCameraMode = ECameraModi.FourSplit;
                    ObjectsToHide(true, true, true);
                    break;
                }
                default:
                    break;
            }

            SaveSettingsStates();

            //Required, so MatchSettings can set the Backline-Dropdown in the Settings-Menu visible/invisible.
            SetUpPlayerAmount(m_matchUIStates.EPlayerAmount);

            m_currentPlayers = (int)m_matchUIStates.EPlayerAmount;
            AToggleButtonAccess?.Invoke(m_currentPlayers);
        }

        #region Name-IF-Toggles
        private void InputFieldOneToggleChanges(bool _toggle)
        {
            int inputFieldIndex = Array.IndexOf(m_inputFieldToggles, m_inputFieldToggles[0]);
            if (m_playerSOData[inputFieldIndex] != null)
                m_playerSOData[inputFieldIndex].KeepNameOnLoad = _toggle;

            SavePlayerData(inputFieldIndex);
        }

        private void InputFieldTwoToggleChanges(bool _toggle)
        {
            int inputFieldIndex = Array.IndexOf(m_inputFieldToggles, m_inputFieldToggles[1]);
            if (m_playerSOData[inputFieldIndex] != null)
                m_playerSOData[inputFieldIndex].KeepNameOnLoad = _toggle;

            SavePlayerData(inputFieldIndex);
        }

        private void InputFieldThreeToggleChanges(bool _toggle)
        {
            int inputFieldIndex = Array.IndexOf(m_inputFieldToggles, m_inputFieldToggles[2]);
            if (m_playerSOData[inputFieldIndex] != null)
                m_playerSOData[inputFieldIndex].KeepNameOnLoad = _toggle;

            SavePlayerData(inputFieldIndex);
        }

        private void InputFieldFourToggleChanges(bool _toggle)
        {
            int inputFieldIndex = Array.IndexOf(m_inputFieldToggles, m_inputFieldToggles[3]);
            if (m_playerSOData[inputFieldIndex] != null)
                m_playerSOData[inputFieldIndex].KeepNameOnLoad = _toggle;

            SavePlayerData(inputFieldIndex);
        }
        #endregion

        #region Device-Toggles
        private void DeviceToggleOneChanges(bool _toggle)
        {
            int deviceToggleIndex = Array.IndexOf(m_deviceToggles, m_deviceToggles[0]);
            if (m_playerSOData[deviceToggleIndex] != null)
                m_playerSOData[deviceToggleIndex].DefaultKeyboard = _toggle;

            SavePlayerData(deviceToggleIndex);
        }

        private void DeviceToggleTwoChanges(bool _toggle)
        {
            int deviceToggleIndex = Array.IndexOf(m_deviceToggles, m_deviceToggles[1]);
            if (m_playerSOData[deviceToggleIndex] != null)
                m_playerSOData[deviceToggleIndex].DefaultKeyboard = _toggle;

            SavePlayerData(deviceToggleIndex);
        }

        private void DeviceToggleThreeChanges(bool _toggle)
        {
            int deviceToggleIndex = Array.IndexOf(m_deviceToggles, m_deviceToggles[2]);
            if (m_playerSOData[deviceToggleIndex] != null)
                m_playerSOData[deviceToggleIndex].DefaultKeyboard = _toggle;

            SavePlayerData(deviceToggleIndex);
        }

        private void DeviceToggleFourChanges(bool _toggle)
        {
            int deviceToggleIndex = Array.IndexOf(m_deviceToggles, m_deviceToggles[3]);
            if (m_playerSOData[deviceToggleIndex] != null)
                m_playerSOData[deviceToggleIndex].DefaultKeyboard = _toggle;

            SavePlayerData(deviceToggleIndex);
        }
        #endregion
        #endregion

        private void SetUpPlayerAmount(EPlayerAmount _ePlayerAmount)
        {
            m_matchValues.PlayerSOData.Clear();
            m_matchValues.PlayerSOData = new();

            int playerAmount = (int)_ePlayerAmount;    //EPlayerAmount.Four => int 4 || EPlayerAmount.Two => int 2
            for (int i = 0; i < playerAmount; i++)
            {
                if (m_playerSOData.Length > 0 && m_playerSOData[i] != null)
                {
                    m_matchValues.PlayerSOData.Add(m_playerSOData[i]);
                    m_nameInputFields[i].text = m_playerSOData[i].PlayerName;
                }
            }
        }

        #region Name-Inputfields
        public void PlayerOneInput()  //TODO: Optional Random a playerName, or set PlayerCharacter 1-4.
        {
            int inputFieldIndex = Array.IndexOf(m_nameInputFields, m_nameInputFields[0]);

            UpdateUIAndScriptable(inputFieldIndex);
            SavePlayerData(inputFieldIndex);
        }

        public void PlayerTwoInput()
        {
            int inputFieldIndex = Array.IndexOf(m_nameInputFields, m_nameInputFields[1]);

            UpdateUIAndScriptable(inputFieldIndex);
            SavePlayerData(inputFieldIndex);
        }

        public void PlayerThreeInput()
        {
            int inputFieldIndex = Array.IndexOf(m_nameInputFields, m_nameInputFields[2]);

            UpdateUIAndScriptable(inputFieldIndex);
            SavePlayerData(inputFieldIndex);
        }

        public void PlayerFourInput()
        {
            int inputFieldIndex = Array.IndexOf(m_nameInputFields, m_nameInputFields[3]);

            UpdateUIAndScriptable(inputFieldIndex);
            SavePlayerData(inputFieldIndex);
        }

        private void UpdateUIAndScriptable(int _index)
        {
            m_matchValues.PlayerSOData[_index].PlayerId = _index;

            if (string.IsNullOrWhiteSpace(m_nameInputFields[_index].text))
            {
                AToggleButtonAccess?.Invoke(m_currentPlayers);
                return;
            }

            m_matchValues.PlayerSOData[_index].PlayerName = m_nameInputFields[_index].text;

            AToggleButtonAccess?.Invoke(m_currentPlayers);
        }

        private void ToggleButtonAccess(int _ePlayerAmount)
        {
            for (int i = 0; i < _ePlayerAmount; i++)
            {
                bool isEmptyText = string.IsNullOrWhiteSpace(m_nameInputFields[i].text);
                switch (isEmptyText)
                {
                    case true:
                    {
                        //TODO: Set a PopUp here.                    
                        m_startButton.interactable = false; //StartButton
                        m_joinButton.interactable = false;  //JoinButton
                        if (m_matchValues.PlayerSOData.Count > 0)
                            m_matchValues.PlayerSOData[i].PlayerName = "";
                        //'return' replaces break, so other filled textFields won't enable these buttons again.
                        return;
                    }
                    case false:
                    {
                        m_startButton.interactable = true;  //StartButton
                        m_joinButton.interactable = true;   //JoinButton
                        break;
                    }
                }
            }
        }
        #endregion

        //Scriptable Objects CAN be used like structs to save data. BUT not, if they got foreign/extra references, like gameObject-Prefabs or Sprites. Need to save their names as string instead.
        private void SavePlayerData(int _index)
        {
            if (m_playerSOData[_index] == null)
                return;

            var playerSO = m_playerSOData[_index];
            var prefabName = playerSO.Prefab.name;
            var avatarName = playerSO.Avatar.name;
            var toggleID = m_inputFieldToggles[_index].GetInstanceID();
                        
            PlayerData playerData = new(prefabName, playerSO.PlayerName, playerSO.PlayerId, avatarName, playerSO.KeepNameOnLoad, playerSO.PlayerOnFrontline, playerSO.DefaultKeyboard, toggleID);
            m_persistentData.SaveData(m_playerDataFolderPath, m_playerDataSubPath + $"{_index}", m_fileFormat, playerData, m_encryptionEnabled, true);
        }

        /// <summary>
        /// WHENEVER YOU GOT THE SAME CLASS IN MULTIPLE SCENES (like MENUMANAGER) SAVE CHANGED DATA!!! OR old RELOADED DATA WILL OVERWRITE IT!!! AND YOU DON'T KNOW WHY...!
        /// </summary>
        private void SaveSettingsStates()
        {
            m_persistentData.SaveData(m_settingsStatesFolderPath, m_matchFileName, m_fileFormat, m_matchUIStates, m_encryptionEnabled, true);
            m_persistentData.SaveData(m_settingsStatesFolderPath, m_graphicFileName, m_fileFormat, m_graphicUiStates, m_encryptionEnabled, true);
        }

        private void ResetDefault()
        {
            //Modifier to ensure that values EPlayerAmount.Two & EPlayerAmount:Four set the correct dropdownIndex.
            int dropdownValueModifier = (int)m_registeredPlayers / 2 /*- 1*/;
            m_playerAmountDd.value = dropdownValueModifier;
        }
    }
}