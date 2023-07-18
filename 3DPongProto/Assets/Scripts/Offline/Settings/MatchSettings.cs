using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ThreeDeePongProto.Offline.Settings
{
    public class MatchSettings : MonoBehaviour
    {
        [SerializeField] private Toggle m_unlimitRounds;
        [SerializeField] private TMP_Dropdown m_roundsDropdown;
        [SerializeField] private Toggle m_unlimitPoints;
        [SerializeField] private TMP_Dropdown m_maxPointsDropdown;

        [SerializeField] private int m_defaultRoundAmount;
        [SerializeField] private int m_maxRoundPoints;

        [SerializeField] private MatchVariables m_matchVariables;
        [SerializeField] private MatchVariables m_defaultMatchVariables;

        private void Awake()
        {
            FillRoundsDropdown();
            FillPointsDropdown();
        }

        private void FillRoundsDropdown()
        {
            m_roundsDropdown.ClearOptions();
            List<string> rounds = new List<string>();

            for (int i = 1; i < m_defaultRoundAmount + 1; i++)
            {
                rounds.Add(i.ToString());
            }

            m_roundsDropdown.AddOptions(rounds);
            //m_roundsDropdown.value = m_defaultRoundAmount;
            SetIntDropdownValues(m_roundsDropdown, m_defaultRoundAmount);
            m_roundsDropdown.RefreshShownValue();
        }

        private void FillPointsDropdown()
        {
            m_maxPointsDropdown.ClearOptions();
            List<string> maxPoints = new List<string>();

            for (int i = 1; i < m_maxRoundPoints + 1; i++)
            {
                //'m_maxPointsDropdown.options.Add (new Dropdown.OptionData() { text = variable });' in foreach-loops.
                maxPoints.Add(i.ToString());
            }

            m_maxPointsDropdown.AddOptions(maxPoints);
            //m_maxPointsDropdown.value = m_maxRoundPoints;
            SetIntDropdownValues(m_maxPointsDropdown, m_maxRoundPoints);
            m_maxPointsDropdown.RefreshShownValue();
        }

        private void SetIntDropdownValues(TMP_Dropdown _dropdown, int _value)
        {
            _dropdown.value = _value;
        }

        /// <summary>
        /// OnValueChanged inside Unity does not find uint! And dropdownIndices start from 0. So setable rounds are in fact '_roundIndex + 1'.
        /// </summary>
        /// <param name="_roundIndex"></param>
        public void SetRoundAmount(int _roundIndex)
        {
            m_roundsDropdown.value = _roundIndex;
            m_matchVariables.LastRoundIndex = _roundIndex;
        }

        /// <summary>
        /// OnValueChanged inside Unity does not find uint! And dropdownIndices start from 0. So setable maxPoints are in fact '_maxPointIndex + 1'.
        /// </summary>
        /// <param name="_maxPointIndex"></param>        
        public void SetMaxPoints(int _maxPointIndex)
        {
            m_maxPointsDropdown.value = _maxPointIndex;
            m_matchVariables.LastMaxPointIndex = _maxPointIndex;
        }
    }
}