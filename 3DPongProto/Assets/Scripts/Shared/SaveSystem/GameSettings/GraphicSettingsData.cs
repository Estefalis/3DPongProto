using System;

[Serializable]
public class GraphicSettingsData
{
    public int QualityLevelIndex = 2; //Medium Default.
    public bool FullScreenMode = true;
    public int splitDdValue = 1; //Horizontal as 2 Player Default. Old (int)ECameraModi.Horizontal.

    public float Brightness = 0.0f; //Post Exposure standard value 0.
    public bool UseDefBrightness = false;
    public float BrightnessBeforeDefault = 0.0001f;

    public string ResolutionString = "";    //Robust on Hardware-Changes ("1920x1080@144hz"). Previous SelectedResolutionIndex.
}