using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class TurnManager : NetworkBehaviour
{
    public static TurnManager Instance;

    [Header("Game Settings")]
    public int totalPlayers = 6;
    public int totalBullets = 1;

    private NetworkVariable<int> currentTurn = new NetworkVariable<int>(0);
    private NetworkVariable<int> bulletPosition = new NetworkVariable<int>(-1);
    private int currentBulletIndex = 0;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            // Randomize bullet position at the start of the game
            bulletPosition.Value = Random.Range(0, totalPlayers);
            Debug.Log($"Bullet is in position: {bulletPosition.Value}");
        }
        
        currentTurn.OnValueChanged += OnTurnChanged;
    }

    private void OnTurnChanged(int oldTurn, int newTurn)
    {
        Debug.Log($"It's Player {newTurn}'s turn.");
        if (newTurn ==(int) NetworkManager.Singleton.LocalClientId)
        {
            Debug.Log("It's your turn! Spin the gun and shoot!");
            StartCoroutine(PlayerTurnCoroutine());
        }
    }

    private IEnumerator PlayerTurnCoroutine()
    {
        yield return new WaitForSeconds(1f);  // Simulate some delay before action
        
        bool gunFires = currentBulletIndex == bulletPosition.Value;

        if (gunFires)
        {
            Debug.Log("Bang! You're out!");
            // Handle player elimination logic here (e.g., disable player object)
            EliminatePlayerServerRpc(NetworkManager.Singleton.LocalClientId);
        }
        else
        {
            Debug.Log("Click! You're safe.");
        }

        currentBulletIndex = (currentBulletIndex + 1) % totalPlayers;
        if (IsServer)
        {
            AdvanceTurnServerRpc();
        }
    }

    [ServerRpc]
    private void EliminatePlayerServerRpc(ulong playerId)
    {
        Debug.Log($"Player {playerId} has been eliminated.");
        // Add logic to mark the player as eliminated
    }

    [ServerRpc]
    public void AdvanceTurnServerRpc()
    {
        int nextTurn = (currentTurn.Value + 1) % totalPlayers;
        currentTurn.Value = nextTurn;

        // Check if only one player is left
        if (CheckForWinner())
        {
            Debug.Log("We have a winner!");
            // Announce the winner and end the game
            EndGameServerRpc();
        }
    }

    private bool CheckForWinner()
    {
        // Implement logic to check how many players are still active
        return false;  // Replace with actual check
    }

    [ServerRpc]
    private void EndGameServerRpc()
    {
        Debug.Log("Game over!");
        // Handle game end logic
    }
}
