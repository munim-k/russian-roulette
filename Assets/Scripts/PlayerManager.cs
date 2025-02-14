using Unity.Netcode;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public ulong clientId;
    public string sessionId;
    public GameObject player;

    void Start() {
        clientId = NetworkManager.Singleton.LocalClientId;
    }

}