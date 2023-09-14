using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Match Settings/MatchControl", fileName = "Match Control")]
public class MatchControl : ScriptableObject
{
    public EGameModi EGameConnectionModi { get => eGameConnectionMode; set => eGameConnectionMode = value; }
    [SerializeField] private EGameModi eGameConnectionMode;
}