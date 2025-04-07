using ThreeDeePongProto.Shared.Managers;
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
                SetConnectionInfo(EGameConnectionModi.LocalPC);
            }

            if (_sender == m_modiButtons[1])
            {
                SetConnectionInfo(EGameConnectionModi.LAN);
            }

            if (_sender == m_modiButtons[2])
            {
                SetConnectionInfo(EGameConnectionModi.Internet);
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
            UserInputManager.ToggleActionMaps(EInputActionMaps.PlayerActions.ToString());

            switch (m_matchUIStates.EGameConnectModi)
            {
                case EGameConnectionModi.LocalPC:
                {
                    SceneManager.LoadScene((int)ESceneNames.LocalGame);
                    break;
                }
                case EGameConnectionModi.LAN:
                {
                    Debug.Log($"Implement the {ESceneNames.LanGame}, once the local scene is completed!");
                    break;
                }
                case EGameConnectionModi.Internet:
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