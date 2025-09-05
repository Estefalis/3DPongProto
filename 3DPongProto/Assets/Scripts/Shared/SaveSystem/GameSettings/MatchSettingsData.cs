using System;
using System.Collections.Generic;

public enum EPlayerLine { Backline, Frontline };

[Serializable]
public class MatchSettingsData
{
    //Connection-Mode
    public int GameConnectMode;                 //TODO: Move into ConnectManager/Fusion etc.

    public EGameMode EGameMode = EGameMode.Normal;
    public bool ObstaclesEnabled = false;       //NOTE: Enum for multiple Layout-Options may be a better choice for complex structures.

    //RoundSetting and PointSetting
    public int RoundsToWin = 5;
    public int RoundPoints = 25;
    public int WinPointDifference = 2;

    //Field-Dimensions
    public int FieldWidth = 25;
    public int FieldLength = 50;
    public bool FixFieldRatio = false;

    //Player-Setup
    public int PlayerCount = 2;
    public bool RotationReset = true;

    public float BacklineDistance = 1.5f;
    public float FrontlineDistance = 6.0f;
    public Dictionary<int, EPlayerLine> PlayerPositions = new();

    public MatchSettingsData()
    {
        //Standard 2-vs-2-Match
        PlayerPositions.Add(0, EPlayerLine.Backline);   //Player 1
        PlayerPositions.Add(1, EPlayerLine.Backline);   //Player 2
        PlayerPositions.Add(2, EPlayerLine.Frontline);  //Player 3
        PlayerPositions.Add(3, EPlayerLine.Frontline);  //Player 4
    }
}