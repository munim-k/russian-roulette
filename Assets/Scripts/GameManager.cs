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
    [SerializeField] GameObject multiplayerSpawnManager;
    [SerializeField] private GameObject gameScreen;

    [SerializeField] GameObject bottle;
    [SerializeField] GameObject gun;

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
        // shootButton.SetActive(false);
        // bottle.gameObject.SetActive(false);
        // gameScreen.SetActive(false);
        // lobbyUI.SetActive(true);
        // multiplayerSpawnManager.SetActive(false);
        // activeLobbiesUI.SetActive(false);
        // mainMenuUI.SetActive(true);
        // TurnManager.Instance.ResetGame();
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

            // for (int i = 0; i < sessionManager.activeSession.Players.Count; i++)
            // {
            //     // Debug.Log("Player " + i + ": " + sessionManager.activeSession.Players[i].Id);
            // }

            int playersInGame = sessionManager.activeSession.Players.Count;

            // Debug.Log("Players in Game: " + playersInGame);
            
            // Initialize TurnManager
            TurnManager.Instance.totalPlayers.Value = playersInGame;
            TurnManager.Instance.totalBullets = Bullets;
            TurnManager.Instance.currentBulletsInBarrel.Value = Bullets;
            TurnManager.Instance.sessionManager = sessionManager;
            TurnManager.Instance.alivePlayerCount.Value = playersInGame;
            // TurnManager.Instance.currentPlayerId = sessionManager.activeSession.CurrentPlayer.Id;
            // TurnManager.Instance.alivePlayers = new bool[playersInGame];
            Debug.Log(TurnManager.Instance.playerIds);
            for (int i = 0; i < playersInGame; i++)
            {
                // TurnManager.Instance.alivePlayers[i] = true;
                TurnManager.Instance.alivePlayers.Add(true);
                // TurnManager.Instance.playerIds.Value.Add(sessionManager.activeSession.Players[i].Id);
                TurnManager.Instance.playerIds.Add(sessionManager.activeSession.Players[i].Id);
            }

            int firstTurn = Random.Range(0, playersInGame);
            TurnManager.Instance.currentTurnIndex.Value = firstTurn;
            TurnManager.Instance.currentTurn.Value = sessionManager.activeSession.Players[firstTurn].Id;
            SpinBottleClientRpc(firstTurn, Random.Range(1, 5), Random.Range(3, 6));
            TurnManager.Instance.ChangeTurnClientRpc(sessionManager.activeSession.Players[firstTurn].Id);
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
        // bottle.gameObject.SetActive(true);
        bottle.GetComponent<Bottle>().SpinBottle(playerNumber, spins, time);
    }


    
}