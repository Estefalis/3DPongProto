using System;
using UnityEngine;

[Serializable]
[CreateAssetMenu(menuName = "Scriptable Objects/Match Settings/Dimensions and Variables", fileName = "Match Variables")]
public class MatchValues : ScriptableObject
{
    [Header("Round-Details")]
    public long StartDateTime;
    public float StartTime;

    public int SetMaxRounds;
    public uint CurrentRoundNr;

    public int SetMaxPoints;
    public int WinPointDifference;

    [Header("TMP-Match-Values")]
    public uint CurrentPointsTPOne;
    public uint TotalPointsTPOne;

    public uint CurrentPointsTPTwo;
    public uint TotalPointsTPTwo;

    [Header("Playfield-Dimensions")]
    public int SetGroundWidth;
    public int SetGroundLength;

    [Header("Player-Adjustments")]
    public float PaddleWidthAdjustment;
    public float MinFrontLineDistance;
    public float FrontlineAdjustment;
    public float MinBackLineDistance;
    public float BacklineAdjustment;
    public float MaxPushDistance;

    public float XPaddleScale;
    public float YPaddleScale;
    public float ZPaddleScale;
}