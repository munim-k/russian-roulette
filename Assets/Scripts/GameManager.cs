using UnityEngine;
using Unity.Netcode;
using Unity.Services.Multiplayer;

public class GameManager : NetworkBehaviour
{

    public static GameManager Instance;

    public int Bullets;
    public int Players;
    public bool isPrivate = false;
    [SerializeField]
    private GameObject lobbyUI;
    [SerializeField]
    GameObject mainMenuUI;
    [SerializeField]
    GameObject activeLobbiesUI;
    [SerializeField]
    private SessionManager sessionManager;
    [SerializeField] PlayerListManager playerListManager;
    [SerializeField] GameObject multiplayerSpawnManager;
    [SerializeField] private GameObject gameScreen;

    [SerializeField] private GameObject gameOverScreen;
    [SerializeField] TMPro.TextMeshProUGUI gameOverText;


    [SerializeField] GameObject bottle;
    [SerializeField] GameObject gun;
    [SerializeField] GameObject shootButton;

    [SerializeField] PlayerManager playerManager;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void GameOver() {
        // gameStarted = false;
        // allowShoot = false;

        gun.SetActive(false);
        bottle.SetActive(true);        
        shootButton.SetActive(false);
        
        // gameScreen.SetActive(false);
        // lobbyUI.SetActive(true);
        // multiplayerSpawnManager.SetActive(false);
        // activeLobbiesUI.SetActive(false);
        // mainMenuUI.SetActive(true);
        // TurnManager.Instance.ResetGame();

        gameOverScreen.SetActive(true);
        Invoke(nameof(ResetGame), 5f);
    }

    [ClientRpc]
    public void SetWonClientRpc(string sessionId) {
        gameOverText.text = "You have " + (sessionId == playerManager.sessionId ? "won" : "lost") + "!";
    }

    public void ResetGame() {
        gameOverScreen.SetActive(false);
        gameScreen.SetActive(false);
        mainMenuUI.SetActive(true);
    }

    public void SetBullets(int bullets) {
        Bullets = bullets;
    }

    public void SetPlayers(int players) {
        Players = players;
    }
    
    private void OnEnable()
    {
        sessionManager.OnSessionJoined += PlayerJoined;
    }

    private void OnDisable()
    {
        sessionManager.OnSessionJoined -= PlayerJoined;
    }
    public void OpenLobby()
    {
        isPrivate = false;
    }
    public void CloseLobby()
    {
        isPrivate = true;
    }
    
    public void SessionStarted()
    {
        mainMenuUI.SetActive(false);
        lobbyUI.SetActive(true);
    }

    public void ShowActiveLobbies()
    {
        mainMenuUI.SetActive(false);
        activeLobbiesUI.SetActive(true);
    }

    public void PlayerJoined()
    {
        activeLobbiesUI.SetActive(false);
        lobbyUI.SetActive(true);
        playerListManager.ResetState();
        // playerListManager.Reset();
    }

    public bool IsLobbyHost() {
        return sessionManager.activeSession.IsHost;
    }

    public void StartGame()
    {
        if(!IsLobbyHost())
            return;
        if (IsServer)
        {
            int playersInGame = sessionManager.activeSession.Players.Count;

            if (playersInGame < 2)
            {
                Debug.Log("Not enough players to start the game.");
                return;
            }

            multiplayerSpawnManager.SetActive(true);
            StartGameClientRpc();

            
            // Initialize TurnManager
            // Bullets
            TurnManager.Instance.totalBullets.Value = Bullets;
            TurnManager.Instance.currentBulletsInBarrel.Value = Bullets;
            TurnManager.Instance.bulletPositions.Clear();
            TurnManager.Instance.currentChamberPosition.Value = 0;
            for (int i = 0; i < TurnManager.Instance.barrelSize.Value; i++)
            {
                TurnManager.Instance.bulletPositions.Add(false);
            }
            for (int i = 0; i < TurnManager.Instance.totalBullets.Value; i++)
            {
                int random = Random.Range(0, TurnManager.Instance.barrelSize.Value - 1);
                while (TurnManager.Instance.bulletPositions[random])
                {
                    random = Random.Range(0, TurnManager.Instance.barrelSize.Value - 1);
                }
                TurnManager.Instance.bulletPositions[random] = true;
            }

            // Players
            TurnManager.Instance.totalPlayers.Value = playersInGame;
            TurnManager.Instance.alivePlayerCount.Value = playersInGame;

            TurnManager.Instance.alivePlayersCheck.Clear();
            TurnManager.Instance.playerClientIds.Clear();
            TurnManager.Instance.playerSessionIds.Clear();
            for (int i = 0; i < playersInGame; i++)
            {
                TurnManager.Instance.alivePlayersCheck.Add(true);
                TurnManager.Instance.playerClientIds.Add(NetworkManager.Singleton.ConnectedClientsList[i].ClientId);
                TurnManager.Instance.playerSessionIds.Add(sessionManager.activeSession.Players[i].Id);
            }

            // Others
            TurnManager.Instance.someFlag.Value = false;

            int firstTurn = Random.Range(0, playersInGame);
            TurnManager.Instance.currentTurnIndex.Value = firstTurn;
            TurnManager.Instance.currentTurn.Value = sessionManager.activeSession.Players[firstTurn].Id;
            SpinBottleClientRpc(firstTurn, Random.Range(1, 5), Random.Range(3, 6));
        }
    }

    [ClientRpc]
    void StartGameClientRpc()
    {
        multiplayerSpawnManager.SetActive(true);
        multiplayerSpawnManager.GetComponent<MultiplayerSpawnManager>().OnNetworkSpawnCustom();
            
        lobbyUI.SetActive(false);
        gameScreen.SetActive(true);
    }

    [ClientRpc]
    void SpinBottleClientRpc(int playerNumber, int spins, int time)
    {
        bottle.SetActive(true);
        bottle.transform.rotation = Quaternion.identity;
        // Debug.Log("Spinning bottle for player " + playerNumber + " with " + spins + " spins and " + time + " seconds."); 
        bottle.GetComponent<Bottle>().SpinBottle(playerNumber, spins, time);
    }
    
}