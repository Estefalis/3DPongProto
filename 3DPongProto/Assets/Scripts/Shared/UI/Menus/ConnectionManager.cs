using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ThreeDeePongProto.Shared.UI
{
    public class ConnectionManager : MonoBehaviour
    {
        [SerializeField] private Button[] m_modiButtons;

        #region Scriptable Objects
        [SerializeField] private MatchUIStates m_matchUIStates;
        #endregion

        public void SetGameModi(Button _sender)
        {
            if (_sender == m_modiButtons[0])
            {
                SetConnectionInfo(EGameModi.LocalPC);
            }

            if (_sender == m_modiButtons[1])
            {
                SetConnectionInfo(EGameModi.LAN);
            }

            if (_sender == m_modiButtons[2])
            {
                SetConnectionInfo(EGameModi.Internet);
            }
        }

        private void SetConnectionInfo(EGameModi _eGameModi)
        {
            m_matchUIStates.EGameConnectModi = _eGameModi;
#if UNITY_EDITOR
            Debug.Log($"Spiel-Modus: {_eGameModi}");
#endif
        }

        public void StartMatch()
        {
            switch (m_matchUIStates.EGameConnectModi)
            {
                case EGameModi.LocalPC:
                {
                    SceneManager.LoadScene((int)ESceneNames.LocalGame);
                    break;
                }
                case EGameModi.LAN:
                {
                    Debug.Log($"Implement the {ESceneNames.LanGame.ToString()}, once the local scene is completed!");
                    break;
                }
                case EGameModi.Internet:
                {
                    Debug.Log($"Implement the {ESceneNames.NetGame.ToString()}, once the local scene is completed!");
                    break;
                }
                default:
                {
                    break;
                }
            }
        }
    }
}