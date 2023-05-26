using Sirenix.OdinInspector.Editor;
using UnityEngine;

public class OdinColorTest : MonoBehaviour
{
    [Vector2SliderAttributes(0, 20)]
    public Vector2 testVector;

    [CustomColor]
    public Color testColor;
}