using System;

public class Vector2SliderAttributes : Attribute
{
    public float m_minValue;
    public float m_maxValue;

    public Vector2SliderAttributes(float _minValue, float _maxValue)
    {
        m_minValue = _minValue;
        m_maxValue = _maxValue;
    }
}