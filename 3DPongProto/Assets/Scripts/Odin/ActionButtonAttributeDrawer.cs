using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector.Editor.ActionResolvers;
using UnityEngine;

public class ActionButtonAttributeDrawer : OdinAttributeDrawer<ActionButtonAttribute>
{
    private ActionResolver m_actionResolver;

    protected override void Initialize()
    {
        this.m_actionResolver = ActionResolver.Get(this.Property, this.Attribute.m_action);
    }

    protected override void DrawPropertyLayout(GUIContent _label)
    {
        this.m_actionResolver.DrawError();

        if (GUILayout.Button("PerformAction"))
        {
            this.m_actionResolver.DoActionForAllSelectionIndices();
        }

        this.CallNextDrawer(_label);
    }
}