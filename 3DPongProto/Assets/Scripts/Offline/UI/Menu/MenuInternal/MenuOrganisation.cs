using UnityEngine;
using UnityEngine.EventSystems;
using ThreeDeePongProto.Shared.InputActions;

namespace ThreeDeePongProto.Offline.UI.Menu
{
    public class MenuOrganisation : MonoBehaviour
    {
        [SerializeField] internal EventSystem m_eventSystem;
        [Header("Internal Connections")]
        [SerializeField] internal MenuNavigation m_menuNavigation;
        [SerializeField] internal InGameMenuActions m_inGameMenuActions;

        internal static GameObject LastSelectedGameObject { get => m_lastSelectedGameObject; }
        private static GameObject m_lastMenuSceneObject, m_lastGameSceneObject, m_lastSelectedGameObject;

        #region Scriptable Object
        [Header("Scriptable Objects")]
        [SerializeField] internal MatchUIStates m_matchUIStates;
        [SerializeField] internal MatchValues m_matchValues;
        [SerializeField] internal PlayerIDData[] m_playerIDData;
        #endregion

        private void Awake()
        {
            if (m_eventSystem == null)
                m_eventSystem = EventSystem.current;

            m_lastSelectedGameObject = null;
        }

        private void Start()
        {
            PreSetUpPlayerAmount(m_matchUIStates.EPlayerAmount);
        }

        private void Update()
        {
            UpdateLastSelectedObject();
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

        private void UpdateLastSelectedObject()
        {
            switch (EventSystem.current.currentSelectedGameObject == null)
            {
                case false:
                {
                    switch (m_lastSelectedGameObject != EventSystem.current.currentSelectedGameObject)
                    {
                        case false:
                            break;
                        case true:
                        {
                            m_lastSelectedGameObject = InputManager.m_PlayerInputActions.UI.enabled ? m_lastMenuSceneObject = EventSystem.current.currentSelectedGameObject : m_lastGameSceneObject = EventSystem.current.currentSelectedGameObject;
                            break;
                        }
                    }
                    break;
                }
                case true:
                {
                    switch (m_lastSelectedGameObject == null)
                    {
                        case false:
                            m_eventSystem.SetSelectedGameObject(m_lastSelectedGameObject);
                            break;
                        case true:
                            break;
                    }
                    break;
                }
            }
#if UNITY_EDITOR
            //Debug.Log(m_lastSelectedGameObject);
#endif
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