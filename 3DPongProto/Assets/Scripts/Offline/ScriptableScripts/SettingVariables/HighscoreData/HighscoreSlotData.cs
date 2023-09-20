using System;
using UnityEngine;

[Serializable]
[CreateAssetMenu(menuName = "Scriptable Objects/Highscore List/Highscore Slot Data", fileName = "Highscore Slot Data")]
public class HighscoreSlotData : ScriptableObject
{
    //Details-Structure for each listed Object.
    public int SetMaxRounds;
    public int SetMaxPoints;
    public string WinningPlayer;
    public string MatchWinDate;
    public float TotalPlaytime;
}