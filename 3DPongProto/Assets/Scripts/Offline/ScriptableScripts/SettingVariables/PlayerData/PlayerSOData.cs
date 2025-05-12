using System;
using UnityEngine;

[Serializable]
[CreateAssetMenu(menuName = "Scriptable Objects/Player Data/Player Customization", fileName = "Player Information")]
public class PlayerSOData : ScriptableObject
{
    public GameObject Prefab;
    public string PlayerName;
    public int PlayerId;
    public Sprite Avatar;
    public bool KeepNameOnLoad;
    public bool PlayerOnFrontline;
    public bool DefaultKeyboard;
    public int ToggleID;
}