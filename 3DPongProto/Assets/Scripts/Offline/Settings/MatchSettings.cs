using System.Collections.Generic;
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

        #region Field-Dimension
        [SerializeField] private Toggle m_ratioToggle;
        [SerializeField] private TMP_Dropdown[] m_fieldDropdowns;
        #endregion

        #region SerializeField-Member-Variables
        [SerializeField] private int m_roundAmount/* = 5*/;
        [SerializeField] private bool m_infiniteRounds;

        [SerializeField] private int m_maxPointsAmount/* = 25*/;
        [SerializeField] private bool m_infinitePoints;

        [Header("Field-Dimension")]
        [SerializeField] private int m_maxFieldWidth/* = 50*/;
        [SerializeField] private int m_maxFieldLength/* = 100*/;
        [SerializeField] private int m_fieldDdWidthIndex /*= 0*/;
        [SerializeField] private int m_fieldDdLengthIndex/* = 0*/;
        [SerializeField] private bool m_fixAspectRatio = false;
        #endregion

        #region Scriptable-References
        [SerializeField] private MatchVariables m_matchVariables;
        [SerializeField] private PlayerData[] m_playerData;
        #endregion

        #region Non-SerializeField-Member-Variables
        private readonly int m_infiniteValue = int.MaxValue;
        private int m_tempPointValue = 0, m_tempRoundValue = 0;

        private int m_lastWidthValue = 0, m_lastLengthValue = 0;
        //private int m_tempWidthDelta = 0;
        private bool m_fixRatioIfTrue;

        private bool m_editablePlayerIF = false, m_editablePlayerTwoIF = false;
        #endregion

        #region Lists and Dictionaries
        private List<string> m_roundsList;
        private List<string> m_maxPointsList;
        private List<string> m_widthList;
        private List<string> m_lengthList;
        private Dictionary<Toggle, TMP_Dropdown> m_matchKeyValuePairs = new Dictionary<Toggle, TMP_Dropdown>();
        private Dictionary<Toggle, TMP_Dropdown[]> m_ratioDict = new Dictionary<Toggle, TMP_Dropdown[]>();
        #endregion

        private void Awake()
        {
            m_ratioToggle.isOn = false;

            //Setup Dictionary to connect the toggles with Round and MaxPoints-Dropdowns.
            for (int i = 0; i < m_unlimitToggleKeys.Count; i++)
            {
                m_matchKeyValuePairs.Add(m_unlimitToggleKeys[i], m_roundValueDropdowns[i]);
            }

            m_ratioDict.Add(m_ratioToggle, m_fieldDropdowns);
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

            FillWidthDropdown();
            FillHeightDropdown();
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
            m_fixRatioIfTrue = m_fixAspectRatio;
            m_matchVariables.FixRatio = m_fixAspectRatio;
#else
            if (m_matchVariables != null)
            {
                m_unlimitToggleKeys[0].isOn = m_matchVariables.InfiniteRounds;
                m_unlimitToggleKeys[1].isOn = m_matchVariables.InfinitePoints;
                m_fixRatioIfTrue = m_matchVariables.FixRatio;
            }
            else
            {
                m_unlimitToggleKeys[0].isOn = m_infiniteRounds;
                m_unlimitToggleKeys[1].isOn = m_infinitePoints;
                m_fixRatioIfTrue = m_fixAspectRatio;
            }
#endif
        }

        private void AddGroupListeners()
        {
            //Listener for changes on 'UnlimitedRounds'-UI-Toggle.
            m_unlimitToggleKeys[0].onValueChanged.AddListener(delegate
            { OnRoundToggleValueChanged(m_unlimitToggleKeys[0]); });
            //Listener for changes on 'UnlimitedMaxPoints'-UI-Toggle.
            m_unlimitToggleKeys[1].onValueChanged.AddListener(delegate
            { OnMaxPointToggleValueChanged(m_unlimitToggleKeys[1]); });

            //Listener for changes on 'UnlimitedRounds'-UI-Dropdown.
            m_roundValueDropdowns[0].onValueChanged.AddListener(delegate
            { OnRoundDropdownValueChanged(m_unlimitToggleKeys[0]); });
            //Listener for changes on 'UnlimitedMaxPoints'-UI-Dropdown.
            m_roundValueDropdowns[1].onValueChanged.AddListener(delegate
            { OnMaxPointDropdownValueChanged(m_unlimitToggleKeys[1]); });

            m_ratioToggle.onValueChanged.AddListener(delegate
            { OnRatioToggleValueChanged(m_ratioToggle); });

            //Field-Width-Dropdown-Listener.
            m_fieldDropdowns[0].onValueChanged.AddListener(delegate
            { OnWidthDropdownValueChanged(m_fieldDropdowns[0]); });
            //Field-Length-Dropdown-Listener.
            m_fieldDropdowns[1].onValueChanged.AddListener(delegate
            { OnLengthDropdownValueChanged(m_fieldDropdowns[1]); });
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

            m_ratioToggle.onValueChanged.RemoveListener(delegate
            { OnRatioToggleValueChanged(m_ratioToggle); });

            m_fieldDropdowns[0].onValueChanged.RemoveListener(delegate
            { OnWidthDropdownValueChanged(m_fieldDropdowns[0]); });
            m_fieldDropdowns[1].onValueChanged.RemoveListener(delegate
            { OnLengthDropdownValueChanged(m_fieldDropdowns[1]); });
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

        private void OnRatioToggleValueChanged(Toggle _toggle)
        {
            TMP_Dropdown[] ratioDropdown = m_ratioDict[_toggle];

            switch (_toggle.isOn)
            {
                case true:
                {
                    m_fixRatioIfTrue = true;
                    ratioDropdown[1].interactable = false;
                    ratioDropdown[1].value = ratioDropdown[0].value * 2;
                    m_matchVariables.FixRatio = _toggle.isOn;
                    break;
                }
                case false:
                {
                    m_fixRatioIfTrue = false;
                    ratioDropdown[1].interactable = true;
                    m_matchVariables.FixRatio = _toggle.isOn;
                    break;
                }
            }
        }

        private void OnWidthDropdownValueChanged(TMP_Dropdown _dropdown)
        {
            if (m_fixRatioIfTrue)
            {
                m_fieldDropdowns[0].onValueChanged.RemoveListener(delegate
                { OnWidthDropdownValueChanged(m_fieldDropdowns[0]); });

                m_fieldDropdowns[1].value = _dropdown.value * 2;
                //m_tempWidthDelta = _dropdown.value - m_lastWidthValue;
                //m_fieldDropdowns[1].value += m_tempWidthDelta * 2;

                m_fieldDropdowns[0].onValueChanged.AddListener(delegate
                { OnWidthDropdownValueChanged(m_fieldDropdowns[0]); });
            }

            m_lastWidthValue = _dropdown.value;
            m_matchVariables.FieldWidthDdIndex = _dropdown.value;
        }

        private void OnLengthDropdownValueChanged(TMP_Dropdown _dropdown)
        {
            m_lastLengthValue = _dropdown.value;
            m_matchVariables.FieldLengthDdIndex = _dropdown.value;
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

        private void FillWidthDropdown()
        {
            m_widthList = new List<string>();

            for (int i = 25; i < m_maxFieldWidth + 1; i++)
            {
                m_widthList.Add(i.ToString());
            }

            m_fieldDropdowns[0].ClearOptions();
            m_fieldDropdowns[0].AddOptions(m_widthList);

            if (m_matchVariables != null)
                m_fieldDropdowns[0].value = m_matchVariables.FieldWidthDdIndex;
            else
                m_fieldDropdowns[0].value = m_fieldDdWidthIndex;

            m_fieldDropdowns[0].RefreshShownValue();
        }

        private void FillHeightDropdown()
        {
            m_lengthList = new List<string>();

            for (int i = 50; i < m_maxFieldLength + 1; i++)
            {
                m_lengthList.Add(i.ToString());
            }

            m_fieldDropdowns[1].ClearOptions();
            m_fieldDropdowns[1].AddOptions(m_lengthList);

            if (m_matchVariables != null)
                m_fieldDropdowns[1].value = m_matchVariables.FieldLengthDdIndex;
            else
                m_fieldDropdowns[1].value = m_fieldDdLengthIndex;

            m_fieldDropdowns[1].RefreshShownValue();
        }
        #endregion
    }
}