using System;
using System.Collections.Generic;
using ThreeDeePongProto.Shared.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum EGameMode
{ Infinite = 0, SuddenDeath = 1, Normal = 2 }

namespace ThreeDeePongProto.Shared.Settings
{
    public class MatchSettings : MonoBehaviour
    {
        #region UI References
        [Header("Game Mode")]
        [SerializeField] private TMP_Dropdown m_gameModeDropdown;
        [SerializeField] private Toggle m_obstacleToggle;
        [SerializeField] private bool m_isNormalMode = true;

        [Header("Rounds and Points")]
        [SerializeField] private TMP_Dropdown m_roundsDropdown;
        [SerializeField] private TMP_Dropdown m_maxPointsDropdown;

        [Header("Field-Dimension")]
        [SerializeField] private Toggle m_fixRatioToggle;
        [SerializeField] private TMP_Dropdown m_fieldWidthDropdown;
        [SerializeField] private TMP_Dropdown m_fieldLengthDropdown;

        [Header("Player-Details")]
        [SerializeField] private TMP_Dropdown m_playerAmountDd;
        [SerializeField] private Toggle m_rotationReset;
        [SerializeField] private TextMeshProUGUI m_rotationResetText;

        [Header("Line-Up")]
        [SerializeField] private Slider[] m_distanceSliders;
        [SerializeField] private TextMeshProUGUI m_backFloatText;
        [SerializeField] private TextMeshProUGUI m_frontFloatText;
        [SerializeField, Range(1.0f, 10.0f)] private float m_adjustSliderStep = 1.0f;
        [Space]
        [SerializeField] private TMP_Dropdown[] m_backLineDds;
        [SerializeField] private TMP_Dropdown[] m_frontLineDds;

        [Header("UI Visibilty Change")]
        [SerializeField] private TMP_Text m_teamText;
        [SerializeField] private Transform m_backLineSlot;
        [SerializeField] private Transform m_frontLineSlot;
        #endregion
        [SerializeField] private Selectable[] m_btnContainer;

        [Header("Navigation Rows")]
        [SerializeField] private Selectable[] m_backlineSelectables;   //Button, Slider, Dropdown for Team 1
        [SerializeField] private Selectable[] m_frontlineSelectables;  //Button, Slider, Dropdown for Team 2
        [Space]

        private const int m_maxPlayers = 4;
        private int m_maxRounds, m_maxPointsEachRound;
        private const int m_minFieldWidth = 25, m_maxFieldWidth = 30;
        private const int m_minFieldLength = 50, m_maxFieldLength = 60;
        private const string m_player1 = "Player 1", m_player2 = "Player 2", m_player3 = "Player 3", m_player4 = "Player 4";

        private MatchSettingsData m_matchData;

        private void Awake()
        {
            m_maxRounds = SettingsManager.Instance.MaxRounds;
            m_maxPointsEachRound = SettingsManager.Instance.MaxRoundPoints;
        }

        private void OnEnable()
        {
            m_matchData = SettingsManager.Instance.CurrentSettings.Match;

            SettingsManager.Instance.OnSettingsChanged += SetUIElements;
            SetUIElements();
        }

        private void OnDisable()
        {
            SettingsManager.Instance.OnSettingsChanged -= SetUIElements;
            RemoveListeners();
            SettingsManager.Instance.SaveSettings();
        }

