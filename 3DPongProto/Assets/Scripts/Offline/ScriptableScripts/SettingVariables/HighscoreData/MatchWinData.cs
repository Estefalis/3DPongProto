using System;
using UnityEngine;

[Serializable]
[CreateAssetMenu(menuName = "Scriptable Objects/Highscore List/Slot Entries", fileName = "Highscore Entries")]
public class MatchWinData : ScriptableObject
{
    public int SetMaxRounds;
    public int SetMaxPoints;
    public int WinningPlayer;
    public string MatchWinDate;
    public string TotalPlaytime;
}