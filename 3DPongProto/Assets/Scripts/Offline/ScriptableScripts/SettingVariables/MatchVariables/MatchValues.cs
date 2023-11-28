using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
[CreateAssetMenu(menuName = "Scriptable Objects/Match Settings/Dimensions and Variables", fileName = "Match Variables")]
public class MatchValues : ScriptableObject
{
    public EGameModi EGameConnectionModi { get => eGameConnectionMode; set => eGameConnectionMode = value; }
    [SerializeField] private EGameModi eGameConnectionMode;
    public List<PlayerIDData> PlayerDataInGame;
    public List<GameObject> PlayersInGame;
    public uint PlayerInGame;
    public uint MaxPlayerInGame;
    [Header("Round-Details")]
    public float StartTime;             //each Match
    public uint CurrentRoundNr;         //each Match
    public int WinPointDifference;      //each Match (MatchManager)    

    public double TotalPoints;          //each Match
    public string WinningPlayer;        //each Match
    public string MatchWinDate;         //each Match
    public float TotalPlaytime;         //each Match

    [Header("TMP-Match-Values")]
    public uint MatchPointsTPOne;       //each Match
    public double TotalPointsTPOne;     //each Match
    public uint MatchPointsTPTwo;       //each Match
    public double TotalPointsTPTwo;     //each Match

    [Header("Player-Adjustments")]
    public float PaddleWidthAdjustment; //each Match (Match Manager sends Player)
    public float MaxPushDistance;       //each Match
    public float XPaddleScale;          //each Match
    public float YPaddleScale;          //each Match
    public float ZPaddleScale;          //each Match
}