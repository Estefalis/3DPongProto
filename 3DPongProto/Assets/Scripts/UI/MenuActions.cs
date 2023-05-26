using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ThreeDeePongProto.Menu.Actions
{
    public class MenuActions : MonoBehaviour
    {
        [SerializeField] private string m_loadNextScene;

        public void StartLocalGame()
        {
            GameManager.Instance.EGameModi = EGameModi.LocalPC;

            if (GameManager.Instance.EGameModi == EGameModi.LocalPC)
                SceneManager.LoadScene(m_loadNextScene);

#if UNITY_EDITOR
            Debug.Log("Meldung für Spiel-Modus: " + GameManager.Instance.EGameModi);
#endif
        }

        public void StartLanGame()
        {
            GameManager.Instance.EGameModi = EGameModi.LAN;
#if UNITY_EDITOR
            Debug.Log("Meldung für Spiel-Modus: " + GameManager.Instance.EGameModi);
#endif
        }

        public void StartInternetGame()
        {
            GameManager.Instance.EGameModi = EGameModi.Internet;
#if UNITY_EDITOR
            Debug.Log("Meldung für Spiel-Modus: " + GameManager.Instance.EGameModi);
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