        #region UnRegister Listener Region
        private void AddListeners()
        {
            //Game-Modi
            m_gameModeDropdown.onValueChanged.AddListener(OnGameModeChanged);
            //Rounds
            m_roundsDropdown.onValueChanged.AddListener(OnRoundDropdownChanged);
            //PointSetting
            m_maxPointsDropdown.onValueChanged.AddListener(OnMaxPointDropdownChanged);
            //Obstacle-Option
            m_obstacleToggle.onValueChanged.AddListener(OnObstacleToggleChanged);
            //Set fixField ratio
            m_fixRatioToggle.onValueChanged.AddListener(OnRatioToggleChanged);
            //Field-Width
            m_fieldWidthDropdown.onValueChanged.AddListener(OnWidthDropdownChanged);
            //Field-Length
            m_fieldLengthDropdown.onValueChanged.AddListener(OnLengthDropdownChanged);
            //PlayerAmount in game
            m_playerAmountDd.onValueChanged.AddListener(OnPlayerAmountChanged);
            //Toggle to allow or deny PaddleRotation-Resets on each Goal
            m_rotationReset.onValueChanged.AddListener(OnRotationToggleChanged);
            //BackLine
            m_backLineDds[0].onValueChanged.AddListener((value) => OnPlayerPositionChanged(0, EPlayerLine.Backline, value));    //Team 1
            m_backLineDds[1].onValueChanged.AddListener((value) => OnPlayerPositionChanged(1, EPlayerLine.Backline, value));    //Team 2
            m_distanceSliders[0].onValueChanged.AddListener(OnBacklineSliderChanged);
            //FrontLine
            m_frontLineDds[0].onValueChanged.AddListener((value) => OnPlayerPositionChanged(0, EPlayerLine.Frontline, value));  //Team 1
            m_frontLineDds[1].onValueChanged.AddListener((value) => OnPlayerPositionChanged(1, EPlayerLine.Frontline, value));  //Team 2
            m_distanceSliders[1].onValueChanged.AddListener(OnFrontlineSliderChanged);
        }

        private void RemoveListeners()
        {
            //Game-Modi
            m_gameModeDropdown.onValueChanged.RemoveListener(OnGameModeChanged);
            //Rounds
            m_roundsDropdown.onValueChanged.RemoveListener(OnRoundDropdownChanged);
            //PointSetting
            m_maxPointsDropdown.onValueChanged.RemoveListener(OnMaxPointDropdownChanged);
            //Obstacle-Option
            m_obstacleToggle.onValueChanged.RemoveListener(OnObstacleToggleChanged);
            //Set fixField ratio
            m_fixRatioToggle.onValueChanged.RemoveListener(OnRatioToggleChanged);
            //Field-Width
            m_fieldWidthDropdown.onValueChanged.RemoveListener(OnWidthDropdownChanged);
            //Field-Length
            m_fieldLengthDropdown.onValueChanged.RemoveListener(OnLengthDropdownChanged);
            //PlayerAmount in game
            m_playerAmountDd.onValueChanged.RemoveListener(OnPlayerAmountChanged);
            //Toggle to allow or deny PaddleRotation-Resets on each Goal
            m_rotationReset.onValueChanged.RemoveListener(OnRotationToggleChanged);
            //BackLine
            m_backLineDds[0].onValueChanged.RemoveListener((value) => OnPlayerPositionChanged(0, EPlayerLine.Backline, value));    //Team 1
            m_backLineDds[1].onValueChanged.RemoveListener((value) => OnPlayerPositionChanged(1, EPlayerLine.Backline, value));    //Team 2
            m_distanceSliders[0].onValueChanged.RemoveListener(OnBacklineSliderChanged);
            //FrontLine
            m_frontLineDds[0].onValueChanged.RemoveListener((value) => OnPlayerPositionChanged(0, EPlayerLine.Frontline, value));  //Team 1
            m_frontLineDds[1].onValueChanged.RemoveListener((value) => OnPlayerPositionChanged(1, EPlayerLine.Frontline, value));  //Team 2
            m_distanceSliders[1].onValueChanged.RemoveListener(OnFrontlineSliderChanged);
        }
        #endregion

        #region Public Button Methods
        public void IncreaseBackline() => m_distanceSliders[0].value += m_adjustSliderStep / 100f;
        public void DecreaseBackline() => m_distanceSliders[0].value -= m_adjustSliderStep / 100f;
        public void IncreaseFrontline() => m_distanceSliders[1].value += m_adjustSliderStep / 100f;
        public void DecreaseFrontline() => m_distanceSliders[1].value -= m_adjustSliderStep / 100f;
        #endregion

        #region Listener Methods        
        #region Dropdown-OnValueChanged-Methods

        private void OnGameModeChanged(int _dropdownIndex)
        {
            m_matchData.EGameMode = (EGameMode)_dropdownIndex;
            switch (m_matchData.EGameMode)
            {
                case EGameMode.Normal:
                case EGameMode.SuddenDeath:
                    {
                        m_matchData.RoundsToWin = 1;
                        m_matchData.RoundPoints = 1;
                        break;
                    }
                case EGameMode.Infinite:
                    {
                        m_matchData.RoundsToWin = 0;
                        m_matchData.RoundPoints = 0;
                        break;
                    }                        
            }

            //Update UI, including '.interactable'-Settings.
            SetUIElements();
        }

