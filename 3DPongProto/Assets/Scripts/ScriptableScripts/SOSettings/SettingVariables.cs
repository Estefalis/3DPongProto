using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Settings/SettingVariables", fileName = "New SettingVariables")]
[Serializable]
public class SettingVariables : ScriptableObject
{
    [Header("Axis")]
    public bool m_XAxisInversion;
    public bool m_YAxisInversion;

    [Space]
    [Range(0.01f, 1f)] public float m_XAxisSensitivity;
    [Range(0.01f, 1f)] public float m_YAxisSensitivity;

    [Header("Volume")]
    public float m_LatestMasterVolume;
    public float m_LatestBGMVolume;
    public float m_LatestSFXVolume;

    [Header("Resolution")]
    [Space]
    public int m_QualityLevel;
    public int m_SelectedResolutionIndex;
    public bool m_ScreenMode;
}