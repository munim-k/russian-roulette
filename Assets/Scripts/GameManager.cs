using UnityEngine;
using Unity.Netcode;
public class GameManager : NetworkBehaviour
{

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

    [SerializeField] Bottle bottle;

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

            int playersInGame = sessionManager.activeSession.Players.Count;
            int firstTurn = Random.Range(0, playersInGame);

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
        bottle.SpinBottle(playerNumber, spins, time);
    }
    
}