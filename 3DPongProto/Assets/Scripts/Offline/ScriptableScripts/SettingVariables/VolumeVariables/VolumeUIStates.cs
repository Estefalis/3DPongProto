using System;
using UnityEngine;

[Serializable]
[CreateAssetMenu(menuName = "Scriptable Objects/Volume Settings/Volume States", fileName = "Volume States")]
public class VolumeUIStates : ScriptableObject
{
    public bool MasterMuteIsOn;
    public bool BGMMuteIsOn;
    public bool SFXMuteIsOn;
}