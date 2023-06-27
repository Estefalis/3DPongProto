using System;
using UnityEngine;

[Serializable]
[CreateAssetMenu(menuName = "Scriptable Objects/Day Circle/Lighting Preset", fileName = "Lighting Condition", order = 0)]
public class LightingConditions : ScriptableObject
{
    public Gradient m_ambientColor;
    public Gradient m_directionColor;
    public Gradient m_fogcolor;
}
