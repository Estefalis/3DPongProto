using System;
using UnityEngine;

[Serializable]
[CreateAssetMenu(menuName = "Scriptable Objects/Match Settings/Dimensions and Values", fileName = "Match UI-Values")]
public class MatchUIValues : ScriptableObject
{
    public int SetMaxRounds;
    public int SetMaxPoints;
    public int SetGroundWidth;
    public int SetGroundLength;
    public float FrontlineAdjustment;
    public float BacklineAdjustment;
}