using System;
using UnityEngine;

[Serializable]
[CreateAssetMenu(menuName = "Scriptable Objects/Match Settings/Indices and Boolean", fileName = "MatchUISettings")]
public class MatchUIStates : ScriptableObject
{
    [Header("Round-Details")]
    public bool InfiniteRounds;
    public bool InfinitePoints;
    public int LastRoundDdIndex;
    public int LastMaxPointDdIndex;

    [Header("Playfield-Dimensions")]
    public bool FixRatio;
    public int LastFieldWidthDdIndex;
    public int LastFieldLengthDdIndex;

    [Header("Line-Dropdown-Indices")]
    public int TPOneBacklineDdIndex;
    public int TPTwoBacklineDdIndex;
    public int TPOneFrontlineDdIndex;
    public int TPTwoFrontlineDdIndex;
}