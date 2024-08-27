using UnityEngine;
using UnityEngine.EventSystems;

namespace ThreeDeePongProto.Offline.UI.Menu
{
    public class MenuOrganisation : MonoBehaviour
    {
        [SerializeField] internal EventSystem m_eventSystem;
        [Header("Internal Connections")]
        [SerializeField] internal MenuNavigation m_menuNavigation;
        [SerializeField] internal InGameMenuActions m_inGameMenuActions;
        [SerializeField] internal AutoScrollView m_autoScrollView;

        [Header("Select First Elements")]
        [SerializeField] internal Transform m_firstElement;
        internal Transform m_lastSelectedTransform;

        #region Scriptable Object
        [Header("Scriptable Objects")]
        [SerializeField] internal MatchUIStates m_matchUIStates;
        [SerializeField] internal MatchValues m_matchValues;
        [SerializeField] internal PlayerIDData[] m_playerIDData;
        #endregion

        private void Start()
        {
            PreSetUpPlayerAmount(m_matchUIStates.EPlayerAmount);
        }

        /// <summary>
        /// Required, so MatchSettings right at start can fill the Front-/Backline-Dropdowns.
        /// </summary>
        /// <param name="_ePlayerAmount"></param>
        private void PreSetUpPlayerAmount(EPlayerAmount _ePlayerAmount)
        {
            if (m_matchUIStates.GameRuns)
                return;

            m_matchValues.PlayerData.Clear();
            m_matchValues.PlayerData = new();

            uint playerAmount = (uint)_ePlayerAmount;    //EPlayerAmount.Four => int 4 || EPlayerAmount.Two => int 2
            for (uint i = 0; i < playerAmount; i++)
            {
                m_matchValues.PlayerData.Add(m_playerIDData[(int)i]);
            }
        }

        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}