        /// <summary>
        /// Listener-Method to set round-values, only while the corresponding dropdown is interactable.
        /// </summary>
        /// <param name="_toggle"></param>
        private void OnRoundDropdownChanged(int _dropdownIndex)
        {
            if (m_isNormalMode)
                m_matchData.RoundsToWin = _dropdownIndex;
        }

        /// <summary>
        /// Listener-Method to set maxPoint-values, only while the corresponding dropdown is interactable.
        /// </summary>
        /// <param name="_toggle"></param>
        private void OnMaxPointDropdownChanged(int _dropdownIndex)
        {
            if (m_isNormalMode)
                m_matchData.RoundPoints = _dropdownIndex;
        }

        private void OnObstacleToggleChanged(bool _isOn)
        {
            m_matchData.ObstaclesEnabled = _isOn;
        }

        private void OnWidthDropdownChanged(int _dropdownValue)
        {
            m_matchData.FieldWidth = _dropdownValue + m_minFieldWidth;
            if (m_matchData.FixFieldRatio)
            {
                m_matchData.FieldLength = m_matchData.FieldWidth * 2;
                SetUIElements();
            }
        }

        private void OnLengthDropdownChanged(int _dropdownValue)
        {
            m_matchData.FieldLength = _dropdownValue + m_minFieldLength;
            if (m_matchData.FixFieldRatio)
            {
                m_matchData.FieldWidth = m_matchData.FieldLength / 2;
                SetUIElements();
            }
        }

        private void OnPlayerAmountChanged(int _dropdownIndex)
        {
            m_matchData.PlayerCount = _dropdownIndex == 2 ? 4 : (_dropdownIndex == 1 ? 2 : 1);
            SetUIElements();
        }

        private void OnPlayerPositionChanged(int _teamIndex, EPlayerLine _line, int _dropdownValue)
        {
            //PlayerIndex from Team 1: 0 = Player1, 1 = Player3. PlayerIndex from Team 2: 0 = Player2, 1 = Player4.
            int selectedPlayerIndex = GetPlayerIndexFromUI(_teamIndex, _dropdownValue);
            if (selectedPlayerIndex < 0)
                return; //Error. No Index/player found.

            //Set the position of the selected player in PlayerPositions-Dict.
            m_matchData.PlayerPositions[selectedPlayerIndex] = _line;

            //Find the TeamMate.
            int teammateIndex = GetTeammateIndex(selectedPlayerIndex);
            if (teammateIndex < 0)
                return; //No TeamMate found. (Example. 1vs1 Mode)

            //Set position of the teamMate to the opposite _line.
            EPlayerLine oppositeLine = (_line == EPlayerLine.Backline) ? EPlayerLine.Frontline : EPlayerLine.Backline;
            m_matchData.PlayerPositions[teammateIndex] = oppositeLine;

            //Update the UI to show the position swap.
            SetUIElements();
        }

        /// <summary>
        /// Helper to translate UI indices (team + dropdown value) to a global player index (0-3).
        /// </summary>
        private int GetPlayerIndexFromUI(int teamIndex, int dropdownValue)
        {
            if (teamIndex == 0) //Team 1
            {
                return (dropdownValue == 0) ? 0 : 2; //Dropdown 0 = Player 1 (index 0), Dropdown 1 = Player 3 (index 2)
            }
            else //Team 2
            {
                return (dropdownValue == 0) ? 1 : 3; //Dropdown 0 = Player 2 (index 1), Dropdown 1 = Player 4 (index 3)
            }
        }

        #endregion

        #region Helper Methods
        /// <summary>
        /// Helper to find a player's teammate.
        /// </summary>
        private int GetTeammateIndex(int _playerIndex)
        {
            return _playerIndex switch
            {
                0 => 2, //Teammate of P1 is P3
                2 => 0,
                1 => 3, //Teammate of P2 is P4
                3 => 1,
                _ => -1
            };
        }
        #endregion

