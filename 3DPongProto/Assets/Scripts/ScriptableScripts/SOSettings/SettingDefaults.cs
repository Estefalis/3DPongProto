using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Settings/SettingDefaults", fileName = "New SettingDefaults")]
public class SettingDefaults : ScriptableObject
{
    public bool m_DefaultXAxisInversion = false;
    public bool m_DefaultYAxisInversion = false;

    [Range(0.01f, 1f)] public float m_DefaultSensitivityX = 0.6f;
    [Range(0.01f, 1f)] public float m_DefaultSensitivityY = 0.75f;

    public float m_DefaultMasterVolume = 1f;
    public float m_DefaultBackgroundVolume = 0.8f;
    public float m_DefaultSoundeffectVolume = 0.8f;

    public int m_DefaultQualityLevel = 2;
    public int m_DefaultResolutionIndex = 2;
    public bool m_DefaultScreenMode = true;
}
