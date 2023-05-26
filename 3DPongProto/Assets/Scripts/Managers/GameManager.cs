using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EGameModi
{
    LocalPC,
    LAN,
    Internet
}

public class GameManager : GenericSingleton<GameManager>
{
    public EGameModi EGameModi { get; set; }
}