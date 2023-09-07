using System;
using UnityEngine;

[Serializable]
[CreateAssetMenu(menuName = "Scriptable Objects/Graphic Settings/Quality and Resolution", fileName = "Graphic Indices and Boolean")]
public class GraphicUiStates : ScriptableObject
{
    public int QualityLevel;
    public int SelectedResolutionIndex;
    public bool FullScreenMode;
    public int ActiveCameraIndex;
}