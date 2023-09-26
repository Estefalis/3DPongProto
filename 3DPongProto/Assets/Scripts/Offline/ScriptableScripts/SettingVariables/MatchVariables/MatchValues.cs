using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
[CreateAssetMenu(menuName = "Scriptable Objects/Match Settings/Dimensions and Variables", fileName = "Match Variables")]
public class MatchValues : ScriptableObject
{
    [Header("Round-Details")]
    public List<string> PlayerInGame = new();
    public float StartTime;
    public int WinPointDifference;
    public uint CurrentRoundNr;
    public int SetMaxRounds;
    public int SetMaxPoints;

    public double TotalPoints;
    public string WinningPlayer;
    public string MatchWinDate;
    public float TotalPlaytime;

    [Header("TMP-Match-Values")]
    public uint MatchPointsTPOne;
    public double TotalPointsTPOne;
    public uint MatchPointsTPTwo;
    public double TotalPointsTPTwo;

    [Header("Playfield-Setup")]
    public int SetGroundWidth;
    public int SetGroundLength;
    public float MinFrontLineDistance;
    public float FrontlineAdjustment;
    public float MinBackLineDistance;
    public float BacklineAdjustment;

    [Header("Player-Adjustments")]
    public float PaddleWidthAdjustment;
    public float MaxPushDistance;
    public float XPaddleScale;
    public float YPaddleScale;
    public float ZPaddleScale;
}