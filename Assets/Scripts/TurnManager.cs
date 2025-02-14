using UnityEngine;
using Unity.Netcode;
using Unity.Collections;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks.Triggers;
using Unity.VisualScripting;

public class TurnManager : NetworkBehaviour
{
    public static TurnManager Instance;

    [Header("Game Settings")]
    public NetworkVariable<int> totalPlayers = new(0); // Set during lobby setup
    public NetworkList<FixedString64Bytes> playerSessionIds = new(); // Set during lobby setup
    public NetworkList<ulong> playerClientIds = new(); // Set during lobby setup
    public SessionManager sessionManager; // Set during lobby setup
    public NetworkList<bool> alivePlayersCheck = new(); // Set during lobby setup
    public NetworkVariable<int> alivePlayerCount = new(); // Set during lobby setup

    public NetworkVariable<int> totalBullets = new(); // Set during lobby setup
    public NetworkVariable<int> barrelSize = new(6);  // Default barrel size for a revolver

    public NetworkVariable<int> currentBulletsInBarrel = new(0);
    public NetworkVariable<int> currentTurnIndex = new(0);
    public NetworkVariable<FixedString64Bytes> currentTurn = new(""); // id
    public string currentTurnStr;
    public NetworkList<bool> bulletPositions = new();
    public NetworkVariable<int> currentChamberPosition = new(0);
    public string currentPlayerId;

    [SerializeField] GameObject gun; // To spin gun
    [SerializeField] GameObject shootButton; // To enable only on your turn

    public NetworkVariable<bool> someFlag = new(false); // TODO: Rename later

    public PlayerManager playerManager;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    [ServerRpc(RequireOwnership = false)]
    public void InitServerRpc(bool value) {
        someFlag.Value = value;
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            for (int i = 0; i < barrelSize.Value; i++) {
                bulletPositions.Add(false);
            }
        }

        currentTurn.OnValueChanged += OnTurnChanged;
    }

    private void OnTurnChanged(FixedString64Bytes oldTurn, FixedString64Bytes newTurn)
    {
        currentTurnStr = newTurn.ToSafeString();
        if (newTurn == currentPlayerId)
        {
            Debug.Log("It's your turn! Spin the gun and shoot!");
        } else {
            Debug.Log($"It's Player {newTurn}'s turn from 'OnTurnChanged'.");
        }
    }

    private void OnTurnIndexChange(int oldIndex, int newIndex) {
        if (IsServer)
            currentTurn.Value = playerSessionIds[newIndex];
    }

    public void Shoot() {
        Debug.Log("Shoot Pressed");
        StartCoroutine(nameof(PlayerTurnCoroutine));
    }

    private IEnumerator PlayerTurnCoroutine()
    {
        yield return new WaitForSeconds(1f);  // Simulate some delay before action

        bool gunFires = bulletPositions[currentChamberPosition.Value];

        if (gunFires)
        {
            Debug.Log("Bang! You're out!");
            // Handle player elimination logic here (e.g., disable player object)
            ulong clientID = NetworkManager.Singleton.LocalClientId;
            Debug.Log("Client ID: " + clientID);
            EliminatePlayerServerRpc(currentTurn.Value,clientID);
        }
        else
        {
            Debug.Log("Click! You're safe.");
            Debug.Log("click client id: " + NetworkManager.Singleton.LocalClientId);
            ClickServerRpc(NetworkManager.Singleton.LocalClientId);
        }

        InitExecuteShootServerRpc();
    }

    [ClientRpc]
    public void PlayerDieClientRpc(ulong id, bool die = true) {
        
        
        GameObject[] playerInstances = GameObject.FindGameObjectsWithTag("Player");
        Debug.Log("Killed Id: " + id);
        for (int i = 0; i < playerInstances.Length; i++) {
            Debug.Log("Checking id: " + playerInstances[i].GetComponent<Player>().GetClientId());
            if (playerInstances[i].GetComponent<Player>().GetClientId() == id) {
                
                if(die)
                    playerInstances[i].GetComponent<Player>().Die();
                else
                {
                    Debug.Log("Clicking");
                    playerInstances[i].GetComponent<Player>().Click();
                }
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void InitExecuteShootServerRpc () {
        ExecuteShootServerRpc();
    }

    [ServerRpc]
    public void ExecuteShootServerRpc() {
        currentChamberPosition.Value = (currentChamberPosition.Value + 1) % barrelSize.Value;
        
        AdvanceTurnServerRpc();
    }

    [ClientRpc]
    public void ChangeTurnClientRpc(FixedString64Bytes currentTurn2, int index) {
        shootButton.SetActive(currentTurn2 == currentPlayerId);
        if (someFlag.Value) {
            gun.GetComponent<Gun>().SpinGun(index, 0, 0.5f, true);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void ClickServerRpc(ulong clientId) {
        PlayerDieClientRpc(clientId, false);
    }
    [ServerRpc(RequireOwnership = false)]
    private void EliminatePlayerServerRpc(FixedString64Bytes playerId, ulong clientID)
    {
        Debug.Log("Client ID Elim: " + clientID);
        PlayerDieClientRpc(clientID);
        Debug.Log($"Player {playerId} has been eliminated.");
        currentBulletsInBarrel.Value--;
        if (currentBulletsInBarrel.Value == 0)
        {
            currentBulletsInBarrel.Value = GameManager.Instance.Bullets;
            for (int i = 0; i < TurnManager.Instance.totalBullets.Value; i++)
            {
                int random = Random.Range(0, TurnManager.Instance.barrelSize.Value - 1);
                while (TurnManager.Instance.bulletPositions[random])
                {
                    random = Random.Range(0, TurnManager.Instance.barrelSize.Value - 1);
                }
                TurnManager.Instance.bulletPositions[random] = true;
            }
        }
        alivePlayerCount.Value--;
        alivePlayersCheck[currentTurnIndex.Value] = false;
    }

    [ServerRpc]
    public void AdvanceTurnServerRpc()
    {
        int tempIndex = currentTurnIndex.Value;
        while (true) {
            tempIndex = (tempIndex + 1) % totalPlayers.Value;
            if (alivePlayersCheck[tempIndex]) break;
        }
        currentTurnIndex.Value = tempIndex;
        currentTurn.Value = playerSessionIds[tempIndex];
        ChangeTurnClientRpc(currentTurn.Value, currentTurnIndex.Value);

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
            GameManager.Instance.SetWonClientRpc(currentTurn.Value.ToSafeString());
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
        sessionManager.LeaveSession();
        GameManager.Instance.GameOver();
        
    }
}
