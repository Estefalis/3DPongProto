using System;
using UnityEngine;

[Serializable]
[CreateAssetMenu(menuName = "Scriptable Objects/Match Settings/Dimensions and Variables", fileName = "Match Variables")]
public class MatchVariables : ScriptableObject
{
    [Header("Round-Details")]
    public int SetRounds;
    public int LastRoundIndex;
    public bool InfiniteRounds;

    public int SetMaxPoints;
    public int LastMaxPointIndex;
    public bool InfinitePoints;

    [Header("TMP-Match-Values")]
    public uint CurrentPointsTeamOne;
    public uint TotalPointsTeamOne;
    
    public uint CurrentPointsTeamTwo;
    public uint TotalPointsTeamTwo;

    [Header("Playfield-Dimensions")]
    public int SetGroundWidth;
    public int LastFieldWidthIndex;
    [Space]
    public int SetGroundLength;
    public int LastFieldLengthIndex;

    public bool FixRatio;

    [Header("Player-Adjustments")]
    public float PaddleWidthAdjustment;
    public float DistanceToGoal;
}