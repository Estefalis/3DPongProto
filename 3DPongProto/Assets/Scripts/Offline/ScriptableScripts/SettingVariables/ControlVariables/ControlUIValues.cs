using System;
using UnityEngine;

[Serializable]
[CreateAssetMenu(menuName = "Scriptable Objects/Control Settings/Control Values", fileName = "Control Values")]
public class ControlUIValues : ScriptableObject
{
    public float LastXMoveSpeed;
    public float LastYRotSpeed;
}