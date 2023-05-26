using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

public class Vector2SliderAttributeDrawer : OdinAttributeDrawer<Vector2SliderAttributes, Vector2>
{
    protected override void DrawPropertyLayout(GUIContent _label)
    {
        Rect rect = EditorGUILayout.GetControlRect();

        if (_label != null)
            rect = EditorGUI.PrefixLabel(rect, _label);

        Vector2 value = this.ValueEntry.SmartValue;

        GUIHelper.PushLabelWidth(15);
        value.x = EditorGUI.Slider(rect.AlignLeft(rect.width * 0.5f), " X", value.x, this.Attribute.m_minValue, this.Attribute.m_maxValue);
        value.y = EditorGUI.Slider(rect.AlignRight(rect.width * 0.5f), " Y", value.y, this.Attribute.m_minValue, this.Attribute.m_maxValue);
        GUIHelper.PopLabelWidth();

        this.ValueEntry.SmartValue = value;
    }
}