using ThreeDeePongProto.Shared.Managers;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ThreeDeePongProto.Shared.UI
{
    public class ConnectionManager : MonoBehaviour
    {
        [SerializeField] private Button[] m_modeButtons;

        #region Scriptable Objects
        [SerializeField] private MatchUIStates m_matchUIStates;
        #endregion

        public void SetGameModi(Button _sender)
        {
            if (_sender == m_modeButtons[0])
            {
                SetConnectionInfo(EGameConnectionModi.LocalGame);
            }

            if (_sender == m_modeButtons[1])
            {
                SetConnectionInfo(EGameConnectionModi.LanGame);
            }

            if (_sender == m_modeButtons[2])
            {
                SetConnectionInfo(EGameConnectionModi.NetGame);
            }
        }

        private void SetConnectionInfo(EGameConnectionModi _eGameModi)
        {
            m_matchUIStates.EGameConnectModi = _eGameModi;
#if UNITY_EDITOR
            //Debug.Log($"Spiel-Modus: {_eGameModi}");
#endif
        }

        public void StartMatch()
        {
            switch (m_matchUIStates.EGameConnectModi)
            {
                case EGameConnectionModi.LocalGame:
                {
                    SceneManager.LoadScene((int)ESceneNames.LocalGame);
                    break;
                }
                case EGameConnectionModi.LanGame:
                {
                    Debug.Log($"Implement the {ESceneNames.LanGame}, once the local scene is completed!");
                    break;
                }
                case EGameConnectionModi.NetGame:
                {
                    Debug.Log($"Implement the {ESceneNames.NetGame}, once the local scene is completed!");
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