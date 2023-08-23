using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
[CreateAssetMenu(menuName = "Scriptable Objects/Match Settings/Dimensions and Variables", fileName = "Match Variables")]
public class MatchVariables : ScriptableObject
{
    [Header("Round-Details")]
    public int SetMaxRounds;
    public int LastRoundDdIndex;
    public uint CurrentRoundNr;
    public bool InfiniteRounds;

    public int SetMaxPoints;
    public int LastMaxPointDdIndex;
    public int WinPointDifference;
    public bool InfinitePoints;

    [Header("TMP-Match-Values")]
    public uint CurrentPointsTPOne;
    public uint TotalPointsTPOne;
    
    public uint CurrentPointsTPTwo;
    public uint TotalPointsTPTwo;

    [Header("Playfield-Dimensions")]
    public int SetGroundWidth;
    public int LastFieldWidthDdIndex;
    [Space]
    public int SetGroundLength;
    public int LastFieldLengthDdIndex;

    public bool FixRatio;

    [Header("Player-Adjustments")]
    public float PaddleWidthAdjustment;
    public float FrontLineDistance;
    public float BackLineDistance;
    public Array FrontLineUpOnTrue = new bool[4];
}