using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Indices set equal to the desired amount of players participating in matches of int 2 and/or 4.
/// </summary>
public enum EPlayerAmount
{
    One = 1,
    Two = 2,
    Four = 4
}

namespace ThreeDeePongProto.Offline.Settings
{
    public class PreparationWindow : MonoBehaviour
    {
        #region SerializeField-Member-Variables
        [SerializeField] private Transform m_playerTwoGroup;
        [SerializeField] private TMP_Dropdown m_playerAmountDd;
        [SerializeField] private EPlayerAmount m_defaultPlayerAmount;

        [Header("Textfields")]
        [SerializeField] private TextMeshProUGUI m_playerTextOne;
        [SerializeField] private TextMeshProUGUI m_playerTextTwo;

        [Header("Inputfield-Group")]
        [SerializeField] private Transform m_playerThreeIFGroup;
        [SerializeField] private Transform m_playerFourIFGroup;

        #region Scriptable-References
        [Header("Scriptable Objects")]
        [SerializeField] private MatchUIStates m_matchUIStates;
        [SerializeField] private MatchValues m_matchValues;
        [SerializeField] private GraphicUiStates m_graphicUiStates;
        [SerializeField] private PlayerIDData[] m_playerIDData;
        #endregion
        #endregion

        private const uint m_MINPLAYER = 1;
        private List<string> m_maxPlayerAmount;

        #region Serialization
        private readonly string m_settingsStatesFolderPath = "/SaveData/Settings-States";
        private readonly string m_graphicFileName = "/Graphic";
        private readonly string m_matchFileName = "/Match";
        private readonly string m_fileFormat = ".json";

        private IPersistentData m_persistentData = new SerializingData();
        private bool m_encryptionEnabled = false;
        #endregion

        private void OnEnable()
        {
            AddGroupListener();
        }

        private void OnDisable()
        {
            RemoveGroupListener();
        }

        private void Start()
        {
            SetupWindow(m_matchUIStates.EGameConnectModi, m_matchUIStates.EPlayerAmount);
            SetupMatchDropdowns();
        }

        private void AddGroupListener()
        {
            //PlayerAmount
            m_playerAmountDd.onValueChanged.AddListener(delegate
            { OnPlayerAmountChanged(m_playerAmountDd); });
        }

        private void RemoveGroupListener()
        {
            //PlayerAmount
            m_playerAmountDd.onValueChanged.RemoveListener(delegate
            { OnPlayerAmountChanged(m_playerAmountDd); });
        }

