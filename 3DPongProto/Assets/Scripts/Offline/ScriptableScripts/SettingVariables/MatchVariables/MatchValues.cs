using System;
using UnityEngine;

[Serializable]
[CreateAssetMenu(menuName = "Scriptable Objects/Match Settings/Dimensions and Variables", fileName = "Match Variables")]
public class MatchValues : ScriptableObject
{
    [Header("Round-Details")]
    public float StartTime;
    public float TotalPlaytime;
    public uint CurrentRoundNr;
    public int SetMaxRounds;
    public int SetMaxPoints;
    public int WinPointDifference;
    public string WinningPlayer;
    public string MatchWinDate;
    public int ListIndex;

    [Header("TMP-Match-Values")]
    public uint CurrentPointsTPOne;
    public uint TotalPointsTPOne;
    public uint CurrentPointsTPTwo;
    public uint TotalPointsTPTwo;

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