using System.Collections.Generic;
using ThreeDeePongProto.Shared.Managers;
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
        #region UI-References
        [Header("UI-References")]
        //TODO: Add RoomName for Online Games. Lan games also, depending on setup and experience.
        [SerializeField] private TMP_InputField[] m_nameInputFields;
        [SerializeField] private Toggle[] m_keepNameToggles;
        [SerializeField] private Toggle[] m_deviceToggles;
        [Space]
        [SerializeField] private Transform m_p2UIGroup;
        [SerializeField] private Transform m_addPlayerSlot;
        [Space]
        [SerializeField] private TMP_Dropdown m_playerAmountDd;
        [SerializeField] private Button m_startButton;
        [SerializeField] private Button m_joinButton;

        private MatchSettingsData m_matchData;
        private List<PlayerProfileData> m_playerProfiles;
        #endregion


        private void OnEnable()
        {
            m_matchData = SettingsManager.Instance.CurrentSettings.Match;
            m_playerProfiles = PlayerProfileManager.Instance.PlayerProfiles;

            SetUIElements();
        }

        private void OnDisable()
        {
            RemoveListeners();
        }

        private void SetUIElements()
        {
            RemoveListeners();

            //Set PlayerAmount-Dropdown
            int playerCountIndex = m_matchData.PlayerCount == 4 ? 2 : (m_matchData.PlayerCount == 2 ? 1 : 0);
            m_playerAmountDd.SetValueWithoutNotify(playerCountIndex);

            //Fill InputField and Toggles for all Player
            for (int i = 0; i < 4; i++)
            {
                var profile = m_playerProfiles[i];
                m_nameInputFields[i].text = profile.PlayerName;
                m_keepNameToggles[i].SetIsOnWithoutNotify(profile.KeepNameOnLoad);
                m_deviceToggles[i].SetIsOnWithoutNotify(profile.DefaultKeyboard);
            }

            UpdateUIVisibility(m_matchData.PlayerCount);
            ButtonValidation();

            AddListeners();
        }

        private void AddListeners()
        {
            m_playerAmountDd.onValueChanged.AddListener(OnPlayerAmountChanged);

            for (int i = 0; i < 4; i++)
            {
                int playerIndex = i; //Replacing multiple similar Listener-Methods with Lambda-Capture.
                m_nameInputFields[i].onEndEdit.AddListener((value) => OnNameChanged(playerIndex, value));
                m_keepNameToggles[i].onValueChanged.AddListener((isOn) => OnKeepNameChanged(playerIndex, isOn));
                m_deviceToggles[i].onValueChanged.AddListener((isKeyboard) => OnDeviceChanged(playerIndex, isKeyboard));
            }
        }

        private void RemoveListeners()
        {
            m_playerAmountDd.onValueChanged.RemoveListener(OnPlayerAmountChanged);

            for (int i = 0; i < 4; i++)
            {
                int playerIndex = i; //Replacing multiple similar Listener-Methods with Lambda-Capture.
                m_nameInputFields[i].onEndEdit.RemoveListener((value) => OnNameChanged(playerIndex, value));
                m_keepNameToggles[i].onValueChanged.RemoveListener((isOn) => OnKeepNameChanged(playerIndex, isOn));
                m_deviceToggles[i].onValueChanged.RemoveListener((isKeyboard) => OnDeviceChanged(playerIndex, isKeyboard));
            }
        }

        #region UI-Setup
        /// <summary>
        /// Sets UI-Elements on/off, depending on the set playerAmount.
        /// </summary>
        private void UpdateUIVisibility(int _playerAmount)
        {
            m_p2UIGroup.gameObject.SetActive(_playerAmount >= 2);
            m_addPlayerSlot.gameObject.SetActive(_playerAmount == 4);
        }
        #endregion

        private void OnPlayerAmountChanged(int _dropdownIndex)
        {
            var graphicSettings = SettingsManager.Instance.CurrentSettings.Graphic;
            graphicSettings.CameraMode = _dropdownIndex == 0 ? (int)ECameraModi.SingleCam : (_dropdownIndex == 1 ? (int)ECameraModi.Horizontal : (int)ECameraModi.Quartet);

            m_matchData.PlayerCount = _dropdownIndex == 2 ? 4 : (_dropdownIndex == 1 ? 2 : 1);
            SettingsManager.Instance.SaveSettings();

            UpdateUIVisibility(m_matchData.PlayerCount);
            ButtonValidation();
        }

        private void OnNameChanged(int _playerIndex, string _newName)
        {
            m_playerProfiles[_playerIndex].PlayerName = _newName;
            PlayerProfileManager.Instance.SaveProfiles();
            ButtonValidation();
        }

        private void OnKeepNameChanged(int _playerIndex, bool _isOn)
        {
            m_playerProfiles[_playerIndex].KeepNameOnLoad = _isOn;
            PlayerProfileManager.Instance.SaveProfiles();
        }

        private void OnDeviceChanged(int _playerIndex, bool _isKeyboard)
        {
            m_playerProfiles[_playerIndex].DefaultKeyboard = _isKeyboard;
            PlayerProfileManager.Instance.SaveProfiles();
        }

        #region Button-Validation
        /// <summary>
        /// Checks, if all relevant player got a name set to (de-)activate Start- & Join-Buttons.
        /// </summary>
        private void ButtonValidation()
        {
            bool allNamesValid = true;
            for (int i = 0; i < m_matchData.PlayerCount; i++)
            {
                if (string.IsNullOrWhiteSpace(m_nameInputFields[i].text))
                {
                    allNamesValid = false;
                    break;
                }
            }

            m_startButton.interactable = allNamesValid;
            m_joinButton.interactable = allNamesValid;
        }
        #endregion
    }
}