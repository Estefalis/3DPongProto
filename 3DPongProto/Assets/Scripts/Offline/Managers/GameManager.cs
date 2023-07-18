using UnityEngine;

public enum EGameModi
{
    LocalPC,
    LAN,
    Internet
}

public enum ECameraModi
{
    SingleCam,
    TwoHorizontal,
    TwoVertical,
    FourSplit
}

namespace ThreeDeePongProto.Managers
{
    public class GameManager : GenericSingleton<GameManager>
    {
        public ECameraModi ECameraMode { get => m_eCameraMode; set => m_eCameraMode = value; }
        [SerializeField] private ECameraModi m_eCameraMode;

        public EGameModi EGameConnectionModi { get => eGameConnectionMode; set => eGameConnectionMode = value; }
        [SerializeField] private EGameModi eGameConnectionMode;

        public bool GameIsPaused { get => m_gameIsPaused; set { m_gameIsPaused = value; } }
        [SerializeField] private bool m_gameIsPaused;
    }
}