[System.Serializable]
public class KeyBindingOverride
{
    public string actionName;
    public int bindingIndex;
    public int playerIndex; //-1 for a global override.
    public string overridePath;
}