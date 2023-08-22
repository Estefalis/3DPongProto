using System;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
[CreateAssetMenu(menuName = "Scriptable Objects/Player Data/Player Customization", fileName = "Player Information")]
public class PlayerData : ScriptableObject
{
    public string PlayerName;
    public uint PlayerId;
    public uint TotalPoints;
    public Image Avatar;

    public bool PlayerOnFrontline;
}