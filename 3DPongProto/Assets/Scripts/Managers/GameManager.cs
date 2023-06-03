using UnityEngine;

public enum EGameModi
{
    LocalPC,
    LAN,
    Internet
}

public enum ECameraModi
{
    TwoHorizontal = default,
    TwoVertical,
    FourSplit
}

public class GameManager : GenericSingleton<GameManager>
{
    public EGameModi EGameConnectionModi { get => eGameConnectionMode; set => eGameConnectionMode = value; }
    [SerializeField] private EGameModi eGameConnectionMode;

    public ECameraModi ECameraMode { get => m_eCameraMode; set => m_eCameraMode = value; }
    [SerializeField] private ECameraModi m_eCameraMode;

    public bool GameIsPaused { get => m_gameIsPaused; set { m_gameIsPaused = value; } }
    [SerializeField] private bool m_gameIsPaused;
}