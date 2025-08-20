using ThreeDeePongProto.Shared.Managers;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ThreeDeePongProto.Shared.UI
{
    public class ConnectionManager : MonoBehaviour
    {
        [SerializeField] private Button[] m_modeButtons;

        public void SetGameModi(Button _sender)
        {
            if (_sender == m_modeButtons[0])
            {
                SetConnectionInfo((int)EGameConnectionModi.LocalGame);
            }

            if (_sender == m_modeButtons[1])
            {
                SetConnectionInfo((int)EGameConnectionModi.LanGame);
            }

            if (_sender == m_modeButtons[2])
            {
                SetConnectionInfo((int)EGameConnectionModi.NetGame);
            }
        }

        private void SetConnectionInfo(int _eGameModi)
        {
            SettingsManager.Instance.CurrentSettings.Match.GameConnectMode = _eGameModi;
#if UNITY_EDITOR
            //Debug.Log($"Spiel-Modus: {_eGameModi}");
#endif
        }

        public void StartMatch()
        {
            var gameConnectMode = (EGameConnectionModi)SettingsManager.Instance.CurrentSettings.Match.GameConnectMode;
            switch (gameConnectMode)
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