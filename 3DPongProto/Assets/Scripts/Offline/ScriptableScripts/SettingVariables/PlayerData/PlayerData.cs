using System;
using UnityEngine;

[Serializable]
[CreateAssetMenu(menuName = "Scriptable Objects/Player Data/Player Customization", fileName = "Player Information")]
public class PlayerData : ScriptableObject
{
    public string PlayerName;
    public uint PlayerId;
}