        #region Toggle-OnValueChanged-Methods
        /// <summary>
        /// Enabling this Toggle shall fix changes of width and length of the playfield to 1:2 ratio, while changing values on one of the two dropdowns.
        /// </summary>
        /// <param name="_toggle"></param>
        private void OnRatioToggleChanged(bool _isOn)
        {
            m_matchData.FixFieldRatio = _isOn;

            if (_isOn)
            {
                m_matchData.FieldLength = m_matchData.FieldWidth * 2;   //Length Dd value = width Dd value * 2.
                SetUIElements();                                        //Sync UI.
            }
        }

        /// <summary>
        /// Sets bool to automaticly reset playerRotationReset on Goals. Not to mix up with OnRatioToggleChanged!
        /// </summary>
        /// <param name="_isOn"></param>
        private void OnRotationToggleChanged(bool _isOn)
        {
            m_matchData.RotationReset = _isOn;
            m_rotationResetText.text = _isOn == true ? "On" : "Off";
        }
        #endregion

        #region Slider-OnValueChanged
        private void OnBacklineSliderChanged(float _sliderValue)
        {
            //Calculate difference
            float delta = _sliderValue - m_matchData.BacklineDistance;

            //Update backline-Slider
            m_matchData.BacklineDistance = _sliderValue;
            // Wende die Veränderung auf die Frontline an und klemme den Wert zwischen 0 und 1
            m_matchData.FrontlineDistance = Mathf.Clamp01(m_matchData.FrontlineDistance + delta);

            //Update the whole UI.
            SetUIElements();
        }

        private void OnFrontlineSliderChanged(float _sliderValue)
        {
            if (_sliderValue < m_matchData.BacklineDistance)
                _sliderValue = m_matchData.BacklineDistance;

            m_matchData.FrontlineDistance = _sliderValue;

            SetUIElements();
        }
        #endregion
        #endregion

        #region Custom-Methods
        private void SetUIElements()
        {
            RemoveListeners();

            m_gameModeDropdown.SetValueWithoutNotify((int)m_matchData.EGameMode);

            m_isNormalMode = m_matchData.EGameMode == EGameMode.Normal;
            m_roundsDropdown.interactable = m_isNormalMode;
            m_maxPointsDropdown.interactable = m_isNormalMode;

            SetupRoundsDropdown();
            SetupMaxPointsDropdown();

            SetupFieldDimension();

            SetupPlayerDetails();

            SetLineUpAndVisibility(m_matchData.PlayerCount);

            AddListeners();
        }

        #region Fill-Dropdowns-On-Start
        private void SetupRoundsDropdown()
        {
            m_roundsDropdown.ClearOptions();
            var roundsDdList = new List<string> { "\u221E" };

            for (int i = 1; i <= m_maxRounds; i++)
                roundsDdList.Add(i.ToString());

            m_roundsDropdown.AddOptions(roundsDdList);
            m_roundsDropdown.SetValueWithoutNotify(m_matchData.EGameMode == EGameMode.Infinite ? 0 : (m_matchData.EGameMode == EGameMode.SuddenDeath ? 1 : m_matchData.RoundsToWin));
        }

        private void SetupMaxPointsDropdown()
        {
            m_maxPointsDropdown.ClearOptions();
            var maxPointsDdList = new List<string> { "\u221E" };

            for (int i = 1; i <= m_maxPointsEachRound; i++)
                maxPointsDdList.Add(i.ToString());

            m_maxPointsDropdown.AddOptions(maxPointsDdList);
            m_maxPointsDropdown.SetValueWithoutNotify(m_matchData.EGameMode == EGameMode.Infinite ? 0 : (m_matchData.EGameMode == EGameMode.SuddenDeath ? 1 : m_matchData.RoundPoints));
        }

        private void SetupFieldDimension()
        {
            //m_fixRatioToggle.isOn = m_matchData.FixFieldRatio;
            m_fixRatioToggle.SetIsOnWithoutNotify(m_matchData.FixFieldRatio);

            m_fieldWidthDropdown.ClearOptions();
            var widthList = new List<string>();

            for (int i = m_minFieldWidth; i <= m_maxFieldWidth; i++)
                widthList.Add(i.ToString());

            m_fieldWidthDropdown.AddOptions(widthList);
            m_fieldWidthDropdown.SetValueWithoutNotify(m_matchData.FieldWidth - m_minFieldWidth);       //Width to DropdownIndex

            m_fieldLengthDropdown.ClearOptions();
            var lengthList = new List<string>();

            for (int i = m_minFieldLength; i <= m_maxFieldLength; i++)
                lengthList.Add(i.ToString());

            m_fieldLengthDropdown.AddOptions(lengthList);
            m_fieldLengthDropdown.SetValueWithoutNotify(m_matchData.FieldLength - m_minFieldLength);    //Length to DropdownIndex
        }

