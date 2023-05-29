using UnityEngine;

public enum EGameModi
{
    LocalPC,
    LAN,
    Internet
}

public class GameManager : GenericSingleton<GameManager>
{
    public EGameModi EGameModi { get => eGame; set => eGame = value; }
    [SerializeField] private EGameModi eGame;
    public bool GameIsPaused { get => m_gameIsPaused; set { m_gameIsPaused = value; } }
    [SerializeField] private bool m_gameIsPaused;
}