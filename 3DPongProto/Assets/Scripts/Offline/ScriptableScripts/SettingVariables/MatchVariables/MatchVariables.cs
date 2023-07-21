using System;
using UnityEngine;

[Serializable]
[CreateAssetMenu(menuName = "Scriptable Objects/Match Settings/Dimensions and Variables", fileName = "Match Variables")]
public class MatchVariables : ScriptableObject
{
    [Header("Round-Details")]
    public int LastRoundIndex;
    public int LastMaxPointIndex;

    public bool InfiniteRounds;
    public bool InfinitePoints;

    [Header("Playfield-Dimensions")]
    public float GroundWidth;
    public float GroundLength;

    [Header("Player-Adjustments")]
    public float PaddleWidthAdjustment;
}