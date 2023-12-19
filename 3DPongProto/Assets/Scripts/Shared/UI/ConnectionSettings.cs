using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ThreeDeePongProto.Shared.UI
{
    public class ConnectionSettings : MonoBehaviour
    {
        [SerializeField] private string m_localGameScene = "LocalGameScene";
        //[SerializeField] private string m_lanScene = "LanScene";
        //[SerializeField] private string m_internetScene = "InternetScene";
        [SerializeField] private Button[] m_modiButtons;        
        //[SerializeField] private MatchValues m_matchValues;
        [SerializeField] private MatchUIStates m_matchUIStates;

        public void SetGameModi(Button _sender)
        {
            //m_matchValues.PlayerData.Clear();
            //m_matchValues.PlayerData = new();
            //m_matchValues.PlayerPrefabs.Clear();
            //m_matchValues.PlayerPrefabs = new();

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

        public void StartLocalGame()
        {
            SceneManager.LoadScene(m_localGameScene);
        }

        public void StartLanGame()
        {
#if UNITY_EDITOR
            Debug.Log("Meldung für Spiel-Modus: TBA");
#endif
        }

        public void StartInternetGame()
        {
#if UNITY_EDITOR
            Debug.Log("Meldung für Spiel-Modus: TBA");
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