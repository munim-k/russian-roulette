using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class MultiplayerSpawnManager : NetworkBehaviour
{
    [SerializeField] private Transform[] spawnPoints;  
    [SerializeField] private GameObject playerPrefab;

    [SerializeField] PlayerManager playerManager;

    public void OnNetworkSpawnCustom()
    {
        if (IsServer)
        {
            Debug.Log("Server detected, spawning players...");
            SpawnPlayers();
        }
    }

    private void SpawnPlayers()
    {
        Debug.Log("Spawning players for all connected clients");

        int i = 0;
        foreach (var clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            Debug.Log($"Spawning player for ClientID: {clientId}");
            SpawnPlayer(clientId, i);
            i++;
        }
    }

    private void SpawnPlayer(ulong clientId, int i)
    {
        // Calculate spawn index based on clientId
        int spawnIndex = i;

        // Log the clientId and spawnIndex for debugging
        Debug.Log($"ClientID: {clientId}, SpawnIndex: {spawnIndex}");

        // Get the spawn position
        Vector3 spawnPosition = spawnPoints[spawnIndex].position;

        // Instantiate the player prefab
        GameObject playerInstance = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
        playerInstance.GetComponent<Player>().SetClientId(clientId);
        playerInstance.GetComponent<Player>().SetSessionId(playerManager.sessionId);
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            playerManager.player = playerInstance;
        }

        // Spawn the player on the network with ownership
        NetworkObject networkObject = playerInstance.GetComponent<NetworkObject>();
        networkObject.SpawnWithOwnership(clientId, true);
    }
}