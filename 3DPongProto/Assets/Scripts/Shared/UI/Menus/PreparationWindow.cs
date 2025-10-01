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
        [Header("InputData and Devices")]
        //TODO: Add RoomName for Online Games. Lan games also, depending on setup and experience.
        [SerializeField] private TMP_InputField[] m_nameInputFields;
        [SerializeField] private Toggle[] m_keepNameToggles;
        [SerializeField] private Toggle[] m_deviceToggles;

        [Header("Navigation Logic")]
        [SerializeField] private Selectable[] m_p1Selectables;              //NameInputFields, KeepNameToggles, DeviceToggles for P1
        [SerializeField] private Selectable[] m_p2Selectables;              //NameInputFields, KeepNameToggles, DeviceToggles for P2
        [SerializeField] private Selectable[] m_p3Selectables;              //NameInputFields, KeepNameToggles, DeviceToggles for P3
        [SerializeField] private Selectable[] m_p4Selectables;              //NameInputFields, KeepNameToggles, DeviceToggles for P4
        [SerializeField] private Selectable[] m_bottomLeftSelectables;      //PlayerDropdown, Start- & JoinButton.
        [SerializeField] private Selectable[] m_bottomRightSelectables;     //Settings & BackButton.

        [Header("Group Visibility")]
        [SerializeField] private Transform m_p2UIGroup;
        [SerializeField] private Transform m_addPlayerSlot;
        //[Space]
        [SerializeField] private TMP_Dropdown m_playerAmountDd;

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

            UpdateUINavigation(m_matchData.PlayerCount);
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
        #region UI-Navigation
        /// <summary>
        /// Adapt Navigation dynamic to the _playerCount.
        /// </summary>
        private void UpdateUINavigation(int _playerCount)
        {
            switch (_playerCount)
            {
                case 1:
                {
                    //P1 to Start-Button
                    foreach (Selectable selectable in m_p1Selectables)
                        SetNavigationDown(selectable, m_bottomLeftSelectables[1]); //StartButton

                    foreach (var selectable in m_bottomLeftSelectables)
                        SetNavigationUp(selectable, m_p1Selectables[0]);
                    foreach (var selectable in m_bottomRightSelectables)
                        SetNavigationUp(selectable, m_p1Selectables[0]);
                    break;
                }
                case 2:
                {
                    //P1 & P2 navigieren to Start-Button
                    foreach (Selectable selectable in m_p1Selectables)
                        SetNavigationDown(selectable, m_bottomLeftSelectables[1]);  //StartButton
                    foreach (Selectable selectable in m_p2Selectables)
                        SetNavigationDown(selectable, m_bottomRightSelectables[1]); //BackButton

                    foreach (Selectable selectable in m_bottomLeftSelectables)
                        SetNavigationUp(selectable, m_p1Selectables[0]);
                    foreach (Selectable selectable in m_bottomRightSelectables)
                        SetNavigationUp(selectable, m_p2Selectables[0]);
                    break;
                }
                case 4:
                {
                    //Connect P1 <-> P2 and P3 <-> P4 vertical. (Already connected.)
                    //LinkTwoColumns(m_p1Selectables, m_p2Selectables);
                    //LinkTwoColumns(m_p3Selectables, m_p4Selectables);

                    for (int i1 = 0; i1 < m_p1Selectables.Length; i1++)
                        SetNavigationDown(m_p1Selectables[i1], m_p3Selectables[i1]);
                    for (int i2 = 0; i2 < m_p2Selectables.Length; i2++)
                        SetNavigationDown(m_p2Selectables[i2], m_p4Selectables[i2]);

                    foreach (Selectable selectable in m_bottomLeftSelectables)
                        SetNavigationUp(selectable, m_p3Selectables[0]);
                    foreach (Selectable selectable in m_bottomRightSelectables)
                        SetNavigationUp(selectable, m_p4Selectables[0]);
                    break;
                }
            }
        }

        private void SetNavigationDown(Selectable from, Selectable to)
        {
            //Set the 'selectOnDown'-Navigation for each Element.
            Navigation nav = from.navigation;
            nav.selectOnDown = to;
            from.navigation = nav;

            #region Invsersed Navigation
            //Navigation navTo = to.navigation;
            //navTo.selectOnUp = from;
            //to.navigation = navTo;
            #endregion
        }

        private void SetNavigationUp(Selectable from, Selectable to)
        {
            var nav = from.navigation;
            nav.selectOnUp = to;
            from.navigation = nav;
        }

        /// <summary>
        /// Connect the vertical Columns of the UI-Elements.
        /// </summary>
        private void LinkTwoColumns(Selectable[] topColumn, Selectable[] bottomColumn)
        {
            //Both columns require the identical amount of elements.
            for (int i = 0; i < topColumn.Length; i++)
            {
                Selectable topElement = topColumn[i];
                Selectable bottomElement = bottomColumn[i];

                //Top -> Down -> Bottom
                Navigation navTop = topElement.navigation;
                navTop.selectOnDown = bottomElement;
                topElement.navigation = navTop;

                //Bottom -> Up -> Top
                Navigation navBottom = bottomElement.navigation;
                navBottom.selectOnUp = topElement;
                bottomElement.navigation = navBottom;
            }
        }
        #endregion

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
            graphicSettings.splitDdValue = _dropdownIndex == 0 ? (int)ECameraModi.SingleCam : (_dropdownIndex == 1 ? (int)ECameraModi.Horizontal : (int)ECameraModi.Quartet);

            m_matchData.PlayerCount = _dropdownIndex == 2 ? 4 : (_dropdownIndex == 1 ? 2 : 1);
            SettingsManager.Instance.SaveSettings();

            UpdateUINavigation(m_matchData.PlayerCount);
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

            m_bottomLeftSelectables[1].interactable = allNamesValid;    //StartButton
            m_bottomLeftSelectables[2].interactable = allNamesValid;    //JoinButton
        }
        #endregion
    }
}