        private void SetupPlayerDetails()
        {
            //m_rotationReset.isOn = m_matchData.RotationReset;
            m_rotationReset.SetIsOnWithoutNotify(m_matchData.RotationReset);
            m_rotationResetText.text = m_matchData.RotationReset ? "On" : "Off";

            for (int i = 0; i < m_maxPlayers; i++)
            {
                //If 0 = 1P, 1 = 2P, 2 = 4P (While maxPlayer of 4 stays unchanged.)
                int playerCountIndex = m_matchData.PlayerCount == 4 ? 2 : (m_matchData.PlayerCount == 2 ? 1 : 0);
                m_playerAmountDd.SetValueWithoutNotify(playerCountIndex);
            }
        }

        /// <summary>
        /// Updates player line-up and UI-Visibility.
        /// </summary>
        /// <param name="_playerCount"></param>
        private void SetLineUpAndVisibility(int _playerCount)
        {
            m_distanceSliders[0].SetValueWithoutNotify(m_matchData.BacklineDistance);
            m_distanceSliders[1].SetValueWithoutNotify(m_matchData.FrontlineDistance);
            m_backFloatText.text = $"{m_matchData.BacklineDistance:N2}";
            m_frontFloatText.text = $"{m_matchData.FrontlineDistance:N2}";

            //Set Visibility
            bool isTwoPlayer = _playerCount == 2;                               //No 3 player allowed.
            bool isFourPlayer = _playerCount == 4;

            m_backLineDds[1].gameObject.SetActive(isTwoPlayer || isFourPlayer); //Team 2 Backline Dropdown
            m_frontLineSlot.gameObject.SetActive(isFourPlayer);                 //Whole Frontline Slot.
            m_teamText.gameObject.SetActive(_playerCount > 1);

            //Set Team's-Dropdown-Options
            var team1Options = new List<string> { m_player1, m_player3 };
            var team2Options = new List<string> { m_player2, m_player4 };

            m_backLineDds[0].ClearOptions();
            m_backLineDds[0].AddOptions(team1Options);
            //.splitDdValue = old .CameraMode.
            //if (_playerCount < 2)
            //    SettingsManager.Instance.CurrentSettings.Graphic.splitDdValue = (int)ECameraModi.SingleCam;

            if (isTwoPlayer)
            {
                m_backLineDds[1].ClearOptions();
                m_backLineDds[1].AddOptions(team2Options);

                //SettingsManager.Instance.CurrentSettings.Graphic.splitDdValue = (int)ECameraModi.Horizontal;
            }

            if (isFourPlayer)
            {
                m_backLineDds[1].ClearOptions();
                m_backLineDds[1].AddOptions(team2Options);
                m_frontLineDds[0].ClearOptions();
                m_frontLineDds[0].AddOptions(team1Options);
                m_frontLineDds[1].ClearOptions();
                m_frontLineDds[1].AddOptions(team2Options);

                SettingsManager.Instance.CurrentSettings.Graphic.splitDdValue = (int)ECameraModi.Quartet;
            }

            //Set Dropdown-Values based on Dictionary
            UpdatePlayerLineup();

            //TODO: Adapt UI-navigation, depending on _playerCount
            UpdateNavigation(_playerCount);
        }

        /// <summary>
        /// Reads the central data dictionary and sets all four dropdowns correctly.
        /// This ensures the UI is always in sync with the data state.
        /// </summary>
        private void UpdatePlayerLineup()
        {
            if (m_matchData.PlayerCount < 2)
                return;

            int team1BacklinePlayer = m_matchData.PlayerPositions[0] == EPlayerLine.Backline ? 0 : 2;
            m_backLineDds[0].SetValueWithoutNotify(team1BacklinePlayer == 0 ? 0 : 1);
            if (m_matchData.PlayerCount == 4)
                m_frontLineDds[0].SetValueWithoutNotify(team1BacklinePlayer == 0 ? 1 : 0);

            int team2BacklinePlayer = m_matchData.PlayerPositions[1] == EPlayerLine.Backline ? 1 : 3;
            m_backLineDds[1].SetValueWithoutNotify(team2BacklinePlayer == 1 ? 0 : 1);
            if (m_matchData.PlayerCount == 4)
                m_frontLineDds[1].SetValueWithoutNotify(team2BacklinePlayer == 1 ? 1 : 0);
        }

