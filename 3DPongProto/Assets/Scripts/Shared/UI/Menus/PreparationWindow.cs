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

        [Header("Inputfield-Group")]
        [SerializeField] private TMP_InputField[] m_nameInputFields;
        [Space]
        [SerializeField] private Toggle[] m_inputFieldToggles;
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
        private readonly string m_playerDatasubPath = "/Player";
        private readonly string m_graphicFileName = "/Graphic";
        private readonly string m_matchFileName = "/Match";
        private readonly string m_fileFormat = ".json";

        private IPersistentData m_persistentData = new SerializingData();
        private bool m_encryptionEnabled = false;
        #endregion

        private static event Action<int> aToggleButtonAccess;
        PlayerData m_playerData;

        private void Awake()
        {
            for (int i = 0; i < m_playerSOData.Length; i++)
                ReSetPreparationUI(i);
        }

        private void OnEnable()
        {
            aToggleButtonAccess += ToggleButtonAccess;
            AddGroupListener();
        }

        private void OnDisable()
        {
            aToggleButtonAccess -= ToggleButtonAccess;
            RemoveGroupListener();
        }

        private void Start()
        {
            var currentPlayers = m_matchUIStates.EPlayerAmount;
            SetupWindow(m_matchUIStates.EGameConnectModi, currentPlayers);

            SetupMatchDropdowns();

            m_currentPlayers = (int)currentPlayers;
            ToggleButtonAccess(m_currentPlayers);
        }

        private void AddGroupListener()
        {
            //PlayerAmount
            m_playerAmountDd.onValueChanged.AddListener(delegate
            { OnPlayerAmountChanged(m_playerAmountDd); });
            m_inputFieldToggles[0].onValueChanged.AddListener(HandleToggleOneChanges);
            m_inputFieldToggles[1].onValueChanged.AddListener(HandleToggleTwoChanges);
            m_inputFieldToggles[2].onValueChanged.AddListener(HandleToggleThreeChanges);
            m_inputFieldToggles[3].onValueChanged.AddListener(HandleToggleFourChanges);
        }

        private void RemoveGroupListener()
        {
            //PlayerAmount
            m_playerAmountDd.onValueChanged.RemoveListener(delegate
            { OnPlayerAmountChanged(m_playerAmountDd); });
            m_inputFieldToggles[0].onValueChanged.RemoveListener(HandleToggleOneChanges);
            m_inputFieldToggles[1].onValueChanged.RemoveListener(HandleToggleTwoChanges);
            m_inputFieldToggles[2].onValueChanged.RemoveListener(HandleToggleThreeChanges);
            m_inputFieldToggles[3].onValueChanged.RemoveListener(HandleToggleFourChanges);
        }

        #region Start_Setup
        private void ReSetPreparationUI(int _playerIndex)
        {
            m_playerData = m_persistentData.LoadData<PlayerData>(m_playerDataFolderPath, m_playerDatasubPath + $"{_playerIndex}", m_fileFormat, m_encryptionEnabled);

            switch (m_playerData.KeepNameOnLoad)
            {
                case true:
                {
                    m_playerSOData[_playerIndex].PlayerName = m_playerData.PlayerName;
                    m_playerSOData[_playerIndex].KeepNameOnLoad = m_playerData.KeepNameOnLoad;
                    m_playerSOData[_playerIndex].PlayerOnFrontline = m_playerData.PlayerOnFrontline;
                    m_playerSOData[_playerIndex].DefaultKeyboard = m_playerData.DefaultKeyboard;
                    break;
                }
                case false:
                {
                    m_playerSOData[_playerIndex].PlayerName = "";
                    m_playerSOData[_playerIndex].KeepNameOnLoad = false;
                    m_playerSOData[_playerIndex].PlayerOnFrontline = false;
                    m_playerSOData[_playerIndex].DefaultKeyboard = false;
                    break;
                }
            }

            m_nameInputFields[_playerIndex].text = m_playerSOData[_playerIndex].PlayerName;
            m_inputFieldToggles[_playerIndex].isOn = m_playerSOData[_playerIndex].KeepNameOnLoad;
        }

        private void SetupWindow(EGameModi _connectMode, EPlayerAmount _ePlayerAmount)
        {
            switch (_connectMode)
            {
                case EGameModi.LocalPC:
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
                case EGameModi.LAN:
                case EGameModi.Internet:
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
            aToggleButtonAccess?.Invoke(m_currentPlayers);
        }

        private void HandleToggleOneChanges(bool _toggle)
        {
            int inputFieldIndex = Array.IndexOf(m_inputFieldToggles, m_inputFieldToggles[0]);
            m_playerSOData[inputFieldIndex].KeepNameOnLoad = _toggle;
            SavePlayerData(inputFieldIndex);
        }

        private void HandleToggleTwoChanges(bool _toggle)
        {
            int inputFieldIndex = Array.IndexOf(m_inputFieldToggles, m_inputFieldToggles[1]);
            m_playerSOData[inputFieldIndex].KeepNameOnLoad = _toggle;
            SavePlayerData(inputFieldIndex);
        }

        private void HandleToggleThreeChanges(bool _toggle)
        {
            int inputFieldIndex = Array.IndexOf(m_inputFieldToggles, m_inputFieldToggles[2]);
            m_playerSOData[inputFieldIndex].KeepNameOnLoad = _toggle;
            SavePlayerData(inputFieldIndex);
        }

        private void HandleToggleFourChanges(bool _toggle)
        {
            int inputFieldIndex = Array.IndexOf(m_inputFieldToggles, m_inputFieldToggles[3]);
            m_playerSOData[inputFieldIndex].KeepNameOnLoad = _toggle;
            SavePlayerData(inputFieldIndex);
        }
        #endregion

        private void SetUpPlayerAmount(EPlayerAmount _ePlayerAmount)
        {
            m_matchValues.PlayerSOData.Clear();
            m_matchValues.PlayerSOData = new();

            int playerAmount = (int)_ePlayerAmount;    //EPlayerAmount.Four => int 4 || EPlayerAmount.Two => int 2
            for (int i = 0; i < playerAmount; i++)
            {
                m_matchValues.PlayerSOData.Add(m_playerSOData[i]);
                m_nameInputFields[i].text = m_playerSOData[i].PlayerName;
            }
        }

        #region Name-Inputfields
        public void PlayerOneInput()  //TODO: Optional Random a playername, or set PlayerCharacter 1-4.
        {
            int inputFieldIndex = Array.IndexOf(m_nameInputFields, m_nameInputFields[0]);

            UpdateUIAndScriptables(inputFieldIndex);
            SavePlayerData(inputFieldIndex);
        }

        public void PlayerTwoInput()
        {
            int inputFieldIndex = Array.IndexOf(m_nameInputFields, m_nameInputFields[1]);

            UpdateUIAndScriptables(inputFieldIndex);
            SavePlayerData(inputFieldIndex);
        }

        public void PlayerThreeInput()
        {
            int inputFieldIndex = Array.IndexOf(m_nameInputFields, m_nameInputFields[2]);

            UpdateUIAndScriptables(inputFieldIndex);
            SavePlayerData(inputFieldIndex);
        }

        public void PlayerFourInput()
        {
            int inputFieldIndex = Array.IndexOf(m_nameInputFields, m_nameInputFields[3]);

            UpdateUIAndScriptables(inputFieldIndex);
            SavePlayerData(inputFieldIndex);
        }

        private void UpdateUIAndScriptables(int _index)
        {
            m_matchValues.PlayerSOData[_index].PlayerId = _index;

            if (string.IsNullOrWhiteSpace(m_nameInputFields[_index].text))
            {
                aToggleButtonAccess?.Invoke(m_currentPlayers);
                return;
            }

            m_matchValues.PlayerSOData[_index].PlayerName = m_nameInputFields[_index].text;

            aToggleButtonAccess?.Invoke(m_currentPlayers);
        }

        private void ToggleButtonAccess(int _ePlayerAmount)
        {
            for (int i = 0; i < _ePlayerAmount; i++)
            {
                bool isEmptyText = string.IsNullOrWhiteSpace(m_nameInputFields[i].text);
                //if (m_inputFields[i].text.IsNullOrWhitespace())   //Sirenix.Utilities.
                switch (isEmptyText)
                {
                    case true:
                    {
                        //TODO: Set a PopUp here.                    
                        m_startButton.interactable = false; //StartButton
                        m_joinButton.interactable = false;  //JoinButton
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

        //GameObjects like Prefab or Sprite can't be null, to prevent NullReferenceExceptions.
        private void SavePlayerData(int _index)
        {
            var playerSO = m_playerSOData[_index];
            var prefabName = playerSO.Prefab.name;
            var avatarName = playerSO.Avatar.name;
            var toggleID = m_inputFieldToggles[_index].GetInstanceID();

            PlayerData playerData = new(prefabName, playerSO.PlayerName, playerSO.PlayerId, avatarName, playerSO.KeepNameOnLoad, playerSO.PlayerOnFrontline, playerSO.DefaultKeyboard, toggleID);
            m_persistentData.SaveData(m_playerDataFolderPath, m_playerDatasubPath + $"{_index}", m_fileFormat, playerData, m_encryptionEnabled, true);
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