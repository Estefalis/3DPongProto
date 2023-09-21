using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ThreeDeePongProto.Offline.Settings
{
    public class MatchSettings : MonoBehaviour
    {
        //TODO: Desired Inputfield-behaviour to select the gameObject without a blinking cursor and to enable editing on pressing Enter.
        #region SerializeField-Member-Variables
        #region Player-Names
        [Header("Player-Names")]
        //OnEndEdit in Unity sets the PlayerNames from MatchSettings UI.
        [SerializeField] private TMP_InputField m_playerIF;
        [SerializeField] private TMP_InputField m_playerTwoIF;
        #endregion

        #region Field-Dimension
        [Header("Field-Dimension")]
        [SerializeField] private Toggle m_fixRatioToggle;
        [SerializeField] private TMP_Dropdown[] m_fieldDropdowns;

        [SerializeField] private int m_maxFieldWidth/* = 30*/;
        [SerializeField] private int m_maxFieldLength/* = 60*/;
        [SerializeField] private int m_fieldWidthDdIndex = 0;
        [SerializeField] private int m_fieldLengthDdIndex = 0;
        [SerializeField] private bool m_fixAspectRatio = false;
        #endregion

        #region Line-Ups
        [Header("Line-Up")]
        [SerializeField] private TMP_Dropdown[] m_frontLineDds;
        [SerializeField] private TMP_Dropdown[] m_backLineDds;
        [SerializeField] private TextMeshProUGUI m_frontLineText;
        [SerializeField] private TextMeshProUGUI m_backLineText;
        [SerializeField] private float m_sliderAdjustStep = 0.01f;
        #endregion

        #region Rounds and Points
        [Header("Rounds and Points")]
        [SerializeField] private int m_setMaxRound/* = 5*/;
        [SerializeField] private int m_maxRoundDdIndex = 5;

        [SerializeField] private int m_setMaxPoints/* = 25*/;
        [SerializeField] private int m_maxPointDdIndex = 25;
        [SerializeField] private TMP_Dropdown m_roundsDropdown;
        [SerializeField] private TMP_Dropdown m_maxPointsDropdown;
        #endregion

        #region Scriptable-References
        [SerializeField] private MatchUIStates m_matchUIStates;
        [SerializeField] private MatchValues m_matchValues;
        [SerializeField] private PlayerIDData[] m_playerIDData;
        #endregion
        #endregion
        [Space]
        [Space]

        #region Lists and Dictionaries
        [SerializeField] private List<Button> m_reduceButtonKeys = new List<Button>();
        [SerializeField] private List<Button> m_increaseButtonKeys = new List<Button>();
        [SerializeField] private List<Slider> m_distanceSliderValues = new List<Slider>();

        #region Key-Value-Connection
        private Dictionary<Button, Slider> m_reduceLineSlider = new Dictionary<Button, Slider>();
        private Dictionary<Button, Slider> m_increaseLineSlider = new Dictionary<Button, Slider>();
        #endregion
        #endregion

        #region Non-SerializeField-Member-Variables
        private int m_firstRoundOffset = 1, m_firstPointOffset = 1, m_firstWidthOffset = 25, m_firstLengthOffset = 50;

        //Set in OnValueChanged-Dropdown-Methods.
        private int m_tempWidthDdValue = 0, m_tempLengthDdValue = 0;
        private bool m_fixRatioIfTrue;

        private List<string> m_roundsDdList;
        private List<string> m_maxPointsDdList;
        private List<string> m_widthList;
        private List<string> m_lengthList;
        private List<string> m_playersTeamOne;
        private List<string> m_playersTeamTwo;
        #endregion

        private event Action m_updateLineText;
        private IPersistentData m_persistentData = new SerializingData();
        private bool m_encryptionEnabled = false;

        private void Awake()
        {
            SetupLineDictionaries();
        }

        private void OnEnable()
        {
            if (m_playerIDData.Length > 2)
            {
                FillFrontlineDropdowns();
                FillBacklineDropdowns();
            }

            PresetToggles();

            FillRoundDropdown(m_roundsDropdown);
            FillMaxPointsDropdown(m_maxPointsDropdown);

            FillWidthDropdown();
            FillLengthDropdown();

            SetupLineUpSliders();
            UpdateLineUpTMPs();
            m_updateLineText += UpdateLineUpTMPs;

            AddGroupListeners();
        }

        private void OnDisable()
        {
            RemoveGroupListeners();

            m_persistentData.SaveData("/SaveData/UI-States", "/Match", ".json", m_matchUIStates, m_encryptionEnabled, true);
            m_persistentData.SaveData("/SaveData/UI-Values", "/Match", ".json", m_matchValues, m_encryptionEnabled, true);
        }

        private void PresetToggles()
        {
            m_fixRatioToggle.isOn = false;
#if UNITY_EDITOR
            m_fixRatioIfTrue = m_fixAspectRatio;
            m_matchUIStates.FixRatio = m_fixAspectRatio;
#else
            if (m_uiStates != null)
            {
                m_fixRatioIfTrue = m_uiStates.FixRatio;
            }
            else
            {
                m_fixRatioIfTrue = m_fixAspectRatio;
            }
#endif
        }

        private void AddGroupListeners()
        {
            //Listener for changes on 'UnlimitedRounds'-UI-Dropdown.
            m_roundsDropdown.onValueChanged.AddListener(delegate
            { OnRoundDropdownValueChanged(m_roundsDropdown); });
            //Listener for changes on 'UnlimitedMaxPoints'-UI-Dropdown.
            m_maxPointsDropdown.onValueChanged.AddListener(delegate
            { OnMaxPointDropdownValueChanged(m_maxPointsDropdown); });

            m_fixRatioToggle.onValueChanged.AddListener(delegate
            { OnRatioToggleValueChanged(m_fixRatioToggle); });

            //Field-Width-Dropdown-Listener.
            m_fieldDropdowns[0].onValueChanged.AddListener(delegate
            { OnWidthDropdownValueChanged(m_fieldDropdowns[0]); });
            //Field-Length-Dropdown-Listener.
            m_fieldDropdowns[1].onValueChanged.AddListener(delegate
            { OnLengthDropdownValueChanged(m_fieldDropdowns[1]); });

            //Player-Set-Frontline-Listeners.
            m_frontLineDds[0].onValueChanged.AddListener(delegate
            { OnTeamOneFrontlineDropdownValueChanged(m_frontLineDds[0]); });
            m_frontLineDds[1].onValueChanged.AddListener(delegate
            { OnTeamTwoFrontlineDropdownValueChanged(m_frontLineDds[1]); });

            //Player-Set-Backline-Listeners.
            m_backLineDds[0].onValueChanged.AddListener(delegate
            { OnTeamOneBacklineDropdownValueChanged(m_backLineDds[0]); });
            m_backLineDds[1].onValueChanged.AddListener(delegate
            { OnTeamTwoBacklineDropdownValueChanged(m_backLineDds[1]); });

            m_distanceSliderValues[0].onValueChanged.AddListener(OnFrontlineSliderValueChanged);
            m_distanceSliderValues[1].onValueChanged.AddListener(OnBacklineSliderValueChanged);
        }

        private void RemoveGroupListeners()
        {
            m_roundsDropdown.onValueChanged.RemoveListener(delegate
            { OnRoundDropdownValueChanged(m_roundsDropdown); });
            m_maxPointsDropdown.onValueChanged.RemoveListener(delegate
            { OnMaxPointDropdownValueChanged(m_maxPointsDropdown); });

            m_fixRatioToggle.onValueChanged.RemoveListener(delegate
            { OnRatioToggleValueChanged(m_fixRatioToggle); });

            m_fieldDropdowns[0].onValueChanged.RemoveListener(delegate
            { OnWidthDropdownValueChanged(m_fieldDropdowns[0]); });
            m_fieldDropdowns[1].onValueChanged.RemoveListener(delegate
            { OnLengthDropdownValueChanged(m_fieldDropdowns[1]); });

            m_frontLineDds[0].onValueChanged.RemoveListener(delegate
            { OnTeamOneFrontlineDropdownValueChanged(m_frontLineDds[0]); });
            m_frontLineDds[1].onValueChanged.RemoveListener(delegate
            { OnTeamTwoFrontlineDropdownValueChanged(m_frontLineDds[1]); });

            m_backLineDds[0].onValueChanged.RemoveListener(delegate
            { OnTeamOneBacklineDropdownValueChanged(m_backLineDds[0]); });
            m_backLineDds[1].onValueChanged.RemoveListener(delegate
            { OnTeamTwoBacklineDropdownValueChanged(m_backLineDds[1]); });

            m_distanceSliderValues[0].onValueChanged.RemoveListener(OnFrontlineSliderValueChanged);
            m_distanceSliderValues[1].onValueChanged.RemoveListener(OnBacklineSliderValueChanged);
        }

        #region Toggle-OnValueChanged-Methods
        #region Fix Ratio-Toggle
        /// <summary>
        /// Enabling this Toggle shall fix changes of width and length of the playfield to 1:2 ratio, while changing values on one of the two dropdowns.
        /// </summary>
        /// <param name="_toggle"></param>
        private void OnRatioToggleValueChanged(Toggle _toggle)
        {
switch (_toggle.isOn)
            {
                case true:
                {
                    m_fixRatioIfTrue = true;
                    m_fieldDropdowns[1].value = m_fieldDropdowns[0].value * 2;
                    m_matchUIStates.FixRatio = _toggle.isOn;
                    break;
                }
                case false:
                {
                    m_fixRatioIfTrue = false;
                    m_matchUIStates.FixRatio = _toggle.isOn;
                    break;
                }
            }
        }
        #endregion
        #endregion

        #region Dropdown-OnValueChanged-Methods
        #region Round-Dropdown
        /// <summary>
        /// Listener-Method to set round-values, only while the corresponding dropdown is interactable.
        /// </summary>
        /// <param name="_toggle"></param>
        private void OnRoundDropdownValueChanged(TMP_Dropdown _maxRoundsDropdown)
        {
            //DropdownValue is equal to DropdownIndex +1, without the infinity option at index 0.
            m_matchValues.SetMaxRounds = _maxRoundsDropdown.value /*+ m_firstRoundOffset*/;
            m_matchUIStates.LastRoundDdIndex = _maxRoundsDropdown.value;
            //m_tempRoundDdValue = _maxRoundsDropdown.value;

            //'m_firstRoundOffset' oversteps the Round 0 in the '_maxRoundsDropdown' + current '_maxRoundsDropdown.value' == m_roundsDdList.Count.
            //if (m_firstRoundOffset + _maxRoundsDropdown.value == m_roundsDdList.Count)
            if (_maxRoundsDropdown.value == 0)
                m_matchUIStates.InfiniteRounds = true;
            else
                m_matchUIStates.InfiniteRounds = false;
        }
        #endregion

        #region MaxPoint-Dropdown
        /// <summary>
        /// Listener-Method to set maxPoint-values, only while the corresponding dropdown is interactable.
        /// </summary>
        /// <param name="_toggle"></param>
        private void OnMaxPointDropdownValueChanged(TMP_Dropdown _maxPointsDropdown)
        {
            //DropdownValue is equal to DropdownIndex +1, without the infinity option at index 0.
            m_matchValues.SetMaxPoints = _maxPointsDropdown.value/* + m_firstPointOffset*/;
            m_matchUIStates.LastMaxPointDdIndex = _maxPointsDropdown.value;

            //'m_firstPointOffset' oversteps the Point 0 in the '_maxPointsDropdown' + current '_maxPointsDropdown.value' == m_maxPointsDdList.Count.
            //if (m_firstPointOffset + _maxPointsDropdown.value == m_maxPointsDdList.Count)
            if (_maxPointsDropdown.value == 0)
                m_matchUIStates.InfinitePoints = true;
            else
                m_matchUIStates.InfinitePoints = false;
        }
        #endregion

        #region FieldDimension (Width & Length)
        private void OnWidthDropdownValueChanged(TMP_Dropdown _dropdown)
        {
            if (m_fixRatioIfTrue)
            {
                m_fieldDropdowns[1].value = _dropdown.value * 2;
            }

            m_tempWidthDdValue = _dropdown.value;
            m_matchValues.SetGroundWidth = _dropdown.value + m_firstWidthOffset;
            m_matchUIStates.LastFieldWidthDdIndex = _dropdown.value;
        }

        private void OnLengthDropdownValueChanged(TMP_Dropdown _dropdown)
        {
            if (m_fixRatioIfTrue)
            {
                m_fieldDropdowns[0].value = (int)(_dropdown.value * 0.5f);
            }

            m_tempLengthDdValue = _dropdown.value;
            m_matchValues.SetGroundLength = _dropdown.value + m_firstLengthOffset;
            m_matchUIStates.LastFieldLengthDdIndex = _dropdown.value;
        }
        #endregion

        /// <summary>
        /// dropdownIndex-Changes set Booleans on playerData-Scriptables to set their goalDistance-Positions on Match-Start.
        /// </summary>
        /// <param name="_dropdown"></param>
        private void OnTeamOneFrontlineDropdownValueChanged(TMP_Dropdown _dropdown)
        {
            switch (_dropdown.value)
            {
                case 0:
                {
                    //Frontline Player 1 (Team 1, ID 0) = Backline Player 3 (Team 1, ID 2).
                    m_backLineDds[0].value = 1;
                    m_matchUIStates.TPOneFrontDdIndex = 0;
                    UpdateFrontlineSetup(m_playerIDData[0].PlayerId);
                    break;
                }
                case 1:
                {
                    //Frontline Player 3 (Team 1, ID 2) = Backline Player 1 (Team 1, ID 0).
                    m_backLineDds[0].value = 0;
                    m_matchUIStates.TPOneFrontDdIndex = 1;
                    UpdateFrontlineSetup(m_playerIDData[2].PlayerId);
                    break;
                }
                default:
                { break; }
            }
        }

        /// <summary>
        /// dropdownIndex-Changes set Booleans on playerData-Scriptables to set their goalDistance-Positions on Match-Start.
        /// </summary>
        /// <param name="_dropdown"></param>
        private void OnTeamTwoFrontlineDropdownValueChanged(TMP_Dropdown _dropdown)
        {
            switch (_dropdown.value)
            {
                case 0:
                {
                    //Frontline Player 2 (Team 2, ID 1) = Backline Player 4 (Team 2, ID 3).
                    m_backLineDds[1].value = 1;
                    m_matchUIStates.TPTwoFrontDdIndex = 0;
                    UpdateFrontlineSetup(m_playerIDData[1].PlayerId);
                    break;
                }
                case 1:
                {
                    //Frontline Player 4 (Team 2, ID 3) = Backline Player 2 (Team 2, ID 1).
                    m_backLineDds[1].value = 0;
                    m_matchUIStates.TPTwoFrontDdIndex = 1;
                    UpdateFrontlineSetup(m_playerIDData[3].PlayerId);
                    break;
                }
                default:
                { break; }
            }
        }

        /// <summary>
        /// dropdownIndex-Changes set Booleans on playerData-Scriptables to set their goalDistance-Positions on Match-Start.
        /// </summary>
        /// <param name="_dropdown"></param>
        private void OnTeamOneBacklineDropdownValueChanged(TMP_Dropdown _dropdown)
        {
            switch (_dropdown.value)
            {
                case 0:
                {
                    //Backline Player 1 (Team 1, ID 0) = Frontline Player 3 (Team 1, ID 2).
                    m_frontLineDds[0].value = 1;
                    m_matchUIStates.TPOneBacklineIndex = 0;
                    UpdateBacklineSetup(m_playerIDData[0].PlayerId);
                    break;
                }
                case 1:
                {
                    //Backline Player 3 (Team 1, ID 2) = Frontline Player 1 (Team 1, ID 0).
                    m_frontLineDds[0].value = 0;
                    m_matchUIStates.TPOneBacklineIndex = 1;
                    UpdateBacklineSetup(m_playerIDData[2].PlayerId);
                    break;
                }
                default:
                { break; }
            }
        }

        /// <summary>
        /// dropdownIndex-Changes set Booleans on playerData-Scriptables to set their goalDistance-Positions on Match-Start.
        /// </summary>
        /// <param name="_dropdown"></param>
        private void OnTeamTwoBacklineDropdownValueChanged(TMP_Dropdown _dropdown)
        {
            switch (_dropdown.value)
            {
                case 0:
                {
                    //Backline Player 2 (Team 2, ID 1) = Frontline Player 4 (Team 2, ID 3).
                    m_frontLineDds[1].value = 1;
                    m_matchUIStates.TPTwoBacklineIndex = 0;
                    UpdateBacklineSetup(m_playerIDData[1].PlayerId);
                    break;
                }
                case 1:
                {
                    //Backline Player 4 (Team 2, ID 3) = Frontline Player 2 (Team 2, ID 1).
                    m_frontLineDds[1].value = 0;
                    m_matchUIStates.TPTwoBacklineIndex = 1;
                    UpdateBacklineSetup(m_playerIDData[3].PlayerId);
                    break;
                }
                default:
                { break; }
            }
        }
        #endregion

        #region Slider-OnValueChanged
        private void OnFrontlineSliderValueChanged(float _value)
        {
            m_distanceSliderValues[0].value = _value;
            m_matchValues.FrontlineAdjustment = m_distanceSliderValues[0].minValue + _value;
            m_updateLineText?.Invoke();
        }

        private void OnBacklineSliderValueChanged(float _value)
        {
            m_distanceSliderValues[1].value = _value;
            m_matchValues.BacklineAdjustment = m_distanceSliderValues[1].minValue + _value;
            m_updateLineText?.Invoke();
        }
        #endregion

        #region Fill-Dropdowns-On-Start
        /// <summary>
        /// Method to fill the round-dropdown on Start. Also sets it's interactable-status.
        /// </summary>
        /// <param name="_toggle"></param>
        private void FillRoundDropdown(TMP_Dropdown _maxRoundsDropdown)
        {
            _maxRoundsDropdown.ClearOptions();
            m_roundsDdList = new List<string> { "\u221E" };
            for (int i = m_firstRoundOffset; i < m_setMaxRound + 1; i++)
            {
                m_roundsDdList.Add(i.ToString());
            }
            _maxRoundsDropdown.AddOptions(m_roundsDdList);

            if (m_matchUIStates != null)
            {
                _maxRoundsDropdown.value = m_matchUIStates.LastRoundDdIndex;
            }
            else
            {
                _maxRoundsDropdown.value = m_maxRoundDdIndex;
            }

            _maxRoundsDropdown.RefreshShownValue();
            _maxRoundsDropdown.interactable = true;

            m_matchValues.SetMaxRounds = m_setMaxRound;
            m_matchValues.SetMaxPoints = m_setMaxPoints;
        }

        /// <summary>
        /// Method to fill the maxPoint-dropdown on Start. Also sets it's interactable-status.
        /// </summary>
        /// <param name="_toggle"></param>
        private void FillMaxPointsDropdown(TMP_Dropdown _maxPointsDropdown)
        {
            _maxPointsDropdown.ClearOptions();
            m_maxPointsDdList = new List<string> {"\u221E"};
            for (int i = m_firstPointOffset; i < m_setMaxPoints + 1; i++)
            {
                //'m_maxPointsDropdown.options.Add (new Dropdown.OptionData() { text = variable });' in foreach-loops.
                m_maxPointsDdList.Add(i.ToString());
            }
            _maxPointsDropdown.AddOptions(m_maxPointsDdList);

            if (m_matchUIStates != null)
            {
                _maxPointsDropdown.value = m_matchUIStates.LastMaxPointDdIndex;
            }
            else
            {
                _maxPointsDropdown.value = m_maxPointDdIndex;
            }

            _maxPointsDropdown.RefreshShownValue();
            _maxPointsDropdown.interactable = true;

            m_matchValues.SetMaxPoints = m_setMaxPoints;
        }

        private void FillWidthDropdown()
        {
            m_widthList = new List<string>();

            for (int i = m_firstWidthOffset; i < m_maxFieldWidth + 1; i++)
            {
                m_widthList.Add(i.ToString());
            }

            m_fieldDropdowns[0].ClearOptions();
            m_fieldDropdowns[0].AddOptions(m_widthList);

            if (m_matchUIStates != null)
                m_fieldDropdowns[0].value = m_matchUIStates.LastFieldWidthDdIndex;
            else
                m_fieldDropdowns[0].value = m_fieldWidthDdIndex;

            m_fieldDropdowns[0].RefreshShownValue();
        }

        private void FillLengthDropdown()
        {
            m_lengthList = new List<string>();

            for (int i = m_firstLengthOffset; i < m_maxFieldLength + 1; i++)
            {
                m_lengthList.Add(i.ToString());
            }

            m_fieldDropdowns[1].ClearOptions();
            m_fieldDropdowns[1].AddOptions(m_lengthList);

            if (m_matchUIStates != null)
                m_fieldDropdowns[1].value = m_matchUIStates.LastFieldLengthDdIndex;
            else
                m_fieldDropdowns[1].value = m_fieldLengthDdIndex;

            m_fieldDropdowns[1].RefreshShownValue();
        }

        private void FillFrontlineDropdowns()
        {
            m_playersTeamOne = new List<string>();
            m_playersTeamTwo = new List<string>();

            for (int i = 0; i < m_playerIDData.Length; i++)
            {
                if (m_playerIDData[i].PlayerId % 2 == 0)
                    m_playersTeamOne.Add($"Player {i + 1}");
                if (m_playerIDData[i].PlayerId % 2 != 0)
                    m_playersTeamTwo.Add($"Player {i + 1}");
            }

            m_frontLineDds[0].ClearOptions();
            m_frontLineDds[0].AddOptions(m_playersTeamOne);

            if (m_matchUIStates != null)
                m_frontLineDds[0].value = m_matchUIStates.TPOneFrontDdIndex;

            m_frontLineDds[0].RefreshShownValue();

            m_frontLineDds[1].ClearOptions();
            m_frontLineDds[1].AddOptions(m_playersTeamTwo);

            if (m_matchUIStates != null)
                m_frontLineDds[1].value = m_matchUIStates.TPTwoFrontDdIndex;

            m_frontLineDds[1].RefreshShownValue();
        }

        private void FillBacklineDropdowns()
        {
            m_playersTeamOne = new List<string>();
            m_playersTeamTwo = new List<string>();

            for (int i = 0; i < m_playerIDData.Length; i++)
            {
                if (m_playerIDData[i].PlayerId % 2 == 0)
                    m_playersTeamOne.Add($"Player {i + 1}");
                if (m_playerIDData[i].PlayerId % 2 != 0)
                    m_playersTeamTwo.Add($"Player {i + 1}");
            }

            m_backLineDds[0].ClearOptions();
            m_backLineDds[0].AddOptions(m_playersTeamOne);

            if (m_matchUIStates != null)
                m_backLineDds[0].value = m_matchUIStates.TPOneBacklineIndex;

            m_backLineDds[0].RefreshShownValue();

            m_backLineDds[1].ClearOptions();
            m_backLineDds[1].AddOptions(m_playersTeamTwo);

            if (m_matchUIStates != null)
                m_backLineDds[1].value = m_matchUIStates.TPTwoBacklineIndex;

            m_backLineDds[1].RefreshShownValue();
        }
        #endregion

        #region Non-OnValueChanged-Methods
        private void SetupLineDictionaries()
        {
            for (int i = 0; i < m_reduceButtonKeys.Count; i++)
            {
                m_reduceLineSlider.Add(m_reduceButtonKeys[i], m_distanceSliderValues[i]);
            }

            for (int i = 0; i < m_increaseButtonKeys.Count; i++)
            {
                m_increaseLineSlider.Add(m_increaseButtonKeys[i], m_distanceSliderValues[i]);
            }
        }

        #region Name-Inputfields
        public void PlayerOneInput(string _playernameOne)
        {
            m_playerIDData[0].PlayerName = _playernameOne;
        }

        public void PlayerTwoInput(string _playernameTwo)
        {
            m_playerIDData[1].PlayerName = _playernameTwo;
        }
        #endregion

        private void SetupLineUpSliders()
        {
            //FrontSlider
            m_distanceSliderValues[0].value = m_distanceSliderValues[0].minValue + m_matchValues.FrontlineAdjustment;
            //BackSlider
            m_distanceSliderValues[1].value = m_distanceSliderValues[1].minValue + m_matchValues.BacklineAdjustment;
        }

        private void UpdateLineUpTMPs()
        {
            if (m_matchValues == null)
            {
                m_frontLineText.text = "No Data";
                m_backLineText.text = "No Data";
                return;
            }

            m_frontLineText.SetText($"{m_distanceSliderValues[0].value + m_matchValues.MinFrontLineDistance + m_matchValues.BacklineAdjustment:N2}");
            m_backLineText.text = $"{m_distanceSliderValues[1].value + m_matchValues.MinBackLineDistance:N2}";
        }

        //Front = ID 0, Back = ID 1. Lists and UI are organized in the same scheme.
        public void MoveLinesForward(int _sliderIndex)
        {
            Slider sliderToIncrease = m_increaseLineSlider[m_increaseButtonKeys[_sliderIndex]];
            sliderToIncrease.value += m_sliderAdjustStep;
        }

        //Front = ID 0, Back = ID 1. Lists and UI are organized in the same scheme.
        public void MoveLinesBackward(int _sliderIndex)
        {
            Slider sliderToReduce = m_reduceLineSlider[m_reduceButtonKeys[_sliderIndex]];
            sliderToReduce.value -= m_sliderAdjustStep;
        }

        private void UpdateFrontlineSetup(int _playerId)
        {
            m_playerIDData[_playerId].PlayerOnFrontline = true;
        }

        private void UpdateBacklineSetup(int _playerId)
        {
            m_playerIDData[_playerId].PlayerOnFrontline = false;
        }

        public void ReSetDefault()
        {
            m_roundsDropdown.value = m_maxRoundDdIndex;
            m_maxPointsDropdown.value = m_maxPointDdIndex;

            m_fixRatioToggle.isOn = m_fixAspectRatio;
            m_fieldDropdowns[0].value = m_fieldWidthDdIndex;
            m_fieldDropdowns[1].value = m_fieldLengthDdIndex;

            m_distanceSliderValues[0].value = 0;
            m_distanceSliderValues[1].value = 0;
            m_backLineDds[0].value = 0;
            m_backLineDds[1].value = 0;
        }
        #endregion
    }
}