using System;
using UnityEngine;

[Serializable]
[CreateAssetMenu(menuName = "Scriptable Objects/Match Settings/Indices and Boolean", fileName = "MatchUISettings")]
public class MatchUIStates : ScriptableObject
{
    [Header("Round-Details")]
    public int LastRoundDdIndex;
    public bool InfiniteRounds;

    public int LastMaxPointDdIndex;
    public bool InfinitePoints;

    [Header("Playfield-Dimensions")]
    public int LastFieldWidthDdIndex;
    [Space]
    public int LastFieldLengthDdIndex;
    public bool FixRatio;

    [Header("Line-Dropdown-Indices")]
    public int TPOneFrontDdIndex;
    public int TPTwoFrontDdIndex;
    public int TPOneBacklineIndex;
    public int TPTwoBacklineIndex;
}