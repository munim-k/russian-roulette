using UnityEngine;
using Unity.Netcode;

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

    public NetworkVariable<ulong> winClientId;

    [SerializeField] PlayerManager playerManager;

    // bool gameStarted = false;
    // bool allowShoot = false;

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
        shootButton.SetActive(false);
        gameOverScreen.SetActive(true);
        
        // bottle.gameObject.SetActive(false);
        // gameScreen.SetActive(false);
        // lobbyUI.SetActive(true);
        // multiplayerSpawnManager.SetActive(false);
        // activeLobbiesUI.SetActive(false);
        // mainMenuUI.SetActive(true);
        // TurnManager.Instance.ResetGame();
    }

    [ClientRpc]
    public void SetWonClientRpc(string sessionId) {
        gameOverText.text = "You have " + (sessionId == playerManager.sessionId ? "won" : "lost") + "!";
        Invoke(nameof(ResetGame), 5f);
    }

    public void ResetGame() {
        mainMenuUI.SetActive(true);
        gameOverScreen.SetActive(false);
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
        playerListManager.Reset();
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
            multiplayerSpawnManager.SetActive(true);
            lobbyUI.SetActive(false);
            gameScreen.SetActive(true);
            StartGameClientRpc();

            int playersInGame = sessionManager.activeSession.Players.Count;
            
            // Initialize TurnManager
            TurnManager.Instance.totalPlayers.Value = playersInGame;
            TurnManager.Instance.totalBullets.Value = Bullets;
            TurnManager.Instance.currentBulletsInBarrel.Value = Bullets;
            TurnManager.Instance.sessionManager = sessionManager;
            TurnManager.Instance.alivePlayerCount.Value = playersInGame;
            for (int i = 0; i < playersInGame; i++)
            {
                TurnManager.Instance.alivePlayersCheck.Add(true);
                TurnManager.Instance.playerClientIds.Add(NetworkManager.Singleton.ConnectedClientsList[i].ClientId);
                TurnManager.Instance.playerSessionIds.Add(sessionManager.activeSession.Players[i].Id);
            }
            // Set bullet positions in the barrel
            for (int i = 0; i < TurnManager.Instance.totalBullets.Value; i++)
            {
                int random = Random.Range(0, TurnManager.Instance.barrelSize.Value - 1);
                while (TurnManager.Instance.bulletPositions[random])
                {
                    random = Random.Range(0, TurnManager.Instance.barrelSize.Value - 1);
                }
                TurnManager.Instance.bulletPositions[random] = true;
            }

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
        // gameStarted = true;
    }

    [ClientRpc]
    void SpinBottleClientRpc(int playerNumber, int spins, int time)
    {
        bottle.GetComponent<Bottle>().SpinBottle(playerNumber, spins, time);
    }
    
}