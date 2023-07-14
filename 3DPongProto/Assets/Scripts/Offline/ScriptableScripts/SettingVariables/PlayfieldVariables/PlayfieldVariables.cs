using System;
using UnityEngine;

[Serializable]
[CreateAssetMenu(menuName = "Scriptable Objects/Playfield Settings/Dimensions", fileName = "Playfield Variables")]
public class PlayfieldVariables : ScriptableObject
{
    [Header("Playfield-Dimensions")]
    public float GroundWidth;
    public float GroundLength;
}