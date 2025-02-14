using UnityEngine;
using Unity.Netcode;

public class Player : NetworkBehaviour
{
    [SerializeField] GameObject bangLogo;
    [SerializeField] ulong clientId;
    public void SetClientId(ulong id) {
        this.clientId = id;
    }
    public ulong GetClientId() {
        return clientId;
    }
    [SerializeField] string sessionId;
    public void SetSessionId(string id) {
        this.sessionId = id;
    }
    public string GetSessionId() {
        return sessionId;
    }

    public void Die() {
        gameObject.GetComponent<SpriteRenderer>().color = Color.red;
        bangLogo.SetActive(true);
        Invoke(nameof(HideBangLogo), 2f);
    }

    void HideBangLogo() {
        bangLogo.SetActive(false);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsOwner || IsLocalPlayer) {
            gameObject.GetComponent<SpriteRenderer>().color = Color.green;
            GameObject.FindAnyObjectByType<PlayerManager>().player = gameObject;
        }
    }
}