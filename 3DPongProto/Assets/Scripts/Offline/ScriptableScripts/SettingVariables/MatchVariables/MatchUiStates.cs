using System;
using UnityEngine;
using ThreeDeePongProto.Offline.Settings;

[Serializable]
[CreateAssetMenu(menuName = "Scriptable Objects/Match Settings/Indices and Boolean", fileName = "MatchUISettings")]
public class MatchUIStates : ScriptableObject
{
    [Header("Player-States")]
    public EPlayerMatchLimit EPlayerAmount;
    public EGameModi EGameConnectModi { get => eGameConnectModi; set => eGameConnectModi = value; }
    [SerializeField] private EGameModi eGameConnectModi;
    public bool TpOneRotReset;
    public bool TpTwoRotReset;

    [Header("Match-Details")]
    public bool GameRuns;
    public bool InfiniteMatch;
    public int LastRoundDdIndex;
    public int LastMaxPointDdIndex;
    public int MaxRounds;
    public int MaxPoints;

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