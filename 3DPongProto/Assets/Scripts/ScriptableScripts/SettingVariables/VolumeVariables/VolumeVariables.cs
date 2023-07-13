using System;
using UnityEngine;

[Serializable]
[CreateAssetMenu(menuName = "Scriptable Objects/Volume Settings/Volume Variables", fileName = "Volume Variables")]
public class VolumeVariables : ScriptableObject
{
    [Header("Volume")]
    public float LatestMasterVolume;
    public float LatestBGMVolume;
    public float LatestSFXVolume;
}