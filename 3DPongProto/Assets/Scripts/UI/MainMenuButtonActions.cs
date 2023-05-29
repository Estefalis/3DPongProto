using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace ThreeDeePongProto.Menu.Actions
{
    public class MainMenuButtonActions : MonoBehaviour
    {
        [SerializeField] private string m_loadNextScene;
        [SerializeField] private Button[] m_modiButtons;

        public void SetGameModi(Button _sender)
        {
            for (int i = 0; i < m_modiButtons.Length; i++)
            {
                if (_sender == m_modiButtons[0])
                {
                    GameManager.Instance.EGameModi = EGameModi.LocalPC;
                }
                else if (_sender == m_modiButtons[1])
                {
                    GameManager.Instance.EGameModi = EGameModi.LAN;
                }
                else if (_sender == m_modiButtons[2])
                {
                    GameManager.Instance.EGameModi = EGameModi.Internet;
                }
            }
#if UNITY_EDITOR
            Debug.Log("Meldung für Spiel-Modus: " + GameManager.Instance.EGameModi);
#endif
        }

        public void StartLocalGame()
        {
            SceneManager.LoadScene(m_loadNextScene);
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