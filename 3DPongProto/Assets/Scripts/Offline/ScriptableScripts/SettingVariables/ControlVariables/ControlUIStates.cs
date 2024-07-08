using System;
using UnityEngine;

[Serializable]
[CreateAssetMenu(menuName = "Scriptable Objects/Control Settings/Control States", fileName = "Control States")]
public class ControlUIStates : ScriptableObject
{
    public bool InvertXAxis;
    public bool InvertYAxis;
    public bool CustomXSensitivity;
    public bool CustomYSensitivity;
    public int ShownPlayerIndex;
}