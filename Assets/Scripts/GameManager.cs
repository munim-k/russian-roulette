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

        Debug.Log("Resetting");

        gameOverScreen.SetActive(true);
        Invoke(nameof(ResetGame), 5f);
    }

    [ClientRpc]
    public void SetWonClientRpc(string sessionId) {
        gameOverText.text = "You have " + (sessionId == playerManager.sessionId ? "won" : "lost") + "!";
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
        playerListManager.ResetState();
        // playerListManager.Reset();
    }

    public bool IsLobbyHost() {
        Debug.Log("Here?");
        return sessionManager.activeSession.IsHost;
    }

    public void StartGame()
    {
        Debug.Log("Session Manager 1: " + sessionManager.activeSession);
        // if(!IsLobbyHost())
        //     return;
        if (IsServer)
        {
            Debug.Log("Start 1");
            multiplayerSpawnManager.SetActive(true);
            lobbyUI.SetActive(false);
            gameScreen.SetActive(true);
            StartGameClientRpc();
            Debug.Log("Start 2");

            int playersInGame = sessionManager.activeSession.Players.Count;
            
            // Initialize TurnManager
            // Bullets
            TurnManager.Instance.totalBullets.Value = Bullets;
            TurnManager.Instance.currentBulletsInBarrel.Value = Bullets;
            TurnManager.Instance.bulletPositions.Clear();
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
            Debug.Log("Start 3");

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

        bottle.transform.rotation = Quaternion.identity;
        gun.transform.rotation = Quaternion.identity;
    }

    [ClientRpc]
    void SpinBottleClientRpc(int playerNumber, int spins, int time)
    {
        bottle.GetComponent<Bottle>().SpinBottle(playerNumber, spins, time);
    }
    
}