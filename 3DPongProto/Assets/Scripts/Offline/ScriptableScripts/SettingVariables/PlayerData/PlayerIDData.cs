using System;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
[CreateAssetMenu(menuName = "Scriptable Objects/Player Data/Player Customization", fileName = "Player Information")]
public class PlayerIDData : ScriptableObject
{
    public GameObject Prefab;
    public string PlayerName;
    public int PlayerId;
    public Image Avatar;

    public bool PlayerOnFrontline;
    public bool DefaultKeyboard;    //TODO: Implement a choice to prefer Keyboard and Mouse or Gamepad as main inputDevice.
}