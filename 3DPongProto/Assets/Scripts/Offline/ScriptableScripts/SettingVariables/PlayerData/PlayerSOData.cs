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
    public bool DefaultKeyboard;    //TODO: Implement a choice to prefer Keyboard and Mouse or Gamepad as main inputDevice.
}