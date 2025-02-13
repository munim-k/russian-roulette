using UnityEngine;
using Unity.Netcode;
using Unity.Collections;
using System.Collections;
using System.Collections.Generic;

public class TurnManager : NetworkBehaviour
{
    public static TurnManager Instance;

    // [SerializeField]  SessionManager sessionManager;

    [Header("Game Settings")]
    public NetworkVariable<int> totalPlayers = new(0); // Set during lobby setup
    // public NetworkVariable<List<string>> playerIds = new(); // Set during lobby setup
    public NetworkList<FixedString64Bytes> playerIds = new();
    public SessionManager sessionManager;
    public NetworkList<bool> alivePlayers = new(); // Set during lobby setup
    public NetworkVariable<int> alivePlayerCount = new();

    public int totalBullets; // Set during lobby setup
    public int barrelSize = 6;  // Default barrel size for a revolver

    public NetworkVariable<int> currentBulletsInBarrel = new(0);
    public NetworkVariable<FixedString64Bytes> currentTurn = new(""); // id
    public NetworkVariable<int> currentTurnIndex = new(0);
    public NetworkList<bool> bulletPositions = new();
    private int currentChamberPosition = 0;
    public string currentPlayerId;

    [SerializeField] GameObject gun; // To spin gun
    [SerializeField] GameObject shootButton; // To enable only on your turn

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            // Debug.Log("TurnManager Spawned");
            for (int i = 0; i < barrelSize; i++) {
                bulletPositions.Add(false);
            }
            // Set bullet positions in the barrel
            for (int i = 0; i < totalBullets; i++)
            {
                int random = Random.Range(0, barrelSize - 1);
                while (bulletPositions[random])
                {
                    random = Random.Range(0, barrelSize - 1);
                }
                bulletPositions[random] = true;
            }
        }

        currentTurn.OnValueChanged += OnTurnChanged;
    }

    private void OnTurnChanged(FixedString64Bytes oldTurn, FixedString64Bytes newTurn)
    {
        Debug.Log($"It's Player {newTurn}'s turn.");
        if (newTurn == currentPlayerId)
        {
            Debug.Log("It's your turn! Spin the gun and shoot!");
        }
    }

    public void Shoot() {
        Debug.Log("Shoot Pressed");
        StartCoroutine(nameof(PlayerTurnCoroutine));
    }

    private IEnumerator PlayerTurnCoroutine()
    {
        yield return new WaitForSeconds(1f);  // Simulate some delay before action

        bool gunFires = bulletPositions[currentChamberPosition];

        if (gunFires)
        {
            Debug.Log("Bang! You're out!");
            // Handle player elimination logic here (e.g., disable player object)
            alivePlayers[currentTurnIndex.Value] = false;
            // EliminatePlayerServerRpc(playerIds.Value[currentTurnIndex.Value]);
            EliminatePlayerServerRpc(playerIds[currentTurnIndex.Value]);
            currentBulletsInBarrel.Value--;
            alivePlayerCount.Value--;
        }
        else
        {
            Debug.Log("Click! You're safe.");
        }

        if (IsServer)
        {
            currentChamberPosition = (currentChamberPosition + 1) % totalPlayers.Value;
            while (true) {
                currentTurnIndex.Value = (currentTurnIndex.Value + 1) % totalPlayers.Value;
                Debug.Log(currentTurnIndex.Value);
                Debug.Log(alivePlayers[currentTurnIndex.Value]);
                Debug.Log(totalPlayers.Value);
                if (alivePlayers[currentTurnIndex.Value]) break;
            }
            // currentTurn.Value = playerIds.Value[currentTurnIndex.Value];
            // ChangeTurnClientRpc(playerIds.Value[currentTurnIndex.Value]);
            currentTurn.Value = playerIds[currentTurnIndex.Value];
            ChangeTurnClientRpc(playerIds[currentTurnIndex.Value]);

            AdvanceTurnServerRpc();
        }
    }

    [ServerRpc]
    public void ExecuteShootServerRpc() {

    }

    [ClientRpc]
    public void ChangeTurnClientRpc(FixedString64Bytes id) {
        Debug.Log($"It's Player {id}'s turn.");
        shootButton.SetActive(id == currentPlayerId);
    }

    [ServerRpc]
    private void EliminatePlayerServerRpc(FixedString64Bytes playerId)
    {
        Debug.Log($"Player {playerId} has been eliminated.");
        // Add logic to mark the player as eliminated
    }

    [ServerRpc]
    public void AdvanceTurnServerRpc()
    {
        int nextTurn = (currentTurnIndex.Value + 1) % totalPlayers.Value;
        // Debug.Log(totalPlayers.Value);
        currentTurnIndex.Value = nextTurn;
        // currentTurn.Value = playerIds.Value[nextTurn];
        currentTurn.Value = playerIds[nextTurn];

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
        if (alivePlayerCount.Value == 1)
        {
            return true;
        }
        return false;  // Replace with actual check
    }

    [ServerRpc]
    private void EndGameServerRpc()
    {
        Debug.Log("Game over!");
        EndGameClientRpc();
    }

    [ClientRpc]
    private void EndGameClientRpc()
    {
        // Debug.Log("Game over!");

        GameManager.Instance.GameOver();
        
    }

    [ClientRpc]
    public void SpinGunClientRpc()
    {
        gun.transform.Rotate(0, 0, Random.Range(360, 720));
    }
}
