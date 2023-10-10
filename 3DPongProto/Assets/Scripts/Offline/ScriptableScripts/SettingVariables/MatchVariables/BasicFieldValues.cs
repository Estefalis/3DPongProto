using System;
using UnityEngine;

[Serializable]
[CreateAssetMenu(menuName = "Scriptable Objects/Playfield Settings/Setup Values", fileName = "FieldValues")]
public class BasicFieldValues : ScriptableObject
{
    public int SetGroundWidth;
    public int SetGroundLength;
    public float MinBackLineDistance;
    public float BacklineAdjustment;
    public float MinFrontLineDistance;
    public float FrontlineAdjustment;
}