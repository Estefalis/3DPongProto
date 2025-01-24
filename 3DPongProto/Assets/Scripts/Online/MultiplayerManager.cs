using System.Collections;
using System.Collections.Generic;
using ThreeDeePongProto.Shared.PlayerCharacter;
using UnityEngine;
using UnityEngine.InputSystem;

public class MultiplayerManager : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform[] spawnPoints;

    private void Start()
    {
        PlayerInputManager.instance.onPlayerJoined += OnPlayerJoined;
    }

    private void OnPlayerJoined(PlayerInput playerInput)
    {
        //// Spawn position are to be defined in the future.
        //int playerIndex = playerInput.playerIndex;
        //playerInput.transform.position = spawnPoints[playerIndex].position;

        //// Initialize CharacterInputHandler.
        //var playerInputHandler = playerInput.GetComponent<CharacterInputHandler>();
        //if (playerInputHandler != null)
        //{
        //    playerInputHandler.InitializePlayer(playerInput);
        //}

        //Debug.Log($"Player {playerIndex} joined!");
    }

    private void OnDestroy()
    {
        PlayerInputManager.instance.onPlayerJoined -= OnPlayerJoined;
    }
}
