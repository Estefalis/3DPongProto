using ThreeDeePongProto.Offline.Player.Inputs;
using ThreeDeePongProto.Offline.UI;
using UnityEngine;

//public enum EGameModi
//{
//    LocalPC,
//    LAN,
//    Internet
//}

//public enum ECameraModi
//{
//    SingleCam,
//    TwoHorizontal,
//    TwoVertical,
//    FourSplit
//}

namespace ThreeDeePongProto.Managers
{
    public class GameManager : GenericSingleton<GameManager>
    {
        //public ECameraModi ECameraMode { get => m_eCameraMode; set => m_eCameraMode = value; }
        //[SerializeField] private ECameraModi m_eCameraMode;

        //public EGameModi EGameConnectionModi { get => eGameConnectionMode; set => eGameConnectionMode = value; }
        //[SerializeField] private EGameModi eGameConnectionMode;

        //public bool GameIsPaused { get => m_gameIsPaused; private set { m_gameIsPaused = value; } }
        //[SerializeField] private bool m_gameIsPaused;

        //private void OnEnable()
        //{
        //    PlayerController.InGameMenuOpens += PauseAndTimeScale;
        //    MenuOrganisation.CloseInGameMenu += ResetPauseAndTimescale;
        //    MenuOrganisation.RestartGameLevel += GameRestartActions;
        //    MenuOrganisation.LoadMainScene += SceneRestartActions;
        //}

        //private void OnDisable()
        //{
        //    PlayerController.InGameMenuOpens -= PauseAndTimeScale;
        //    MenuOrganisation.CloseInGameMenu -= ResetPauseAndTimescale;
        //    MenuOrganisation.RestartGameLevel -= GameRestartActions;
        //    MenuOrganisation.LoadMainScene -= SceneRestartActions;
        //}

        //private void PauseAndTimeScale()
        //{
        //    Time.timeScale = 0f;
        //    GameIsPaused = true;
        //}

        //private void ResetPauseAndTimescale()
        //{
        //    Time.timeScale = 1f;
        //    GameIsPaused = false;
        //}

        //private void GameRestartActions()
        //{
        //    ResetPauseAndTimescale();
        //}

        //private void SceneRestartActions()
        //{
        //    ResetPauseAndTimescale();
        //}
    }
}