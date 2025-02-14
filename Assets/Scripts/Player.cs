using UnityEngine;
using Unity.Netcode;

public class Player : NetworkBehaviour
{
    [SerializeField] GameObject bangLogo;
    [SerializeField] ulong clientId;
    [SerializeField] private GameObject clickLogo;
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

    public void Click()
    {
        clickLogo.SetActive(true);
        Invoke(nameof(HideClickLogo), 1f);
    }
    
    void HideClickLogo()
    {
        clickLogo.SetActive(false);
    }
    public void Die() {
        gameObject.GetComponent<SpriteRenderer>().color = Color.red;
        bangLogo.SetActive(true);
        Invoke(nameof(HideBangLogo), 1f);
    }

    void HideBangLogo() {
        bangLogo.SetActive(false);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsOwner || IsLocalPlayer) {
            sessionId = TurnManager.Instance.currentPlayerId;
            gameObject.GetComponent<SpriteRenderer>().color = Color.green;
            GameObject.FindAnyObjectByType<PlayerManager>().player = gameObject;
        }
        clientId = OwnerClientId;
    }

    [ClientRpc]
    public void SetupClientRpc(ulong clientId, string sessionId) {
        if (clientId == OwnerClientId) {
            this.clientId = clientId;
            this.sessionId = sessionId;
        }
    }
}