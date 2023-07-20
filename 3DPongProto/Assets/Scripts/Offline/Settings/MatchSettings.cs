using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ThreeDeePongProto.Offline.Settings
{
    public class MatchSettings : MonoBehaviour
    {
        //[SerializeField] private Toggle m_unlimitedRounds;
        //[SerializeField] private TMP_Dropdown m_roundsDropdown;
        //[SerializeField] private Toggle m_unlimitedPoints;
        //[SerializeField] private TMP_Dropdown m_maxPointsDropdown;

        [SerializeField] private List<Toggle> m_unlimitToggleKeys = new List<Toggle>();
        [SerializeField] private List<TMP_Dropdown> m_roundValueDropdowns = new List<TMP_Dropdown>();
        private Dictionary<Toggle, TMP_Dropdown> m_matchKeyValuePairs = new Dictionary<Toggle, TMP_Dropdown>();

        [SerializeField] private int m_defaultRoundAmount;
        [SerializeField] private int m_maxRoundPoints;

        [SerializeField] private bool m_infiniteRounds;
        [SerializeField] private bool m_infinitePoints;

        [SerializeField] private MatchVariables m_matchVariables;
        [SerializeField] private MatchVariables m_defaultMatchVariables;

        List<string> m_rounds/* = new List<string>()*/;
        List<string> m_maxPoints/* = new List<string>()*/;

        private readonly int m_infiniteValue = int.MaxValue;

        private void Awake()
        {
            for (int i = 0; i < m_unlimitToggleKeys.Count; i++)
            {
                m_matchKeyValuePairs.Add(m_unlimitToggleKeys[i], m_roundValueDropdowns[i]);
            }

            //m_unlimitedRounds.isOn = m_infiniteRounds;
            m_unlimitToggleKeys[0].isOn = m_infiniteRounds;
            //m_unlimitedPoints.isOn = m_infinitePoints;
            m_unlimitToggleKeys[1].isOn = m_infinitePoints;
            //TODO: Implement code to set 'm_infiniteValue = int.MaxValue', if the corresponding toggles are on.

            //FillDropdowns(m_unlimitedRounds);
            FillDropdowns(m_unlimitToggleKeys[0]);
            //FillDropdowns(m_unlimitedPoints);
            FillDropdowns(m_unlimitToggleKeys[1]);
        }

        private void OnEnable()
        {
            AddGroupListeners();
        }

        private void OnDisable()
        {
            RemoveGroupListeners();
        }

        private void AddGroupListeners()
        {
            m_unlimitToggleKeys[0].onValueChanged.AddListener(delegate
            { RoundToggleValueChanged(m_unlimitToggleKeys[0]); });
            m_unlimitToggleKeys[1].onValueChanged.AddListener(delegate
            { MaxPointToggleValueChanged(m_unlimitToggleKeys[1]); });

            m_roundValueDropdowns[0].onValueChanged.AddListener(delegate
            { RoundDropdownValueChanged(m_unlimitToggleKeys[0]); });
            m_roundValueDropdowns[1].onValueChanged.AddListener(delegate
            { MaxPointDropdownValueChanged(m_unlimitToggleKeys[1]); });
        }

        private void RemoveGroupListeners()
        {
            m_unlimitToggleKeys[0].onValueChanged.RemoveAllListeners();
            m_unlimitToggleKeys[1].onValueChanged.RemoveAllListeners();
            m_roundValueDropdowns[0].onValueChanged.RemoveAllListeners();
            m_roundValueDropdowns[1].onValueChanged.RemoveAllListeners();
        }

        #region Toggle-Listeners
        private void RoundToggleValueChanged(Toggle _toggle)
        {
            m_rounds = new List<string>();

            TMP_Dropdown dropdown = m_matchKeyValuePairs[_toggle];
            dropdown.interactable = !dropdown.interactable;
            dropdown.ClearOptions();

            switch (_toggle.isOn)
            {
                case false:
                {
                    for (int i = 1; i < m_defaultRoundAmount + 1; i++)
                    {
                        m_rounds.Add(i.ToString());
                    }

                    dropdown.AddOptions(m_rounds);
                    dropdown.RefreshShownValue();
                    dropdown.interactable = true;
                    SetIntDropdownValues(dropdown, m_matchVariables.LastRoundIndex);

                    break;
                }
                case true:
                {
                    m_rounds.Add("(-.-)");
                    dropdown.AddOptions(m_rounds);
                    dropdown.RefreshShownValue();
                    dropdown.interactable = false;
                    break;
                }
            }
        }

        private void MaxPointToggleValueChanged(Toggle _toggle)
        {
            m_maxPoints = new List<string>();

            TMP_Dropdown dropdown = m_matchKeyValuePairs[_toggle];
            dropdown.interactable = !dropdown.interactable;
            dropdown.ClearOptions();

            switch (_toggle.isOn)
            {
                case false:
                {
                    for (int i = 1; i < m_maxRoundPoints + 1; i++)
                    {
                        //'m_maxPointsDropdown.options.Add (new Dropdown.OptionData() { text = variable });' in foreach-loops.
                        m_maxPoints.Add(i.ToString());
                    }

                    dropdown.AddOptions(m_maxPoints);
                    SetIntDropdownValues(dropdown, m_matchVariables.LastMaxPointIndex);
                    dropdown.RefreshShownValue();
                    break;
                }
                case true:
                {
                    m_maxPoints.Add("(-.-)");
                    dropdown.AddOptions(m_maxPoints);
                    dropdown.RefreshShownValue();
                    break;
                }
            }
        }
        #endregion

        private void RoundDropdownValueChanged(Toggle _toggle)
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
                    SetIntDropdownValues(dropdown, dropdown.value);
                    m_matchVariables.LastRoundIndex = dropdown.value;
                    break;
                }
            }
        }

        private void MaxPointDropdownValueChanged(Toggle _toggle)
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
                    SetIntDropdownValues(dropdown, dropdown.value);
                    m_matchVariables.LastMaxPointIndex = dropdown.value;
                    break;
                }
            }
        }

        private void FillDropdowns(Toggle _toggle)
        {
            TMP_Dropdown dropdown = m_matchKeyValuePairs[_toggle];
            dropdown.ClearOptions();

            m_rounds = new List<string>();
            m_maxPoints = new List<string>();

            switch (_toggle.isOn)
            {
                case false:
                {
                    if (dropdown == m_roundValueDropdowns[0])
                    {
                        for (int i = 1; i < m_defaultRoundAmount + 1; i++)
                        {
                            m_rounds.Add(i.ToString());
                        }

                        dropdown.AddOptions(m_rounds);
                        SetIntDropdownValues(dropdown, m_matchVariables.LastRoundIndex);
                        dropdown.RefreshShownValue();
                        dropdown.interactable = true;
                    }

                    if (dropdown == m_roundValueDropdowns[1])
                    {
                        for (int i = 1; i < m_maxRoundPoints + 1; i++)
                        {
                            //'m_maxPointsDropdown.options.Add (new Dropdown.OptionData() { text = variable });' in foreach-loops.
                            m_maxPoints.Add(i.ToString());
                        }

                        dropdown.AddOptions(m_maxPoints);
                        SetIntDropdownValues(dropdown, m_matchVariables.LastMaxPointIndex);
                        dropdown.RefreshShownValue();
                        dropdown.interactable = true;
                    }

                    break;
                }                
                case true:
                {
                    if (dropdown == m_roundValueDropdowns[0])
                    {
                        m_rounds.Add("(-.-)");
                        dropdown.AddOptions(m_rounds);
                        dropdown.RefreshShownValue();
                        dropdown.interactable = false;
                    }

                    if (dropdown == m_roundValueDropdowns[1])
                    {
                        m_maxPoints.Add("(-.-)");
                        dropdown.AddOptions(m_maxPoints);
                        dropdown.RefreshShownValue();
                        dropdown.interactable = false;
                    }

                    break;
                }
            }
        }

        private void SetIntDropdownValues(TMP_Dropdown _dropdown, int _value)
        {
            _dropdown.value = _value;
        }
    }
}