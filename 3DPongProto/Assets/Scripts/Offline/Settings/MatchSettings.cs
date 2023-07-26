﻿using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ThreeDeePongProto.Offline.Settings
{
    public class MatchSettings : MonoBehaviour
    {
        //TODO: Desired Inputfield-behaviour to select the gameObject without a blinking cursor and to enable editing on pressing Enter.
        #region Player-Names
        [SerializeField] private TMP_InputField m_playerIF;
        [SerializeField] private TMP_InputField m_playerTwoIF;
        #endregion

        #region Toggle-Dropdown-Connection
        [SerializeField] private List<Toggle> m_unlimitToggleKeys = new List<Toggle>();
        [SerializeField] private List<TMP_Dropdown> m_roundValueDropdowns = new List<TMP_Dropdown>();
        #endregion

        #region SerializeField-Member-Variables
        [SerializeField] private int m_roundAmount/* = 5*/;
        [SerializeField] private bool m_infiniteRounds;

        [SerializeField] private int m_maxPointsAmount/* = 25*/;
        [SerializeField] private bool m_infinitePoints;
        #endregion

        #region Scriptable-References
        [SerializeField] private MatchVariables m_matchVariables;
        [SerializeField] private PlayerData[] m_playerData;
        #endregion

        #region Non-SerializeField-Member-Variables
        private readonly int m_infiniteValue = int.MaxValue;
        private int m_tempPointValue = 0, m_tempRoundValue = 0;

        private bool m_editablePlayerIF = false, m_editablePlayerTwoIF = false;
        #endregion

        #region Lists and Dictionaries
        private List<string> m_roundsList;
        private List<string> m_maxPointsList;
        private Dictionary<Toggle, TMP_Dropdown> m_matchKeyValuePairs = new Dictionary<Toggle, TMP_Dropdown>();
        #endregion

        private void Awake()
        {
            //Setup Dictionary to connect the toggles with Round and MaxPoints-Dropdowns.
            for (int i = 0; i < m_unlimitToggleKeys.Count; i++)
            {
                m_matchKeyValuePairs.Add(m_unlimitToggleKeys[i], m_roundValueDropdowns[i]);
            }
        }

        private void OnEnable()
        {
            AddGroupListeners();
        }

        private void OnDisable()
        {
            RemoveGroupListeners();
        }

        private void Start()
        {
            PresetToggles();

            FillRoundDropdown(m_unlimitToggleKeys[0]);
            FillMaxPointsDropdown(m_unlimitToggleKeys[1]);
        }

        #region Name-Inputfields
        public void PlayerOneInput(string _playernameOne)
        {
            m_playerData[0].PlayerName = _playernameOne;
        }

        public void PlayerTwoInput(string _playernameTwo)
        {
            m_playerData[1].PlayerName = _playernameTwo;
        }
        #endregion

        private void PresetToggles()
        {
#if UNITY_EDITOR
            m_unlimitToggleKeys[0].isOn = m_infiniteRounds;
            m_unlimitToggleKeys[1].isOn = m_infinitePoints;
#else
            if (m_matchVariables != null)
            {
                m_unlimitToggleKeys[0].isOn = m_matchVariables.InfiniteRounds;
                m_unlimitToggleKeys[1].isOn = m_matchVariables.InfinitePoints;
            }
            else
            {
                m_unlimitToggleKeys[0].isOn = m_infiniteRounds;
                m_unlimitToggleKeys[1].isOn = m_infinitePoints;
            }
#endif
        }

        private void AddGroupListeners()
        {
            m_unlimitToggleKeys[0].onValueChanged.AddListener(delegate
            { OnRoundToggleValueChanged(m_unlimitToggleKeys[0]); });
            m_unlimitToggleKeys[1].onValueChanged.AddListener(delegate
            { OnMaxPointToggleValueChanged(m_unlimitToggleKeys[1]); });

            m_roundValueDropdowns[0].onValueChanged.AddListener(delegate
            { OnRoundDropdownValueChanged(m_unlimitToggleKeys[0]); });
            m_roundValueDropdowns[1].onValueChanged.AddListener(delegate
            { OnMaxPointDropdownValueChanged(m_unlimitToggleKeys[1]); });
        }

        private void RemoveGroupListeners()
        {
            m_unlimitToggleKeys[0].onValueChanged.RemoveListener(delegate
            { OnRoundToggleValueChanged(m_unlimitToggleKeys[0]); });
            m_unlimitToggleKeys[1].onValueChanged.RemoveListener(delegate
            { OnMaxPointToggleValueChanged(m_unlimitToggleKeys[1]); });

            m_roundValueDropdowns[0].onValueChanged.RemoveListener(delegate
            { OnRoundDropdownValueChanged(m_unlimitToggleKeys[0]); });
            m_roundValueDropdowns[1].onValueChanged.RemoveListener(delegate
            { OnMaxPointDropdownValueChanged(m_unlimitToggleKeys[1]); });
        }

        public void OnPlayerIFSelected()
        {
            m_editablePlayerIF = !m_editablePlayerIF;
#if UNITY_EDITOR
            Debug.Log($"IF 1 editable = {m_editablePlayerIF}.");
#endif
        }

        public void OnPlayerIFDeselected()
        {
            m_editablePlayerIF = !m_editablePlayerIF;
#if UNITY_EDITOR
            Debug.Log($"IF 1 editable = {m_editablePlayerIF}.");
#endif
        }

        public void OnPlayerTwoIFSelected()
        {
            m_editablePlayerTwoIF = !m_editablePlayerTwoIF;
#if UNITY_EDITOR
            Debug.Log($"IF 2 editable = {m_editablePlayerTwoIF}.");
#endif
        }

        public void OnPlayerTwoIFDeselected()
        {
            m_editablePlayerTwoIF = !m_editablePlayerTwoIF;
#if UNITY_EDITOR
            Debug.Log($"IF 2 editable = {m_editablePlayerTwoIF}.");
#endif
        }

        #region Toggle-OnValueChanged-Methods
        /// <summary>
        /// Listener-Method to replace the displayed round-information in the related dropdown. Also sets roundAmount and bool-state in the scriptable. 
        /// </summary>
        /// <param name="_toggle"></param>
        private void OnRoundToggleValueChanged(Toggle _toggle)
        {
            m_roundsList = new List<string>();

            TMP_Dropdown dropdown = m_matchKeyValuePairs[_toggle];
            dropdown.interactable = !dropdown.interactable;
            dropdown.ClearOptions();

            switch (_toggle.isOn)
            {
                case false:
                {
                    for (int i = 1; i < m_roundAmount + 1; i++)
                    {
                        m_roundsList.Add(i.ToString());
                    }

                    dropdown.AddOptions(m_roundsList);
                    dropdown.RefreshShownValue();
                    //Reset to the last saved value, instead of the set value in the scriptable object.
                    dropdown.value = m_tempRoundValue;
                    m_matchVariables.LastRoundIndex = m_tempRoundValue;
                    m_matchVariables.InfiniteRounds = _toggle.isOn;

                    break;
                }
                case true:
                {
                    m_roundsList.Add("\u221E");
                    dropdown.AddOptions(m_roundsList);
                    dropdown.RefreshShownValue();
                    m_matchVariables.LastRoundIndex = m_infiniteValue;
                    m_matchVariables.InfiniteRounds = _toggle.isOn;
                    break;
                }
            }
        }

        /// <summary>
        /// Listener-Method to replace the displayed point-information in the related dropdown. Also sets pointAmount and bool-state in the scriptable.
        /// </summary>
        /// <param name="_toggle"></param>
        private void OnMaxPointToggleValueChanged(Toggle _toggle)
        {
            m_maxPointsList = new List<string>();

            TMP_Dropdown dropdown = m_matchKeyValuePairs[_toggle];
            dropdown.interactable = !dropdown.interactable;
            dropdown.ClearOptions();

            switch (_toggle.isOn)
            {
                case false:
                {
                    for (int i = 1; i < m_maxPointsAmount + 1; i++)
                    {
                        //'m_maxPointsDropdown.options.Add (new Dropdown.OptionData() { text = variable });' in foreach-loops.
                        m_maxPointsList.Add(i.ToString());
                    }

                    dropdown.AddOptions(m_maxPointsList);
                    dropdown.RefreshShownValue();
                    //Reset to the last saved value, instead of the set value in the scriptable object.
                    dropdown.value = m_tempPointValue;
                    m_matchVariables.LastMaxPointIndex = m_tempPointValue;
                    m_matchVariables.InfinitePoints = _toggle.isOn;

                    break;
                }
                case true:
                {
                    m_maxPointsList.Add("\u221E");
                    dropdown.AddOptions(m_maxPointsList);
                    dropdown.RefreshShownValue();
                    m_matchVariables.LastMaxPointIndex = m_infiniteValue;
                    m_matchVariables.InfinitePoints = _toggle.isOn;
                    break;
                }
            }
        }
        #endregion

        #region Dropdown-OnValueChanged-Methods
        /// <summary>
        /// Listener-Method to set round-values, only while the corresponding dropdown is interactable.
        /// </summary>
        /// <param name="_toggle"></param>
        private void OnRoundDropdownValueChanged(Toggle _toggle)
        {
            TMP_Dropdown dropdown = m_matchKeyValuePairs[_toggle];

            switch (dropdown.interactable)
            {
                case false:
                {
                    //Nothing shall happen.
                    break;
                }
                case true:
                {
                    m_matchVariables.LastRoundIndex = dropdown.value;
                    m_tempRoundValue = dropdown.value;
                    break;
                }
            }
        }

        /// <summary>
        /// Listener-Method to set maxPoint-values, only while the corresponding dropdown is interactable.
        /// </summary>
        /// <param name="_toggle"></param>
        private void OnMaxPointDropdownValueChanged(Toggle _toggle)
        {
            TMP_Dropdown dropdown = m_matchKeyValuePairs[_toggle];

            switch (dropdown.interactable)
            {
                case false:
                {
                    break;
                }
                case true:
                {
                    m_matchVariables.LastMaxPointIndex = dropdown.value;
                    m_tempPointValue = dropdown.value;
                    break;
                }
            }
        }
        #endregion

        #region Fill-Dropdowns-On-Start
        /// <summary>
        /// Method to fill the round-dropdown on Start. Also sets it's interactable-status.
        /// </summary>
        /// <param name="_toggle"></param>
        private void FillRoundDropdown(Toggle _toggle)
        {
            TMP_Dropdown dropdown = m_matchKeyValuePairs[_toggle];
            dropdown.ClearOptions();

            m_roundsList = new List<string>();

            switch (_toggle.isOn)
            {
                case false:
                {
                    for (int i = 1; i < m_roundAmount + 1; i++)
                    {
                        m_roundsList.Add(i.ToString());
                    }

                    dropdown.AddOptions(m_roundsList);

                    if (m_matchVariables != null)
                        dropdown.value = m_matchVariables.LastRoundIndex;
                    else
                        dropdown.value = m_roundAmount;
                    m_tempRoundValue = dropdown.value;

                    dropdown.RefreshShownValue();
                    dropdown.interactable = true;

                    break;
                }
                case true:
                {
                    m_roundsList.Add("\u221E");
                    dropdown.AddOptions(m_roundsList);
                    dropdown.RefreshShownValue();
                    dropdown.interactable = false;

                    break;
                }
            }
        }

        /// <summary>
        /// Method to fill the maxPoint-dropdown on Start. Also sets it's interactable-status.
        /// </summary>
        /// <param name="_toggle"></param>
        private void FillMaxPointsDropdown(Toggle _toggle)
        {
            TMP_Dropdown dropdown = m_matchKeyValuePairs[_toggle];
            dropdown.ClearOptions();

            m_maxPointsList = new List<string>();

            switch (_toggle.isOn)
            {
                case false:
                {
                    for (int i = 1; i < m_maxPointsAmount + 1; i++)
                    {
                        //'m_maxPointsDropdown.options.Add (new Dropdown.OptionData() { text = variable });' in foreach-loops.
                        m_maxPointsList.Add(i.ToString());
                    }

                    dropdown.AddOptions(m_maxPointsList);

                    if (m_matchVariables != null)
                        dropdown.value = m_matchVariables.LastMaxPointIndex;
                    else
                        dropdown.value = m_maxPointsAmount;
                    m_tempPointValue = dropdown.value;

                    dropdown.RefreshShownValue();
                    dropdown.interactable = true;

                    break;
                }
                case true:
                {
                    m_maxPointsList.Add("\u221E");
                    dropdown.AddOptions(m_maxPointsList);
                    dropdown.RefreshShownValue();
                    dropdown.interactable = false;

                    break;
                }
            }
        }
        #endregion
    }
}