        #region Start Setup
        private void SetupWindow(EGameModi _connectMode, EPlayerAmount _ePlayerAmount)
        {
            switch (_connectMode)
            {
                case EGameModi.LocalPC:
                {
                    switch (_ePlayerAmount)
                    {
                        //TODO: PlayerAmount 1 vs NPC in Offline mode?
                        case EPlayerAmount.One:
                            ObjectsToHide(false, false, false, 437.0f);
                            break;
                        case EPlayerAmount.Two:
                        {
                            //Player 3 invisible, Player 4 invisible, Group 2 visible, TextWidths large.
                            ObjectsToHide(false, false, true, 437.0f);
                            break;
                        }
                        case EPlayerAmount.Four:
                        {
                            //Player 3 visible, Player 4 visible, Group 2 visible, TextWidths small.
                            ObjectsToHide(true, true, true, 110.0f);
                            break;
                        }
                        default:
                            ObjectsToHide(false, false, true, 437.0f);
                            break;
                    }
                    break;
                }
                case EGameModi.LAN:
                {
                    switch (_ePlayerAmount)
                    {
                        case EPlayerAmount.One:
                        {
                            //Only Player 1 visible for Lan 1 vs 1 Matches.
                            ObjectsToHide(false, false, false, 437.0f);
                            break;
                        }
                        case EPlayerAmount.Two:
                        {
                            //Player 3 invisible, Player 4 invisible, Group 2 visible, TextWidths large.
                            ObjectsToHide(false, false, true, 437.0f);
                            break;
                        }
                        //No 'EPlayerAmount.4', since the max playerAmount shall be 2 x 2 = 4.
                        default:
                            ObjectsToHide(false, false, false, 437.0f);
                            break;
                    }
                    break;
                }
                case EGameModi.Internet:
                {
                    switch (_ePlayerAmount)
                    {
                        case EPlayerAmount.One:
                        {
                            //Only Player 1 visible for Lan 1 vs 1 Matches.
                            ObjectsToHide(false, false, false, 437.0f);
                            break;
                        }
                        case EPlayerAmount.Two:
                        {
                            //Player 3 invisible, Player 4 invisible, Group 2 visible, TextWidths large.
                            ObjectsToHide(false, false, true, 437.0f);
                            break;
                        }
                        //No 'EPlayerAmount.4', since the max playerAmount shall be 2 x 2 = 4.
                        default:
                            ObjectsToHide(false, false, false, 437.0f);
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

        private void ObjectsToHide(bool _IFThree, bool _IFFour, bool _playerGroupTwo, float _textWidth)
        {
            m_playerThreeIFGroup.gameObject.SetActive(_IFThree);
            m_playerFourIFGroup.gameObject.SetActive(_IFFour);
            m_playerTwoGroup.gameObject.SetActive(_playerGroupTwo);
            m_playerTextOne.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _textWidth);
            m_playerTextTwo.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _textWidth);
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
                    ObjectsToHide(false, false, false, 437.0f);
                    break;
                }
                case 1:
                {
                    m_matchUIStates.EPlayerAmount = EPlayerAmount.Two;
                    m_graphicUiStates.SetCameraMode = ECameraModi.TwoHorizontal;
                    ObjectsToHide(false, false, true, 437.0f);
                    break;
                }
                case 2:
                {
                    m_matchUIStates.EPlayerAmount = EPlayerAmount.Four;
                    m_graphicUiStates.SetCameraMode = ECameraModi.FourSplit;
                    ObjectsToHide(true, true, true, 110.0f);
                    break;
                }
                default:
                    break;
            }

            //WHENEVER YOU GOT THE SAME CLASS IN MULTIPLE SCENES (like MENUORGANISATION) SAVE CHANGED DATA!!! OR old RELOADED DATA WILL OVERWRITE IT!!! AND YOU DON'T KNOW WHY...!
            m_persistentData.SaveData(m_settingsStatesFolderPath, m_matchFileName, m_fileFormat, m_matchUIStates, m_encryptionEnabled, true);
            m_persistentData.SaveData(m_settingsStatesFolderPath, m_graphicFileName, m_fileFormat, m_graphicUiStates, m_encryptionEnabled, true);
            SetUpPlayerAmount(m_matchUIStates.EPlayerAmount);
        }
        #endregion

        private void SetUpPlayerAmount(EPlayerAmount _ePlayerAmount)
        {
            m_matchValues.PlayerData.Clear();
            m_matchValues.PlayerData = new();

            uint playerAmount = (uint)_ePlayerAmount;    //EPlayerAmount.Four => int 4 || EPlayerAmount.Two => int 2
            for (uint i = 0; i < playerAmount; i++)
            {
                m_matchValues.PlayerData.Add(m_playerIDData[(int)i]);
            }
        }

        #region Name-Inputfields
        public void PlayerOneInput(string _playername)  //TODO: Optional Random a playername, or set Player 1-4.
        {
            m_matchValues.PlayerData[0].PlayerId = 0;

            if (string.IsNullOrWhiteSpace(_playername))
                return;
            m_matchValues.PlayerData[0].PlayerName = _playername;
        }

        public void PlayerTwoInput(string _playername)
        {
            m_matchValues.PlayerData[1].PlayerId = 1;

            if (string.IsNullOrWhiteSpace(_playername))
                return;
            m_matchValues.PlayerData[1].PlayerName = _playername;
        }

        public void PlayerThreeInput(string _playername)
        {
            m_matchValues.PlayerData[2].PlayerId = 2;

            if (string.IsNullOrWhiteSpace(_playername))
                return;
            m_matchValues.PlayerData[2].PlayerName = _playername;
        }

        public void PlayerFourInput(string _playername)
        {
            m_matchValues.PlayerData[3].PlayerId = 3;

            if (string.IsNullOrWhiteSpace(_playername))
                return;
            m_matchValues.PlayerData[3].PlayerName = _playername;
        }
        #endregion

        private void ResetDefault()
        {
            //Modifier to ensure that values EPlayerAmount.Two & EPlayerAmount:Four set the correct dropdownIndex.
            int dropdownValueModifier = (int)m_defaultPlayerAmount / 2 /*- 1*/;
            m_playerAmountDd.value = dropdownValueModifier;
        }
    }
}