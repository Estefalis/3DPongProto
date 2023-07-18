using System;
using UnityEngine;

[Serializable]
[CreateAssetMenu(menuName = "Scriptable Objects/Match Settings/Dimensions and Variables", fileName = "Match Variables")]
public class MatchVariables : ScriptableObject
{
    [Header("Round-Details")]
    public uint LastRoundIndex;
    public uint LastMaxPointIndex;

    [Header("Playfield-Dimensions")]
    public float GroundWidth;
    public float GroundLength;

    [Header("Player-Adjustments")]
    public float PaddleWidthAdjustment;
}