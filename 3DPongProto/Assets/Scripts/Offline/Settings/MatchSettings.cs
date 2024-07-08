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
        [Header("Player-Details")]
        //OnEndEdit in Unity sets the PlayerNames from MatchSettings UI.        
        [SerializeField] private Toggle[] m_rotationReset;
        [SerializeField] private bool m_TpOneRotResetDefault = true;
        [SerializeField] private bool m_TpTwoRotResetDefault = true;
        #endregion

        #region Field-Dimension
        [Header("Field-Dimension")]
        [SerializeField] private Toggle m_fixRatioToggle;
        [SerializeField] private TMP_Dropdown[] m_matchSetupDropdowns;

        [SerializeField] private int m_maxFieldWidth = 30;
        [SerializeField] private int m_maxFieldLength = 60;
        [SerializeField] private int m_fieldWidthDdIndex = 0;
        [SerializeField] private int m_fieldLengthDdIndex = 0;
        [SerializeField] private bool m_fixAspectRatio = false;
        #endregion

        #region Line-Ups
        [Header("Line-Up")]
        [SerializeField] private TMP_Dropdown[] m_frontLineDds;
        [SerializeField] private TMP_Dropdown[] m_backLineDds;
        [SerializeField] private int m_frontLineDefaultValue = 0;
        [SerializeField] private int m_backLineDefaultValue = 0;
        [Space]
        [SerializeField] private TextMeshProUGUI m_backFloatText;
        [SerializeField] private TextMeshProUGUI m_frontFloatText;
        [SerializeField] private float m_distanceSliderDefaults = 0;
        [SerializeField] private float m_sliderAdjustStep = 0.01f;
        #endregion

        #region Rounds and Points
        [Header("Rounds and Points")]
        [SerializeField] private int m_maxRoundDdIndex = 5;
        [SerializeField] private int m_maxPointDdIndex = 25;
        #endregion

        #region Hiding
        [Header("Hiding")]
        [SerializeField] private Transform m_frontParentTransform;
        [SerializeField] private Transform m_backPTwoDdGroup;
        [SerializeField] private TextMeshProUGUI m_backLineUpText;
        #endregion
        #endregion
        [Space]

        #region Lists and Dictionaries
        [SerializeField] private List<Button> m_reduceButtonKeys = new List<Button>();
        [SerializeField] private List<Button> m_increaseButtonKeys = new List<Button>();
        [SerializeField] private List<Slider> m_distanceSliderValues = new List<Slider>();

        private List<string> m_roundsDdList;
        private List<string> m_maxPointsDdList;
        private List<string> m_widthList;
        private List<string> m_lengthList;

        private List<string> m_playersTeamOne;
        private List<string> m_playersTeamTwo;

        #region Key-Value-Connection
        private Dictionary<Button, Slider> m_reduceLineSlider = new Dictionary<Button, Slider>();
        private Dictionary<Button, Slider> m_increaseLineSlider = new Dictionary<Button, Slider>();
        #endregion
        #endregion

        #region Non-SerializeField-Member-Variables
        private readonly int m_firstRoundOffset = 1, m_firstPointOffset = 1, m_firstWidthOffset = 25, m_firstLengthOffset = 50;
        #endregion

        #region Scriptable-References
        [Header("Scriptable Objects")]
        [SerializeField] private BasicFieldValues m_basicFieldValues;
        [SerializeField] private MatchUIStates m_matchUIStates;
        [SerializeField] private MatchValues m_matchValues;
        [SerializeField] private GraphicUIStates m_graphicUiStates;
        #endregion
        
        #region Serialization
        private readonly string m_settingsStatesFolderPath = "/SaveData/Settings-States";
        private readonly string m_fieldSettingsPath = "/SaveData/FieldSettings";
        private readonly string m_matchFileName = "/Match";
        private readonly string m_fileFormat = ".json";

        private IPersistentData m_persistentData = new SerializingData();
        private bool m_encryptionEnabled = false;
        #endregion

        private void Awake()
        {
            //PreparationWindow.PlayerAmountUpdated += UpdateObjectsVisibility;
            SetupLineDictionaries();

            if (m_matchUIStates == null || m_matchValues == null)
                ReSetDefault();
            //else LoadMatchSettings(); moved to 'MenuOrganisation.cs'.
        }

        private void OnEnable()
        {
            //TODO: InitialUISetup and UpdateLineUpTMPs check for nulled Scriptables.
            InitializeUISetup();
            UpdateLineUpTMPs();
            AddGroupListeners();
        }

        private void OnDisable()
        {
            //PreparationWindow.PlayerAmountUpdated -= UpdateObjectsVisibility;
            RemoveGroupListeners();

            m_persistentData.SaveData(m_settingsStatesFolderPath, m_matchFileName, m_fileFormat, m_matchUIStates, m_encryptionEnabled, true);
            m_persistentData.SaveData(m_fieldSettingsPath, m_matchFileName, m_fileFormat, m_basicFieldValues, m_encryptionEnabled, true);
        }
                
        #region UnRegister Listener Region
        private void AddGroupListeners()
        {
            //Rounds
            m_matchSetupDropdowns[0].onValueChanged.AddListener(delegate
            { OnRoundDropdownValueChanged(m_matchSetupDropdowns[0]); });
            //MaxPoints
            m_matchSetupDropdowns[1].onValueChanged.AddListener(delegate
            { OnMaxPointDropdownValueChanged(m_matchSetupDropdowns[1]); });

            m_fixRatioToggle.onValueChanged.AddListener(delegate
            { OnRatioToggleValueChanged(m_fixRatioToggle); });
            //Field-Width
            m_matchSetupDropdowns[2].onValueChanged.AddListener(delegate
            { OnWidthDropdownValueChanged(m_matchSetupDropdowns[2]); });
            //Field-Length
            m_matchSetupDropdowns[3].onValueChanged.AddListener(delegate
            { OnLengthDropdownValueChanged(m_matchSetupDropdowns[3]); });

            if ((int)m_matchUIStates.EPlayerAmount > 3)
            {
                //Player-Set-Frontline
                m_frontLineDds[0].onValueChanged.AddListener(delegate
                { OnTeamOneFrontlineDropdownValueChanged(m_frontLineDds[0]); });
                m_frontLineDds[1].onValueChanged.AddListener(delegate
                { OnTeamTwoFrontlineDropdownValueChanged(m_frontLineDds[1]); });
                m_distanceSliderValues[0].onValueChanged.AddListener(OnFrontlineSliderValueChanged);
            }

            //Player-Set-Backline
            m_backLineDds[0].onValueChanged.AddListener(delegate
            { OnTeamOneBacklineDropdownValueChanged(m_backLineDds[0]); });
            m_backLineDds[1].onValueChanged.AddListener(delegate
            { OnTeamTwoBacklineDropdownValueChanged(m_backLineDds[1]); });
            m_distanceSliderValues[1].onValueChanged.AddListener(OnBacklineSliderValueChanged);

            //Toggles to allow or deny PaddleRotation-Resets on each Goal for TeamPlayerOne [0] and TeamPlayerTwo [1].
            m_rotationReset[0].onValueChanged.AddListener(HandleTpOneToggleValueChanges);
            m_rotationReset[1].onValueChanged.AddListener(HandleTpTwoToggleValueChanges);
        }

        private void RemoveGroupListeners()
        {
            //Rounds
            m_matchSetupDropdowns[0].onValueChanged.RemoveListener(delegate
            { OnRoundDropdownValueChanged(m_matchSetupDropdowns[0]); });
            //MaxPoints
            m_matchSetupDropdowns[1].onValueChanged.RemoveListener(delegate
            { OnMaxPointDropdownValueChanged(m_matchSetupDropdowns[1]); });

            m_fixRatioToggle.onValueChanged.RemoveListener(delegate
            { OnRatioToggleValueChanged(m_fixRatioToggle); });
            //Field-Width
            m_matchSetupDropdowns[2].onValueChanged.RemoveListener(delegate
            { OnWidthDropdownValueChanged(m_matchSetupDropdowns[2]); });
            //Field-Length
            m_matchSetupDropdowns[3].onValueChanged.RemoveListener(delegate
            { OnLengthDropdownValueChanged(m_matchSetupDropdowns[3]); });

            if (m_matchValues.PlayerData.Count > 3)
            {
                //Player-Set-Frontline
                m_frontLineDds[0].onValueChanged.RemoveListener(delegate
                { OnTeamOneFrontlineDropdownValueChanged(m_frontLineDds[0]); });
                m_frontLineDds[1].onValueChanged.RemoveListener(delegate
                { OnTeamTwoFrontlineDropdownValueChanged(m_frontLineDds[1]); });
                m_distanceSliderValues[0].onValueChanged.RemoveListener(OnFrontlineSliderValueChanged);
            }

            //Player-Set-Backline
            m_backLineDds[0].onValueChanged.RemoveListener(delegate
            { OnTeamOneBacklineDropdownValueChanged(m_backLineDds[0]); });
            m_backLineDds[1].onValueChanged.RemoveListener(delegate
            { OnTeamTwoBacklineDropdownValueChanged(m_backLineDds[1]); });
            m_distanceSliderValues[1].onValueChanged.RemoveListener(OnBacklineSliderValueChanged);

            m_rotationReset[0].onValueChanged.RemoveListener(HandleTpOneToggleValueChanges);
            m_rotationReset[1].onValueChanged.RemoveListener(HandleTpTwoToggleValueChanges);
        }
        #endregion

        #region Listener Methods
        #region Toggle-OnValueChanged-Methods
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
                    m_matchSetupDropdowns[3].value = m_matchSetupDropdowns[2].value * 2;
                    m_matchUIStates.FixRatio = _toggle.isOn;
                    break;
                }
                case false:
                {
                    m_matchUIStates.FixRatio = _toggle.isOn;
                    break;
                }
            }
        }

        private void HandleTpOneToggleValueChanges(bool _toggle)
        {
            m_matchUIStates.TpOneRotReset = _toggle;
        }

        private void HandleTpTwoToggleValueChanges(bool _toggle)
        {
            m_matchUIStates.TpTwoRotReset = _toggle;
        }
        #endregion

        #region Dropdown-OnValueChanged-Methods
        /// <summary>
        /// Listener-Method to set round-values, only while the corresponding dropdown is interactable.
        /// </summary>
        /// <param name="_toggle"></param>
        private void OnRoundDropdownValueChanged(TMP_Dropdown _maxRoundsDropdown)
        {
            //With the infinity-option at Index 0, Round-Value is equal to DropdownIndex.
            m_matchUIStates.LastRoundDdIndex = _maxRoundsDropdown.value;

            if (!m_matchUIStates.InfiniteMatch && _maxRoundsDropdown.value == 0)
                UpdateDropdowns(_maxRoundsDropdown.value);
            if (m_matchUIStates.InfiniteMatch && _maxRoundsDropdown.value > 0)
                UpdateDropdowns(_maxRoundsDropdown.value);
        }

        /// <summary>
        /// Listener-Method to set maxPoint-values, only while the corresponding dropdown is interactable.
        /// </summary>
        /// <param name="_toggle"></param>
        private void OnMaxPointDropdownValueChanged(TMP_Dropdown _maxPointsDropdown)
        {
            //With the infinity-option at Index 0, MaxPoint-Value is equal to DropdownIndex.
            m_matchUIStates.LastMaxPointDdIndex = _maxPointsDropdown.value;

            if (!m_matchUIStates.InfiniteMatch && _maxPointsDropdown.value == 0)
                UpdateDropdowns(_maxPointsDropdown.value);
            if (m_matchUIStates.InfiniteMatch && _maxPointsDropdown.value > 0)
                UpdateDropdowns(_maxPointsDropdown.value);
        }

        private void OnWidthDropdownValueChanged(TMP_Dropdown _dropdown)
        {
            if (m_fixRatioToggle.isOn)
            {
                //m_fixRatioToggle.isOn = true;
                m_matchSetupDropdowns[3].value = _dropdown.value * 2;
            }

            m_basicFieldValues.SetGroundWidth = _dropdown.value + m_firstWidthOffset;
            m_matchUIStates.LastFieldWidthDdIndex = _dropdown.value;
        }

        private void OnLengthDropdownValueChanged(TMP_Dropdown _dropdown)
        {
            if (m_fixRatioToggle.isOn)
            {
                //m_fixRatioToggle.isOn = true;
                m_matchSetupDropdowns[2].value = (int)(_dropdown.value * 0.5f);
            }

            m_basicFieldValues.SetGroundLength = _dropdown.value + m_firstLengthOffset;
            m_matchUIStates.LastFieldLengthDdIndex = _dropdown.value;
        }

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
                    m_matchUIStates.TPOneFrontlineDdIndex = 0;
                    UpdateFrontlineSetup(m_matchValues.PlayerData[0].PlayerId);
                    break;
                }
                case 1:
                {
                    //Frontline Player 3 (Team 1, ID 2) = Backline Player 1 (Team 1, ID 0).
                    m_backLineDds[0].value = 0;
                    m_matchUIStates.TPOneFrontlineDdIndex = 1;
                    UpdateFrontlineSetup(m_matchValues.PlayerData[2].PlayerId);
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
                    m_matchUIStates.TPTwoFrontlineDdIndex = 0;
                    UpdateFrontlineSetup(m_matchValues.PlayerData[1].PlayerId);
                    break;
                }
                case 1:
                {
                    //Frontline Player 4 (Team 2, ID 3) = Backline Player 2 (Team 2, ID 1).
                    m_backLineDds[1].value = 0;
                    m_matchUIStates.TPTwoFrontlineDdIndex = 1;
                    UpdateFrontlineSetup(m_matchValues.PlayerData[3].PlayerId);
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
                    m_matchUIStates.TPOneBacklineDdIndex = 0;
                    UpdateBacklineSetup(m_matchValues.PlayerData[0].PlayerId);
                    break;
                }
                case 1:
                {
                    //Backline Player 3 (Team 1, ID 2) = Frontline Player 1 (Team 1, ID 0).
                    m_frontLineDds[0].value = 0;
                    m_matchUIStates.TPOneBacklineDdIndex = 1;
                    UpdateBacklineSetup(m_matchValues.PlayerData[2].PlayerId);
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
                    m_matchUIStates.TPTwoBacklineDdIndex = 0;
                    UpdateBacklineSetup(m_matchValues.PlayerData[1].PlayerId);
                    break;
                }
                case 1:
                {
                    //Backline Player 4 (Team 2, ID 3) = Frontline Player 2 (Team 2, ID 1).
                    m_frontLineDds[1].value = 0;
                    m_matchUIStates.TPTwoBacklineDdIndex = 1;
                    UpdateBacklineSetup(m_matchValues.PlayerData[3].PlayerId);
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
            m_basicFieldValues.FrontlineAdjustment = m_distanceSliderValues[0].minValue + _value;
            UpdateLineUpTMPs();
        }

        private void OnBacklineSliderValueChanged(float _value)
        {
            m_distanceSliderValues[1].value = _value;
            m_basicFieldValues.BacklineAdjustment = m_distanceSliderValues[1].minValue + _value;
            UpdateLineUpTMPs();
        }
        #endregion
        #endregion

        #region Custom-Methods
        private void InitializeUISetup()
        {
            m_fixRatioToggle.isOn = m_matchUIStates.FixRatio;
            m_rotationReset[0].isOn = m_matchUIStates.TpOneRotReset;
            m_rotationReset[1].isOn = m_matchUIStates.TpTwoRotReset;

            int roundDdIndex = Array.FindIndex(m_matchSetupDropdowns, (fn) => fn == m_matchSetupDropdowns[0]);
            int maxPointDdIndex = Array.FindIndex(m_matchSetupDropdowns, (fn) => fn == m_matchSetupDropdowns[1]);
            int fieldWidthDdIndex = Array.FindIndex(m_matchSetupDropdowns, (fn) => fn == m_matchSetupDropdowns[2]);
            int fieldLengthDdIndex = Array.FindIndex(m_matchSetupDropdowns, (fn) => fn == m_matchSetupDropdowns[3]);
            SetupMatchDropdowns(roundDdIndex);
            SetupMatchDropdowns(maxPointDdIndex);
            SetupMatchDropdowns(fieldWidthDdIndex);
            SetupMatchDropdowns(fieldLengthDdIndex);

            SetupLineUpSliders();
            UpdateLineUpTMPs();

            UpdateObjectsVisibility(m_matchUIStates.EPlayerAmount);
        }

        private void UpdateObjectsVisibility(EPlayerAmount _ePlayerAmount)
        {
            uint switchPlayerAmount = (uint)_ePlayerAmount;

            switch (switchPlayerAmount)
            {
                case 4:
                {
                    ////In case 4 Player shall play, set the Splitscreen Mode to load to ECameraModi.FourSplit.
                    m_graphicUiStates.SetCameraMode = ECameraModi.FourSplit;
                    ObjectsToHide(true, true, 225.0f);
                    SetupFrontlineDropdowns();
                    break;
                }
                case 2:
                {
                    ////In case 2 Player shall play, set the Splitscreen Mode to load to ECameraModi.TwoHorizontal.
                    m_graphicUiStates.SetCameraMode = ECameraModi.TwoHorizontal;
                    ObjectsToHide(false, false, 714.0f);
                    break;
                }
            }

            SetupBacklineDropdowns();
        }

        private void ObjectsToHide(bool _frontParent, bool _backDropdowns, float _backTextWidth)
        {
            m_frontParentTransform.gameObject.SetActive(_frontParent);
            m_backPTwoDdGroup.gameObject.SetActive(_backDropdowns);
            m_backLineUpText.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _backTextWidth);
        }

        #region Fill-Dropdowns-On-Start
        private void SetupMatchDropdowns(int _dropdownID)
        {
            switch (_dropdownID)
            {
                //Rounds
                case 0:
                {
                    m_matchSetupDropdowns[_dropdownID].ClearOptions();
                    m_roundsDdList = new List<string> { "\u221E" };

                    for (int i = m_firstRoundOffset; i < m_matchUIStates.MaxRounds + 1; i++)
                    {
                        m_roundsDdList.Add(i.ToString());
                    }

                    m_matchSetupDropdowns[_dropdownID].AddOptions(m_roundsDdList);

                    if (m_matchUIStates != null)
                    {
                        m_matchSetupDropdowns[_dropdownID].value = m_matchUIStates.LastRoundDdIndex;
                    }
                    else
                    {
                        DefaultMatchDdValue(_dropdownID);
                    }

                    m_matchSetupDropdowns[_dropdownID].RefreshShownValue();
                    m_matchSetupDropdowns[_dropdownID].interactable = true;

                    m_matchUIStates.LastRoundDdIndex = m_matchSetupDropdowns[_dropdownID].value;
                    break;
                }
                //MaxPoints
                case 1:
                {
                    m_matchSetupDropdowns[_dropdownID].ClearOptions();
                    m_maxPointsDdList = new List<string> { "\u221E" };

                    for (int i = m_firstPointOffset; i < m_matchUIStates.MaxPoints + 1; i++)
                    {
                        //'m_maxPointsDropdown.options.Add (new Dropdown.OptionData() { text = variable });' in foreach-loops.
                        m_maxPointsDdList.Add(i.ToString());
                    }

                    m_matchSetupDropdowns[_dropdownID].AddOptions(m_maxPointsDdList);

                    if (m_matchUIStates != null)
                    {
                        m_matchSetupDropdowns[_dropdownID].value = m_matchUIStates.LastMaxPointDdIndex;
                    }
                    else
                    {
                        DefaultMatchDdValue(_dropdownID);
                    }

                    m_matchSetupDropdowns[_dropdownID].RefreshShownValue();
                    m_matchSetupDropdowns[_dropdownID].interactable = true;

                    m_matchUIStates.LastMaxPointDdIndex = m_matchSetupDropdowns[_dropdownID].value;
                    break;
                }
                //FieldWidth
                case 2:
                {
                    m_widthList = new List<string>();

                    for (int i = m_firstWidthOffset; i < m_maxFieldWidth + 1; i++)
                    {
                        m_widthList.Add(i.ToString());
                    }

                    m_matchSetupDropdowns[_dropdownID].ClearOptions();
                    m_matchSetupDropdowns[_dropdownID].AddOptions(m_widthList);

                    if (m_matchUIStates != null)
                    {
                        m_matchSetupDropdowns[_dropdownID].value = m_matchUIStates.LastFieldWidthDdIndex;
                    }
                    else
                    {
                        DefaultMatchDdValue(_dropdownID);
                    }

                    m_matchSetupDropdowns[_dropdownID].RefreshShownValue();
                    break;
                }
                //FieldLength
                case 3:
                {
                    m_lengthList = new List<string>();

                    for (int i = m_firstLengthOffset; i < m_maxFieldLength + 1; i++)
                    {
                        m_lengthList.Add(i.ToString());
                    }

                    m_matchSetupDropdowns[_dropdownID].ClearOptions();
                    m_matchSetupDropdowns[_dropdownID].AddOptions(m_lengthList);

                    if (m_matchUIStates != null)
                    {
                        m_matchSetupDropdowns[_dropdownID].value = m_matchUIStates.LastFieldLengthDdIndex;
                    }
                    else
                    {
                        DefaultMatchDdValue(_dropdownID);
                    }

                    m_matchSetupDropdowns[_dropdownID].RefreshShownValue();
                    break;
                }
                default:
                    break;
            }
        }
        #endregion

        private void SetupFrontlineDropdowns()
        {
            m_playersTeamOne = new List<string>();
            m_playersTeamTwo = new List<string>();

            for (int i = 0; i < m_matchValues.PlayerData.Count; i++)
            {
                if (m_matchValues.PlayerData[i].PlayerId % 2 == 0)
                    m_playersTeamOne.Add($"Player {i + 1}");
                if (m_matchValues.PlayerData[i].PlayerId % 2 != 0)
                    m_playersTeamTwo.Add($"Player {i + 1}");
            }

            m_frontLineDds[0].ClearOptions();
            m_frontLineDds[0].AddOptions(m_playersTeamOne);

            if (m_matchUIStates != null)
                m_frontLineDds[0].value = m_matchUIStates.TPOneFrontlineDdIndex;
            else
                DefaultFrontlineDdValue(0);

            m_frontLineDds[0].RefreshShownValue();

            m_frontLineDds[1].ClearOptions();
            m_frontLineDds[1].AddOptions(m_playersTeamTwo);

            if (m_matchUIStates != null)
                m_frontLineDds[1].value = m_matchUIStates.TPTwoFrontlineDdIndex;
            else
                DefaultFrontlineDdValue(1);

            m_frontLineDds[1].RefreshShownValue();
        }

        private void SetupBacklineDropdowns()
        {
            m_playersTeamOne = new List<string>();
            m_playersTeamTwo = new List<string>();

            for (int i = 0; i < m_matchValues.PlayerData.Count; i++)
            {
                if (m_matchValues.PlayerData[i].PlayerId % 2 % 2 == 0)
                    m_playersTeamOne.Add($"Player {i + 1}");
                if (m_matchValues.PlayerData[i].PlayerId % 2 % 2 != 0)
                    m_playersTeamTwo.Add($"Player {i + 1}");
            }

            m_backLineDds[0].ClearOptions();
            m_backLineDds[0].AddOptions(m_playersTeamOne);

            if (m_matchUIStates != null)
                m_backLineDds[0].value = m_matchUIStates.TPOneBacklineDdIndex;
            else
                DefaultBacklineDdValue(0);

            m_backLineDds[0].RefreshShownValue();

            m_backLineDds[1].ClearOptions();
            m_backLineDds[1].AddOptions(m_playersTeamTwo);

            if (m_matchUIStates != null)
                m_backLineDds[1].value = m_matchUIStates.TPTwoBacklineDdIndex;
            else
                DefaultBacklineDdValue(1);

            m_backLineDds[1].RefreshShownValue();
        }
        
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

        private void SetupLineUpSliders()
        {
            //FrontSlider
            m_distanceSliderValues[0].value = m_distanceSliderValues[0].minValue + m_basicFieldValues.FrontlineAdjustment;
            //BackSlider
            m_distanceSliderValues[1].value = m_distanceSliderValues[1].minValue + m_basicFieldValues.BacklineAdjustment;
        }

        private void UpdateLineUpTMPs()
        {
            if (m_basicFieldValues == null)
            {
                m_frontFloatText.text = "No Data";
                m_backFloatText.text = "No Data";
                return;
            }

            switch ((int)m_matchUIStates.EPlayerAmount)
            {
                case 4:
                {
                    m_frontFloatText.SetText($"{m_distanceSliderValues[0].value + m_basicFieldValues.MinFrontLineDistance + m_basicFieldValues.BacklineAdjustment:N2}");
                    break;
                }
                default:
                    break;
            }

            m_backFloatText.text = $"{m_distanceSliderValues[1].value + m_basicFieldValues.MinBackLineDistance:N2}";
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
            m_matchValues.PlayerData[_playerId].PlayerOnFrontline = true;
        }

        private void UpdateBacklineSetup(int _playerId)
        {
            m_matchValues.PlayerData[_playerId].PlayerOnFrontline = false;
        }

        private void UpdateDropdowns(int _dropdownIndex)
        {
            switch (_dropdownIndex)
            {
                case 0:
                {
                    m_matchUIStates.InfiniteMatch = true;

                    //Only Round and MaxPoint-Dropdowns shall get set.
                    for (int i = 0; i < m_matchSetupDropdowns.Length - 2; i++)
                        if (m_matchSetupDropdowns[i].value != _dropdownIndex)
                            m_matchSetupDropdowns[i].value = _dropdownIndex;
                    break;
                }
                default:
                {
                    m_matchUIStates.InfiniteMatch = false;
                    //Only Round and MaxPoint-Dropdowns shall get set.
                    for (int i = 0; i < m_matchSetupDropdowns.Length - 2; i++)
                        if (m_matchSetupDropdowns[i].value == 0)
                            m_matchSetupDropdowns[i].value++;
                    break;
                }
            }
        }

        public void DefaultMatchDdValue(int _tmpDropdownIndex)
        {
            switch (_tmpDropdownIndex)
            {
                case 0:
                {
                    m_matchSetupDropdowns[0].value = m_maxRoundDdIndex;
                    break;
                }
                case 1:
                {
                    m_matchSetupDropdowns[1].value = m_maxPointDdIndex;
                    break;
                }
                case 2:
                {
                    m_matchSetupDropdowns[2].value = m_fieldWidthDdIndex;
                    break;
                }
                case 3:
                {
                    m_matchSetupDropdowns[3].value = m_fieldLengthDdIndex;
                    break;
                }                
                default:
                    break;
            }
        }

        private void DefaultBacklineDdValue(int _index)
        {
            switch (_index)
            {
                case 0:
                    m_backLineDds[0].value = m_backLineDefaultValue;
                    break;
                case 1:
                    m_backLineDds[1].value = m_backLineDefaultValue;
                    break;
                default:
                    break;
            }
        }

        private void DefaultFrontlineDdValue(int _index)
        {
            switch (_index)
            {
                case 0:
                    m_frontLineDds[0].value = m_frontLineDefaultValue;
                    break;
                case 1:
                    m_frontLineDds[1].value = m_frontLineDefaultValue;
                    break;
                default:
                    break;
            }
        }

        public void ReSetDefault()
        {
            m_fixRatioToggle.isOn = m_fixAspectRatio;
            m_rotationReset[0].isOn = m_TpOneRotResetDefault;
            m_rotationReset[1].isOn = m_TpTwoRotResetDefault;

            m_matchSetupDropdowns[0].value = m_maxRoundDdIndex;
            m_matchSetupDropdowns[1].value = m_maxPointDdIndex;
            m_matchSetupDropdowns[2].value = m_fieldWidthDdIndex;
            m_matchSetupDropdowns[3].value = m_fieldLengthDdIndex;

            m_distanceSliderValues[0].value = m_distanceSliderDefaults;
            m_distanceSliderValues[1].value = m_distanceSliderDefaults;

            m_backLineDds[0].value = m_backLineDefaultValue;
            m_backLineDds[1].value = m_backLineDefaultValue;
        }
        #endregion
    }
}