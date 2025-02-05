using UnityEngine;

public class GameManager : MonoBehaviour
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

    public void Bullet1()
    {
        Bullets = 1;
    }
    public void Bullet2()
    {
        Bullets = 2;
    }public void Bullet3()
    {
        Bullets = 3;
    }public void Bullet4()
    {
        Bullets = 4;
    }public void Bullet5()
    {
        Bullets = 5;
    }public void Bullet6()
    {
        Bullets = 6;
    }
    public void Player1()
    {
        Players = 1;
    }
    public void Player2()
    {
        Players = 2;
    }
    public void Player3()
    {
        Players = 3;
    }
    public void Player4()
    {
        Players = 4;
    }
    public void Player5()
    {
        Players = 5;
    }
    public void Player6()
    {
        Players = 6;
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
        Invoke(nameof(activateLobby),5f);
    }
    void activateLobby()
    {
        lobbyUI.SetActive(true);
    }
}
