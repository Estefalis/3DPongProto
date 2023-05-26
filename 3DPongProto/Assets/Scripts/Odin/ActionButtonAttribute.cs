using System;

public class ActionButtonAttribute : Attribute
{
    public string m_action;

    public ActionButtonAttribute(string _action)
    {
        this.m_action = _action;
    }
}