using UnityEngine;
using Unity.Netcode;


public class MultiplayerSpawnManager : NetworkBehaviour
{
    [SerializeField] private Transform[] spawnPoints;  
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameManager gameManager;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            int spawnIndex = (int)NetworkManager.Singleton.LocalClientId % gameManager.Players;
            SpawnPlayer(spawnIndex);
        }
    }

    private void SpawnPlayer(int spawnIndex)
    {
        GameObject playerInstance = Instantiate(playerPrefab, spawnPoints[spawnIndex].position, Quaternion.identity);
        playerInstance.GetComponent<NetworkObject>().Spawn(true);  // Spawn the player for all clients
    }
}