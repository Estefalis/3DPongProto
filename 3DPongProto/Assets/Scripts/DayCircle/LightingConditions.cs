using System;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "Lighting Condition", menuName = "Scriptables/Lighting Preset", order = 0)]
public class LightingConditions : ScriptableObject
{
    public Gradient m_ambientColor;
    public Gradient m_directionColor;
    public Gradient m_fogcolor;
}
