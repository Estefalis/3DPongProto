using System;
using UnityEngine;

[Serializable]
[CreateAssetMenu(menuName = "Scriptable Objects/Graphic Settings/Quality and Resolution", fileName = "Graphic Variables")]
public class GraphicVariables : ScriptableObject
{
    [Header("Graphic")]
    public int m_QualityLevel;
    public int m_SelectedResolutionIndex;
    public int m_ActiveCameraIndex;
    public bool m_ScreenMode;
}