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

        [SerializeField] private uint m_rounds = 5;
        [SerializeField] private uint m_maxRoundPoints = 25;

        [SerializeField] private MatchVariables m_matchVariables;

        private void Awake()
        {
            FillRoundsDropdown();
            FillPointsDropdown();
        }

        private void FillRoundsDropdown()
        {
            m_roundsDropdown.ClearOptions();
            List<string> rounds = new List<string>();

            for (uint i = 1; i < m_rounds + 1; i++)
            {
                rounds.Add(i.ToString());
            }

            m_roundsDropdown.AddOptions(rounds);
            m_roundsDropdown.value = (int)m_rounds;
            m_roundsDropdown.RefreshShownValue();
        }

        private void FillPointsDropdown()
        {
            m_maxPointsDropdown.ClearOptions();
            List<string> maxPoints = new List<string>();

            for (uint i = 1; i < m_maxRoundPoints + 1; i++)
            {
                //'m_maxPointsDropdown.options.Add (new Dropdown.OptionData() { text = variable });' in foreach loops.
                maxPoints.Add(i.ToString());
            }

            m_maxPointsDropdown.AddOptions(maxPoints);
            m_maxPointsDropdown.value = (int)m_maxRoundPoints;
            m_maxPointsDropdown.RefreshShownValue();
        }

        /// <summary>
        /// OnValueChanged inside Unity does not find uint! And dropdownIndices start from 0. So setable rounds are in fact '_roundIndex + 1'.
        /// </summary>
        /// <param name="_roundIndex"></param>
        public void SetRoundAmount(int _roundIndex)
        {
            m_roundsDropdown.value = _roundIndex;
            m_matchVariables.LastRoundIndex = (uint)m_roundsDropdown.value;
        }

        /// <summary>
        /// OnValueChanged inside Unity does not find uint! And dropdownIndices start from 0. So setable maxPoints are in fact '_maxPointIndex + 1'.
        /// </summary>
        /// <param name="_maxPointIndex"></param>        
        public void SetMaxPoints(int _maxPointIndex)
        {
            m_maxPointsDropdown.value = _maxPointIndex;
            m_matchVariables.LastMaxPointIndex = (uint)m_maxPointsDropdown.value;
        }
    }
}