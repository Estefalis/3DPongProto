using System;
using UnityEngine;

[Serializable]
[CreateAssetMenu(menuName = "Scriptable Objects/Volume Settings/Volume Variables", fileName = "Volume Variables")]
public class VolumeVariables : ScriptableObject
{
    [Header("Volume")]
    public float m_LatestMasterVolume;
    public float m_LatestBGMVolume;
    public float m_LatestSFXVolume;
}