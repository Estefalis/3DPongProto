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
}