using System;
using UnityEngine;

[Serializable]
[CreateAssetMenu(menuName = "Scriptable Objects/Graphic Settings/Quality and Resolution", fileName = "Graphic Variables")]
public class GraphicVariables : ScriptableObject
{
    [Header("Graphic")]
    public int QualityLevel;
    public int SelectedResolutionIndex;
    public int ActiveCameraIndex;
    public bool ScreenMode;
}