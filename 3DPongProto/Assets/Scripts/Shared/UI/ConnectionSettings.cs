using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ThreeDeePongProto.Shared.UI
{
    public class ConnectionSettings : MonoBehaviour
    {
        #region SerializeField-Member-Variables
        [SerializeField] private string m_localGameScene = "LocalGameScene";
        [SerializeField] private string m_lanGameScene = "LanGameScene";
        [SerializeField] private string m_internetGameScene = "InternetGameScene";
        [SerializeField] private Button[] m_modiButtons;
        #endregion

        #region Scriptable Objects
        [SerializeField] private MatchUIStates m_matchUIStates;
        #endregion

        public void SetGameModi(Button _sender)
        {
            if (_sender == m_modiButtons[0])
            {
                m_matchUIStates.EGameConnectModi = EGameModi.LocalPC;
            }

            if (_sender == m_modiButtons[1])
            {
                m_matchUIStates.EGameConnectModi = EGameModi.LAN;
            }

            if (_sender == m_modiButtons[2])
            {
                m_matchUIStates.EGameConnectModi = EGameModi.Internet;
            }
#if UNITY_EDITOR
            Debug.Log("Meldung für Spiel-Modus: " + m_matchUIStates.EGameConnectModi);
#endif
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
                    SceneManager.LoadScene(m_lanGameScene);
                    break;
                }
                case EGameModi.Internet:
                {
                    SceneManager.LoadScene(m_internetGameScene);
                    break;
                }
                default:
                {
                    break;
                }
            }
        }

        //public void StartLanGame()
        //{
        //    SceneManager.LoadScene(m_lanGameScene);
        //}

        //public void StartInternetGame()
        //{
        //    SceneManager.LoadScene(m_internetGameScene);
        //}

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