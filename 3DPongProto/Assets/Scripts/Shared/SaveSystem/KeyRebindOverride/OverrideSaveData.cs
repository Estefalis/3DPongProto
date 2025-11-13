using System.Collections.Generic;

[System.Serializable]
//Wrapper class to hold a list of all overrides for JSON serialization.
public class OverrideSaveData
{
    public List<KeyBindingOverride> bindingOverrides = new();
}