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
        [SerializeField] private PlayerIDData[] m_playerIDData;
        [SerializeField] private GameObject[] m_playerPrefabs;
        [SerializeField] private MatchValues m_matchValues;

        [SerializeField] private uint m_maxPlayersAmount = 4;

        public void SetGameModi(Button _sender)
        {
            m_matchValues.PlayerDataInGame.Clear();
            m_matchValues.PlayerDataInGame = new();
            m_matchValues.PlayersInGame.Clear();
            m_matchValues.PlayersInGame = new();

            if (_sender == m_modiButtons[0])
            {
                m_matchValues.EGameConnectionModi = EGameModi.LocalPC;
                SetMaxPlayerAmount(m_maxPlayersAmount /*- 2*/);
            }
            else if (_sender == m_modiButtons[1])
            {
                m_matchValues.EGameConnectionModi = EGameModi.LAN;
                SetMaxPlayerAmount(m_maxPlayersAmount);
            }
            else if (_sender == m_modiButtons[2])
            {
                m_matchValues.EGameConnectionModi = EGameModi.Internet;
                SetMaxPlayerAmount(m_maxPlayersAmount);
            }
#if UNITY_EDITOR
            Debug.Log("Meldung für Spiel-Modus: " + m_matchValues.EGameConnectionModi);
#endif
        }

        private void SetMaxPlayerAmount(uint _maxPlayersAmount)
        {
            for (int i = 0; i < _maxPlayersAmount; i++)
            {
                m_matchValues.PlayerDataInGame.Add(m_playerIDData[i]);
                m_matchValues.PlayersInGame.Add(m_playerPrefabs[i]);
            }
                        
            m_matchValues.PlayerInGame = _maxPlayersAmount;
            m_matchValues.MaxPlayerInGame = m_maxPlayersAmount;
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