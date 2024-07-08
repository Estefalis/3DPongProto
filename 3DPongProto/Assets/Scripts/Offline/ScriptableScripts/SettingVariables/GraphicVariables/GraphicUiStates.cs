using System;
using UnityEngine;

[Serializable]
[CreateAssetMenu(menuName = "Scriptable Objects/Graphic Settings/Quality and Resolution", fileName = "Graphic Indices and Boolean")]
public class GraphicUIStates : ScriptableObject
{
    public int QualityLevelIndex;
    public int SelectedResolutionIndex;
    public bool FullScreenMode;

    [Header("Camera-Settings")]
    public ECameraModi SetCameraMode;
}