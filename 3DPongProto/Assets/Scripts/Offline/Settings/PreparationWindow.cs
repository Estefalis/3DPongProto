using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Indices set equal to the desired amount of players participating in matches of int 2 and/or 4.
/// </summary>
public enum EPlayerAmount
{
    Two = 2,
    Four = 4
}

namespace ThreeDeePongProto.Offline.Settings
{
    public class PreparationWindow : MonoBehaviour
    {
        #region SerializeField-Member-Variables
        [SerializeField] private EPlayerAmount m_defaultPlayerAmount;

        [SerializeField] private TextMeshProUGUI m_playerTextOne;
        [SerializeField] private TextMeshProUGUI m_playerTextTwo;
        [SerializeField] private Transform m_playerThreeGroup;
        [SerializeField] private Transform m_playerFourGroup;
        [SerializeField] private Transform m_playerGroupTwo;
        [SerializeField] private TMP_Dropdown m_playerInGame;

        [SerializeField] private MatchUIStates m_matchUIStates;
        [SerializeField] private MatchValues m_matchValues;
        [SerializeField] private GraphicUiStates m_graphicUiStates;
        #endregion

        #region Scriptable-References
        [SerializeField] private PlayerIDData[] m_playerIDData;
        //[SerializeField] private GameObject[] m_playerPrefabs;
        #endregion

        private readonly uint m_minPlayerInGame = 2;
        private List<string> m_maxPlayerAmount;

        #region Serialization
        private readonly string m_settingStatesFolderPath = "/SaveData/Settings-States";
        private readonly string m_graphicFileName = "/Graphic";
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
            SetupMatchDropdowns();
        }

        private void AddGroupListener()
        {
            //PlayerAmount
            m_playerInGame.onValueChanged.AddListener(delegate
            { OnPlayerAmountChanged(m_playerInGame); });
        }

        private void RemoveGroupListener()
        {
            //PlayerAmount
            m_playerInGame.onValueChanged.RemoveListener(delegate
            { OnPlayerAmountChanged(m_playerInGame); });
        }

        private void SetupMatchDropdowns()
        {
            //PlayerAmount
            m_maxPlayerAmount = new();

            for (uint i = m_minPlayerInGame; i < m_matchValues.MaxPlayerInGame + m_minPlayerInGame; i++)
            {
                if (i % 2 == 0)
                    m_maxPlayerAmount.Add($"{i}");
            }

            m_playerInGame.ClearOptions();
            m_playerInGame.AddOptions(m_maxPlayerAmount);

            if (m_matchUIStates != null)
            {
                //Modifier to ensure that values EPlayerAmount.Two & EPlayerAmount:Four set the correct dropdownIndex.
                int dropdownValueModifier = (int)m_matchUIStates.EPlayerAmount / 2 - 1;
                m_playerInGame.value = dropdownValueModifier;
            }
            else
            {
                ResetDefault();
            }

            m_playerInGame.RefreshShownValue();

            OnPlayerAmountChanged(m_playerInGame);
        }

        private void OnPlayerAmountChanged(TMP_Dropdown _dropdown)
        {
            switch (_dropdown.value)
            {
                case 0:
                {
                    m_matchUIStates.EPlayerAmount = EPlayerAmount.Two;
                    m_graphicUiStates.SetCameraMode = ECameraModi.TwoHorizontal;
                    ObjectsToHide(false, false, true, 437.0f);
                    break;
                }
                case 1:
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
            m_persistentData.SaveData(m_settingStatesFolderPath, m_graphicFileName, m_fileFormat, m_graphicUiStates, m_encryptionEnabled, true);
            SetUpPlayerAmount(m_matchUIStates.EPlayerAmount);
        }

        private void SetUpPlayerAmount(EPlayerAmount _ePlayerAmount)
        {
            //Shall no reset the Lists, if the amount is equal.
            if (m_matchUIStates.EPlayerAmount == _ePlayerAmount)
                return;

            m_matchValues.PlayerData.Clear();
            m_matchValues.PlayerData = new();
            //m_matchValues.PlayerPrefabs.Clear();
            //m_matchValues.PlayerPrefabs = new();

            uint playerAmount = (uint)_ePlayerAmount;    //EPlayerAmount.Four => int 4 || EPlayerAmount.Two => int 2
            for (uint i = 0; i < playerAmount; i++)
            {
                m_matchValues.PlayerData.Add(m_playerIDData[(int)i]);
                //m_matchValues.PlayerPrefabs.Add(m_playerPrefabs[(int)i]);
            }
        }

        private void ObjectsToHide(bool _IFThree, bool _IFFour, bool _playerGroupTwo, float _textWidth)
        {
            m_playerThreeGroup.gameObject.SetActive(_IFThree);
            m_playerFourGroup.gameObject.SetActive(_IFFour);
            m_playerGroupTwo.gameObject.SetActive(_playerGroupTwo);
            m_playerTextOne.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _textWidth);
            m_playerTextTwo.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _textWidth);
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
            int dropdownValueModifier = (int)m_defaultPlayerAmount / 2 - 1;
            m_playerInGame.value = dropdownValueModifier;
        }
    }
}