        private void UpdateNavigation(int _playerCount)
        {
            //While set element positions do NOT change!
            var backlineDdT1Nav = m_backlineSelectables[3].navigation;

            switch (_playerCount)
            {
                case 1:
                {
                    backlineDdT1Nav.selectOnRight = null;       //Team 1 Dropdown selectOnRight None.
                    for (int i = 0; i < m_backlineSelectables.Length; i++)
                    {
                        var navigation = m_backlineSelectables[i].navigation;
                        navigation.selectOnDown = m_btnContainer[2]; //Sets BackButton as target.
                        m_backlineSelectables[i].navigation = navigation;
                    }

                    //Default- & BackButton navigate to Team1-Dropdown
                    for (int i = 0; i < m_btnContainer.Length; i++)
                    {
                        m_btnContainer[i].TryGetComponent(out Button btnComponent);
                        if (btnComponent != null)
                        {
                            Navigation buttonNavigation = btnComponent.navigation;
                            buttonNavigation.selectOnUp = m_backlineSelectables[3];   //Team 1 Frontline Dropdown

                            btnComponent.navigation = buttonNavigation;
                        }
                    }
                    break;
                }
                case 2:
                {
                    backlineDdT1Nav.selectOnRight = m_backlineSelectables[4];  //Team 1 Dropdown selectOnRight to Team 2 Dropdown.
                    for (int i = 0; i < m_backlineSelectables.Length; i++)
                    {
                        var navigation = m_backlineSelectables[i].navigation;
                        navigation.selectOnDown = m_btnContainer[2];            //Sets BackButton as target.
                        m_backlineSelectables[i].navigation = navigation;
                    }

                    //Default- & BackButton navigate to Team1-Dropdown
                    for (int i = 0; i < m_btnContainer.Length; i++)
                    {
                        m_btnContainer[i].TryGetComponent(out Button btnComponent);
                        if (btnComponent != null)
                        {
                            Navigation buttonNavigation = btnComponent.navigation;
                            buttonNavigation.selectOnUp = m_backlineSelectables[3];   //Team 1 Frontline Dropdown

                            btnComponent.navigation = buttonNavigation;
                        }
                    }
                    break;
                }
                case 4:
                {
                    backlineDdT1Nav.selectOnRight = m_backlineSelectables[4];  //Team 1 Dropdown selectOnRight to Team 2 Dropdown.
                    //Connect Backline-Selectables with their counterParts in the FrontlineSlot.
                    for (int i = 0; i < m_backlineSelectables.Length; i++)
                    {
                        var navigation = m_backlineSelectables[i].navigation;
                        navigation.selectOnDown = m_frontlineSelectables[i];
                        m_backlineSelectables[i].navigation = navigation;
                    }

                    //Default- & BackButton navigate to Team1-Dropdown
                    for (int i = 0; i < m_btnContainer.Length; i++)
                    {
                        m_btnContainer[i].TryGetComponent(out Button btnComponent);
                        if (btnComponent != null)
                        {
                            Navigation buttonNavigation = btnComponent.navigation;
                            buttonNavigation.selectOnUp = m_frontlineSelectables[3];   //Team 2 Frontline Dropdown

                            btnComponent.navigation = buttonNavigation;
                        }
                    }
                    break;
                }
                default:
                    break;
            }
        }
        #endregion

        private int MaxAllowedLocalPlayer()
        {
            int connectMode = m_matchData.GameConnectMode; // Lese den aktuellen Modus aus den Daten

            return connectMode switch
            {
                //Offline
                0 => 4,
                //LAN
                1 => 2,
                // Internet
                2 => 2,
                _ => 1,
            };
        }

        public void ReSetDefault()
        {
            SettingsManager.Instance.ResetMatchSettings();
        }
        #endregion
    }
}