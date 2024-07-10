//using System;     //Used by Actions.
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ThreeDeePongProto.Shared.UI
{
    public class ConnectionManager : MonoBehaviour
    {
        #region SerializeField-Member-Variables
        [SerializeField] private string m_localGameScene = "LocalGameScene";
        [SerializeField] private string m_lanGameScene = "LanGameScene";
        [SerializeField] private string m_internetGameScene = "InternetGameScene";
        [SerializeField] private Button[] m_modiButtons;

        //[SerializeField] private uint m_maxPlayers = 4;
        #endregion

        //public static event Action LocalGameAnnounced;
        //public static event Action LANGameAnnounced;
        //public static event Action InternetGameAnnounced;

        #region Scriptable Objects
        [SerializeField] private MatchUIStates m_matchUIStates;
        #endregion

        public void SetGameModi(Button _sender)
        {
            if (_sender == m_modiButtons[0])
            {
                SetConnectionInfo(EGameModi.LocalPC/*, LocalGameAnnounced*/);
            }

            if (_sender == m_modiButtons[1])
            {
                SetConnectionInfo(EGameModi.LAN/*, LANGameAnnounced*/);
            }

            if (_sender == m_modiButtons[2])
            {
                SetConnectionInfo(EGameModi.Internet/*, InternetGameAnnounced*/);
            }
#if UNITY_EDITOR
            Debug.Log("Meldung für Spiel-Modus: " + m_matchUIStates.EGameConnectModi);
#endif
        }

        private void SetConnectionInfo(EGameModi _eGameModi/*, Action _action*/)
        {
            m_matchUIStates.EGameConnectModi = _eGameModi;
            //_action?.Invoke();
        }

        public void StartMatch()
        {
            switch (m_matchUIStates.EGameConnectModi)
            {
                case EGameModi.LocalPC:
                {
                    SceneManager.LoadScene(m_localGameScene);
                    break;
                }
                case EGameModi.LAN:
                {
                    Debug.Log($"Implement the {m_lanGameScene}, once the local scene is completed!");
                    //SceneManager.LoadScene(m_lanGameScene);
                    break;
                }
                case EGameModi.Internet:
                {
                    Debug.Log($"Implement the {m_internetGameScene}, once the local scene is completed!");
                    //SceneManager.LoadScene(m_internetGameScene);
                    break;
                }
                default:
                {
                    break;
                }
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