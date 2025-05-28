using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ThreeDeePongProto.Shared.Settings
{
    public class MatchSettings : MonoBehaviour
    {
        //TODO: Desired InputField behavior to select the gameObject without a blinking cursor and to enable editing on pressing Enter.
        #region SerializeField-Member-Variables
        #region Player-Names
        [Header("Player-Details")]
        //OnEndEdit in Unity sets the PlayerNames from MatchSettings UI.        
        [SerializeField] private Toggle m_rotationReset;
        [SerializeField] private TextMeshProUGUI m_rotationResetText;
        [SerializeField] private bool m_paddleRotResetDefault = true;
        #endregion

        #region Rounds and Points
        [Header("Rounds and Points")]
        [SerializeField] private TMP_Dropdown m_roundsDropdown;
        [SerializeField] private TMP_Dropdown m_maxPointsDropdown;
        [SerializeField] private int m_maxRoundDdIndex = 5;
        [SerializeField] private int m_maxPointDdIndex = 25;
        #endregion

        #region Field-Dimension
        [Header("Field-Dimension")]
        [SerializeField] private Toggle m_fixRatioToggle;
        //[SerializeField] private TMP_Dropdown[] m_fieldScaleDropdowns;
        [SerializeField] private TMP_Dropdown m_fieldWidthDropdown;
        [SerializeField] private TMP_Dropdown m_fieldLengthDropdown;

        [SerializeField] private int m_maxFieldWidth = 30;
        [SerializeField] private int m_maxFieldLength = 60;
        [SerializeField] private int m_fieldWidthDdResetTo = 0;
        [SerializeField] private int m_fieldLengthDdResetTo = 0;
        [SerializeField] private bool m_fixAspectRatio = false;
        #endregion

        #region Line-Ups
        [Header("Line-Up")]
        [SerializeField] private TextMeshProUGUI m_backFloatText;
        [SerializeField] private TextMeshProUGUI m_frontFloatText;
        [SerializeField] private float m_distanceSliderDefaults = 0;
        [SerializeField] private float m_sliderAdjustStep = 0.01f;
        [Space]
        [SerializeField] private TMP_Dropdown[] m_frontLineDds;
        [SerializeField] private TMP_Dropdown[] m_backLineDds;
        [SerializeField] private int m_lineDdResetTo = 0;
        #endregion

        #region Hiding
        [Header("Hiding")]
        [SerializeField] private Transform m_frontParentTransform;
        [SerializeField] private Transform m_backLineDdGroup;
        [SerializeField] private TextMeshProUGUI m_backLineUpText;
        #endregion
        #endregion
        [Space]

        #region Lists and Dictionaries
        [SerializeField] private List<Button> m_reduceButtonKeys = new();
        [SerializeField] private List<Button> m_increaseButtonKeys = new();
        [SerializeField] private List<Slider> m_distanceSliderValues = new();

        private List<string> m_roundsDdList;
        private List<string> m_maxPointsDdList;
        private List<string> m_widthList;
        private List<string> m_lengthList;

        private List<string> m_playersTeamOne;
        private List<string> m_playersTeamTwo;

        #region Key-Value-Connection
        private readonly Dictionary<Button, Slider> m_reduceLineSlider = new();
        private readonly Dictionary<Button, Slider> m_increaseLineSlider = new();
        #endregion
        #endregion

        private readonly int m_firstRoundOffset = 1, m_firstPointOffset = 1, m_firstWidthOffset = 25, m_firstLengthOffset = 50;

        #region Scriptable-Objects
        [Header("Scriptable Objects")]
        [SerializeField] private BasicFieldValues m_basicFieldValues;
        [SerializeField] private MatchUIStates m_matchUIStates;
        [SerializeField] private MatchValues m_matchValues;
        [SerializeField] private GraphicUIStates m_graphicUiStates;
        [SerializeField] private PlayerSOData[] m_playerSOData;
        #endregion

        private int m_lastAmount = -1;

        #region Serialization
        private readonly string m_settingsStatesFolderPath = "/SaveData/Settings-States";
        private readonly string m_fieldSettingsPath = "/SaveData/FieldSettings";
        private readonly string m_playerDataFolderPath = "/SaveData/PlayerData";
        private readonly string m_playerDataSubPath = "/Player";
        private readonly string m_matchFileName = "/Match";
        private readonly string m_fileFormat = ".json";

        private readonly IPersistentData m_persistentData = new SerializingData();
        private readonly bool m_encryptionEnabled = false;
        #endregion

        private void Awake()
        {
            SetupLineDictionaries();

            if (m_matchUIStates == null || m_matchValues == null)
                ReSetDefault();
            //else LoadMatchSettings(); moved to 'MenuNavigation.cs'.
        }

        private void OnEnable()
        {
            //TODO: InitialUISetup and UpdateLineUpTMPs check for nulled Scriptable.
            InitializeUISetup();
            AddGroupListeners();
        }

        private void OnDisable()
        {
            RemoveGroupListeners();

            m_persistentData.SaveData(m_settingsStatesFolderPath, m_matchFileName, m_fileFormat, m_matchUIStates, m_encryptionEnabled, true);
            m_persistentData.SaveData(m_fieldSettingsPath, m_matchFileName, m_fileFormat, m_basicFieldValues, m_encryptionEnabled, true);
        }

        #region UnRegister Listener Region
        private void AddGroupListeners()
        {
            //Toggle to allow or deny PaddleRotation-Resets on each Goal.
            m_rotationReset.onValueChanged.AddListener(HandleRotationToggleValueChanges);

            //Rounds
            m_roundsDropdown.onValueChanged.AddListener(OnRoundDropdownValueChanged);
            //MaxPoints
            m_maxPointsDropdown.onValueChanged.AddListener(OnMaxPointDropdownValueChanged);

            m_fixRatioToggle.onValueChanged.AddListener(OnRatioToggleValueChanged);
            //Field-Width
            m_fieldWidthDropdown.onValueChanged.AddListener(OnWidthDropdownValueChanged);
            //Field-Length
            m_fieldLengthDropdown.onValueChanged.AddListener(OnLengthDropdownValueChanged);

            if ((int)m_matchUIStates.EPlayerAmount > 3)
            {
                //PlayerCharacter-Set-Frontline
                m_frontLineDds[0].onValueChanged.AddListener(delegate
                { OnTeamOneFrontlineDropdownValueChanged(m_frontLineDds[0]); });
                m_frontLineDds[1].onValueChanged.AddListener(delegate
                { OnTeamTwoFrontlineDropdownValueChanged(m_frontLineDds[1]); });
                m_distanceSliderValues[0].onValueChanged.AddListener(OnFrontlineSliderValueChanged);
            }

            //PlayerCharacter-Set-Backline
            m_backLineDds[0].onValueChanged.AddListener(delegate
            { OnTeamOneBacklineDropdownValueChanged(m_backLineDds[0]); });
            m_backLineDds[1].onValueChanged.AddListener(delegate
            { OnTeamTwoBacklineDropdownValueChanged(m_backLineDds[1]); });
            m_distanceSliderValues[1].onValueChanged.AddListener(OnBacklineSliderValueChanged);
        }

        private void RemoveGroupListeners()
        {
            //Toggle to allow or deny PaddleRotation-Resets on each Goal.
            m_rotationReset.onValueChanged.RemoveListener(HandleRotationToggleValueChanges);

            //Rounds
            m_roundsDropdown.onValueChanged.RemoveListener(OnRoundDropdownValueChanged);
            //MaxPoints
            m_maxPointsDropdown.onValueChanged.RemoveListener(OnMaxPointDropdownValueChanged);

            m_fixRatioToggle.onValueChanged.RemoveListener(OnRatioToggleValueChanged);
            //Field-Width
            m_fieldWidthDropdown.onValueChanged.RemoveListener(OnWidthDropdownValueChanged);
            //Field-Length
            m_fieldLengthDropdown.onValueChanged.RemoveListener(OnLengthDropdownValueChanged);

            if (m_matchValues.PlayerSOData.Count > 3)
            {
                //PlayerCharacter-Set-Frontline
                m_frontLineDds[0].onValueChanged.RemoveListener(delegate
                { OnTeamOneFrontlineDropdownValueChanged(m_frontLineDds[0]); });
                m_frontLineDds[1].onValueChanged.RemoveListener(delegate
                { OnTeamTwoFrontlineDropdownValueChanged(m_frontLineDds[1]); });
                m_distanceSliderValues[0].onValueChanged.RemoveListener(OnFrontlineSliderValueChanged);
            }

            //PlayerCharacter-Set-Backline
            m_backLineDds[0].onValueChanged.RemoveListener(delegate
            { OnTeamOneBacklineDropdownValueChanged(m_backLineDds[0]); });
            m_backLineDds[1].onValueChanged.RemoveListener(delegate
            { OnTeamTwoBacklineDropdownValueChanged(m_backLineDds[1]); });
            m_distanceSliderValues[1].onValueChanged.RemoveListener(OnBacklineSliderValueChanged);
        }
        #endregion

        #region Listener Methods
        #region Toggle-OnValueChanged-Methods
        /// <summary>
        /// Enabling this Toggle shall fix changes of width and length of the playfield to 1:2 ratio, while changing values on one of the two dropdowns.
        /// </summary>
        /// <param name="_toggle"></param>
        private void OnRatioToggleValueChanged(bool _isOn)
        {
            switch (_isOn)
            {
                case true:
                {
                    m_fieldLengthDropdown.value = m_fieldWidthDropdown.value * 2;    //Length Dd value = width Dd value * 2.
                    m_matchUIStates.FixRatio = _isOn;
                    break;
                }
                case false:
                {
                    m_matchUIStates.FixRatio = _isOn;
                    break;
                }
            }
        }

        private void HandleRotationToggleValueChanges(bool _toggle)
        {
            m_matchUIStates.RotationReset = _toggle;
            SetRotationResetText(_toggle);
        }
        #endregion

        #region Dropdown-OnValueChanged-Methods
        /// <summary>
        /// Listener-Method to set round-values, only while the corresponding dropdown is interactable.
        /// </summary>
        /// <param name="_toggle"></param>
        private void OnRoundDropdownValueChanged(int _dropdownValue)
        {
            //With the infinity-option at Index 0, Round-Value is equal to DropdownIndex.
            m_matchUIStates.LastRoundDdIndex = _dropdownValue;  //Save last set roundDropdown value in Scriptable. 

            if (!m_matchUIStates.InfiniteMatch && _dropdownValue == 0)
            {
                m_roundsDropdown.value = _dropdownValue;
                m_matchUIStates.InfiniteMatch = true;
            }

            if (m_matchUIStates.InfiniteMatch && _dropdownValue > 0)
            {
                m_roundsDropdown.value = _dropdownValue;
                m_matchUIStates.InfiniteMatch = false;
            }
        }

        /// <summary>
        /// Listener-Method to set maxPoint-values, only while the corresponding dropdown is interactable.
        /// </summary>
        /// <param name="_toggle"></param>
        private void OnMaxPointDropdownValueChanged(int _dropdownValue)
        {
            //With the infinity-option at Index 0, MaxPoint-Value is equal to DropdownIndex.
            m_matchUIStates.LastMaxPointDdIndex = _dropdownValue;     //Save last set maxPointsDropdown value in Scriptable.

            if (!m_matchUIStates.InfiniteMatch && _dropdownValue == 0)
            {
                m_maxPointsDropdown.value = _dropdownValue;
                m_matchUIStates.InfiniteMatch = true;
            }

            if (m_matchUIStates.InfiniteMatch && _dropdownValue > 0)
            {
                m_maxPointsDropdown.value = _dropdownValue;
                m_matchUIStates.InfiniteMatch = false;
            }
        }

        private void OnWidthDropdownValueChanged(int _dropdownValue)
        {
            if (m_fixRatioToggle.isOn)
                m_fieldLengthDropdown.value = _dropdownValue * 2;

            m_basicFieldValues.SetGroundWidth = _dropdownValue + m_firstWidthOffset;
            m_matchUIStates.LastFieldWidthDdIndex = _dropdownValue;         //Save last set fieldWidthDropdown value in Scriptable.
        }

        private void OnLengthDropdownValueChanged(int _dropdownValue)
        {
            if (m_fixRatioToggle.isOn)
                m_fieldWidthDropdown.value = (int)(_dropdownValue * 0.5f);

            m_basicFieldValues.SetGroundLength = _dropdownValue + m_firstLengthOffset;
            m_matchUIStates.LastFieldLengthDdIndex = _dropdownValue;        //Save last set fieldLengthDropdown value in Scriptable.
        }

        /// <summary>
        /// dropdownIndex-Changes set Booleans on playerSOData Scriptable to set their goalDistance-Positions on Match-Start.
        /// </summary>
        /// <param name="_dropdown"></param>
        private void OnTeamOneFrontlineDropdownValueChanged(TMP_Dropdown _dropdown)
        {
            switch (_dropdown.value)
            {
                case 0:
                {
                    //Frontline PlayerCharacter 1 (Team 1, ID 0) = Backline PlayerCharacter 3 (Team 1, ID 2).
                    m_backLineDds[0].value = 1;
                    m_matchUIStates.TPOneFrontlineDdIndex = 0;
                    UpdateFrontlineSetup(m_matchValues.PlayerSOData[0].PlayerId);
                    break;
                }
                case 1:
                {
                    //Frontline PlayerCharacter 3 (Team 1, ID 2) = Backline PlayerCharacter 1 (Team 1, ID 0).
                    m_backLineDds[0].value = 0;
                    m_matchUIStates.TPOneFrontlineDdIndex = 1;
                    UpdateFrontlineSetup(m_matchValues.PlayerSOData[2].PlayerId);
                    break;
                }
                default:
                { break; }
            }
        }

        /// <summary>
        /// dropdownIndex-Changes set Booleans on playerSOData Scriptable to set their goalDistance-Positions on Match-Start.
        /// </summary>
        /// <param name="_dropdown"></param>
        private void OnTeamTwoFrontlineDropdownValueChanged(TMP_Dropdown _dropdown)
        {
            switch (_dropdown.value)
            {
                case 0:
                {
                    //Frontline PlayerCharacter 2 (Team 2, ID 1) = Backline PlayerCharacter 4 (Team 2, ID 3).
                    m_backLineDds[1].value = 1;
                    m_matchUIStates.TPTwoFrontlineDdIndex = 0;
                    UpdateFrontlineSetup(m_matchValues.PlayerSOData[1].PlayerId);
                    break;
                }
                case 1:
                {
                    //Frontline PlayerCharacter 4 (Team 2, ID 3) = Backline PlayerCharacter 2 (Team 2, ID 1).
                    m_backLineDds[1].value = 0;
                    m_matchUIStates.TPTwoFrontlineDdIndex = 1;
                    UpdateFrontlineSetup(m_matchValues.PlayerSOData[3].PlayerId);
                    break;
                }
                default:
                { break; }
            }
        }

        /// <summary>
        /// dropdownIndex-Changes set Booleans on playerSOData Scriptable to set their goalDistance-Positions on Match-Start.
        /// </summary>
        /// <param name="_dropdown"></param>
        private void OnTeamOneBacklineDropdownValueChanged(TMP_Dropdown _dropdown)
        {
            switch (_dropdown.value)
            {
                case 0:
                {
                    //Backline PlayerCharacter 1 (Team 1, ID 0) = Frontline PlayerCharacter 3 (Team 1, ID 2).
                    m_frontLineDds[0].value = 1;
                    m_matchUIStates.TPOneBacklineDdIndex = 0;
                    UpdateBacklineSetup(m_matchValues.PlayerSOData[0].PlayerId);
                    break;
                }
                case 1:
                {
                    //Backline PlayerCharacter 3 (Team 1, ID 2) = Frontline PlayerCharacter 1 (Team 1, ID 0).
                    m_frontLineDds[0].value = 0;
                    m_matchUIStates.TPOneBacklineDdIndex = 1;
                    UpdateBacklineSetup(m_matchValues.PlayerSOData[2].PlayerId);
                    break;
                }
                default:
                { break; }
            }
        }

        /// <summary>
        /// dropdownIndex-Changes set Booleans on playerSOData Scriptable to set their goalDistance-Positions on Match-Start.
        /// </summary>
        /// <param name="_dropdown"></param>
        private void OnTeamTwoBacklineDropdownValueChanged(TMP_Dropdown _dropdown)
        {
            switch (_dropdown.value)
            {
                case 0:
                {
                    //Backline PlayerCharacter 2 (Team 2, ID 1) = Frontline PlayerCharacter 4 (Team 2, ID 3).
                    m_frontLineDds[1].value = 1;
                    m_matchUIStates.TPTwoBacklineDdIndex = 0;
                    UpdateBacklineSetup(m_matchValues.PlayerSOData[1].PlayerId);
                    break;
                }
                case 1:
                {
                    //Backline PlayerCharacter 4 (Team 2, ID 3) = Frontline PlayerCharacter 2 (Team 2, ID 1).
                    m_frontLineDds[1].value = 0;
                    m_matchUIStates.TPTwoBacklineDdIndex = 1;
                    UpdateBacklineSetup(m_matchValues.PlayerSOData[3].PlayerId);
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
            m_rotationReset.isOn = m_matchUIStates.RotationReset;
            SetRotationResetText(m_rotationReset.isOn);

            SetupMatchDropdowns(m_roundsDropdown);
            SetupMatchDropdowns(m_maxPointsDropdown);
            SetupMatchDropdowns(m_fieldWidthDropdown);
            SetupMatchDropdowns(m_fieldLengthDropdown);

            SetupDistanceSliders();
            UpdateLineUpTMPs();

            SetPlayerData((int)m_matchUIStates.EPlayerAmount);
            UpdateObjectsVisibility(m_lastAmount);
        }

        private void SetRotationResetText(bool _rotationReset)
        {
            m_rotationResetText.text = _rotationReset == true ? "On" : "Off";
        }

        private void SetPlayerData(int _playerAmount)
        {
            if (m_lastAmount == _playerAmount)
                return;

            m_lastAmount = _playerAmount;

            m_matchValues.PlayerSOData.Clear();
            m_matchValues.PlayerSOData = new();

            for (int i = 0; i < m_lastAmount; i++)  //EPlayerAmount.Four => int 4 || EPlayerAmount.Two => int 2
            {
                if (m_playerSOData.Length > 0 && m_playerSOData[i] != null)
                    m_matchValues.PlayerSOData.Add(m_playerSOData[i]);
            }
        }

        private void UpdateObjectsVisibility(int _playerAmount)
        {
            m_playersTeamOne = new List<string>();
            m_playersTeamTwo = new List<string>();
            m_playersTeamOne.Clear();
            m_playersTeamTwo.Clear();

            int playerAmount = _playerAmount;    //EPlayerAmount.Four => int 4 || EPlayerAmount.Two => int 2
            for (int playerID = 0; playerID < playerAmount; playerID++)
            {
                if (playerID % 2 == 0)
                    m_playersTeamOne.Add($"Player {playerID + 1}");
                if (playerID % 2 != 0)
                    m_playersTeamTwo.Add($"Player {playerID + 1}");
            }

            if (m_matchUIStates == null)
                Debug.LogWarning($"matchUIStates Scriptable is null. Please set it up in the inspector!");

            SetupBacklineDropdowns();

            switch (playerAmount)
            {
                case 4:
                {
                    //In case 4 PlayerCharacter shall play, set the SplitScreen Mode to load to ECameraModi.Quartet.
                    m_graphicUiStates.SetCameraMode = ECameraModi.Quartet;
                    ObjectsToHide(true, true, 225.0f);
                    SetupFrontlineDropdowns();
                    break;
                }
                case 2:
                {
                    //In case 2 PlayerCharacter shall play, set the SplitScreen Mode to load to ECameraModi.Horizontal.
                    m_graphicUiStates.SetCameraMode = ECameraModi.Horizontal;
                    ObjectsToHide(false, false, 714.0f);
                    break;
                }
            }
        }

        private void ObjectsToHide(bool _frontParent, bool _backDropdowns, float _backTextWidth)
        {
            m_frontParentTransform.gameObject.SetActive(_frontParent);
            m_backLineDdGroup.gameObject.SetActive(_backDropdowns);
            m_backLineUpText.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _backTextWidth);
        }

        #region Fill-Dropdowns-On-Start
        private void SetupMatchDropdowns(TMP_Dropdown _dropdown)
        {
            _dropdown.ClearOptions();

            #region Round_Dropdown
            if (_dropdown == m_roundsDropdown)
            {
                m_roundsDdList = new List<string> { "\u221E" };

                for (int i = m_firstRoundOffset; i < m_matchUIStates.MaxRounds + 1; i++)
                    m_roundsDdList.Add(i.ToString());

                m_roundsDropdown.AddOptions(m_roundsDdList);

                if (m_matchUIStates != null)
                    m_roundsDropdown.value = m_matchUIStates.LastRoundDdIndex;
                else
                    m_roundsDropdown.value = m_maxRoundDdIndex;

                //m_matchUIStates.LastRoundDdIndex = m_roundsDropdown.value;
            }
            #endregion

            #region MaxPoints_Dropdown
            if (_dropdown == m_maxPointsDropdown)
            {
                m_maxPointsDdList = new List<string> { "\u221E" };

                for (int i = m_firstPointOffset; i < m_matchUIStates.MaxPoints + 1; i++)
                    m_maxPointsDdList.Add(i.ToString());

                m_maxPointsDropdown.AddOptions(m_maxPointsDdList);

                if (m_matchUIStates != null)
                    m_maxPointsDropdown.value = m_matchUIStates.LastMaxPointDdIndex;
                else
                    m_maxPointsDropdown.value = m_maxPointDdIndex;

                //m_matchUIStates.LastMaxPointDdIndex = m_maxPointsDropdown.value;
            }
            #endregion

            #region FieldWidth_Dropdown
            if (_dropdown == m_fieldWidthDropdown)
            {
                m_widthList = new List<string>();

                for (int i = m_firstWidthOffset; i < m_maxFieldWidth + 1; i++)
                    m_widthList.Add(i.ToString());

                m_fieldWidthDropdown.AddOptions(m_widthList);

                if (m_matchUIStates != null)
                    m_fieldWidthDropdown.value = m_matchUIStates.LastFieldWidthDdIndex;
                else
                    m_fieldWidthDropdown.value = m_maxFieldWidth;

                //m_matchUIStates.LastFieldWidthDdIndex = m_fieldWidthDropdown.value;
            }
            #endregion

            #region FieldLength_Dropdown
            if (_dropdown == m_fieldLengthDropdown)
            {
                m_lengthList = new List<string>();

                for (int i = m_firstLengthOffset; i < m_maxFieldLength + 1; i++)
                    m_lengthList.Add(i.ToString());

                m_fieldLengthDropdown.AddOptions(m_lengthList);

                if (m_matchUIStates != null)
                    m_fieldLengthDropdown.value = m_matchUIStates.LastFieldLengthDdIndex;
                else
                    m_fieldLengthDropdown.value = m_maxFieldLength;

                //m_matchUIStates.LastFieldLengthDdIndex = m_fieldLengthDropdown.value;
            }
            #endregion

            _dropdown.RefreshShownValue();
            _dropdown.interactable = true;
        }
        #endregion

        private void SetupFrontlineDropdowns()
        {
            if (m_frontLineDds.Length > 0)
            {
                foreach (var dropdown in m_frontLineDds)
                    dropdown.ClearOptions();

                m_frontLineDds[0].AddOptions(m_playersTeamOne);
                m_frontLineDds[1].AddOptions(m_playersTeamTwo);

                if (m_matchUIStates != null)
                {
                    m_frontLineDds[0].value = m_matchUIStates.TPOneFrontlineDdIndex;
                    m_frontLineDds[1].value = m_matchUIStates.TPTwoFrontlineDdIndex;
                }
                else
                {
                    m_frontLineDds[0].value = 0;
                    m_frontLineDds[1].value = 1;
                }

                foreach (var dropdown in m_frontLineDds)
                    dropdown.RefreshShownValue();
            }
        }

        private void SetupBacklineDropdowns()
        {
            if (m_backLineDds.Length > 0)
            {
                foreach (var dropdown in m_backLineDds)
                    dropdown.ClearOptions();

                m_backLineDds[0].AddOptions(m_playersTeamOne);
                m_backLineDds[1].AddOptions(m_playersTeamTwo);

                if (m_matchUIStates != null)
                {
                    m_backLineDds[0].value = m_matchUIStates.TPOneBacklineDdIndex;
                    m_backLineDds[1].value = m_matchUIStates.TPTwoBacklineDdIndex;
                }
                else
                {
                    m_backLineDds[0].value = 0;
                    m_backLineDds[1].value = 1;
                }

                foreach (var dropdown in m_backLineDds)
                    dropdown.RefreshShownValue();
            }
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

        private void SetupDistanceSliders()
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
            m_matchValues.PlayerSOData[_playerId].PlayerOnFrontline = true;
            SavePlayerData(_playerId);
        }

        private void UpdateBacklineSetup(int _playerId)
        {
            m_matchValues.PlayerSOData[_playerId].PlayerOnFrontline = false;
            SavePlayerData(_playerId);
        }

        private void SavePlayerData(int _index)
        {
            //Scriptable Objects CAN be used like structs to save data. BUT not, if they got foreign/extra references, like gameObject-Prefabs or Sprites. Need to save their names as string instead.
            if (m_playerSOData[_index] == null)
                return;

            var playerSO = m_playerSOData[_index];
            var prefabName = playerSO.Prefab.name;
            var avatarName = playerSO.Avatar.name;
            var toggleID = playerSO.ToggleID;

            PlayerData playerData = new(prefabName, playerSO.PlayerName, playerSO.PlayerId, avatarName, playerSO.KeepNameOnLoad, playerSO.PlayerOnFrontline, playerSO.DefaultKeyboard, toggleID);
            m_persistentData.SaveData(m_playerDataFolderPath, m_playerDataSubPath + $"{_index}", m_fileFormat, playerData, m_encryptionEnabled, true);
        }

        public void ReSetDefault()
        {
            m_rotationReset.isOn = m_paddleRotResetDefault;

            m_roundsDropdown.value = m_maxRoundDdIndex;
            m_maxPointsDropdown.value = m_maxPointDdIndex;

            m_fixRatioToggle.isOn = m_fixAspectRatio;
            m_fieldWidthDropdown.value = m_fieldWidthDdResetTo;
            m_fieldLengthDropdown.value = m_fieldLengthDdResetTo;

            m_distanceSliderValues[0].value = m_distanceSliderDefaults;
            m_distanceSliderValues[1].value = m_distanceSliderDefaults;

            m_backLineDds[0].value = m_lineDdResetTo;
            m_backLineDds[1].value = m_lineDdResetTo + 1;
            m_frontLineDds[0].value = m_lineDdResetTo;
            m_frontLineDds[1].value = m_lineDdResetTo + 1;
        }
        #endregion
    }
}