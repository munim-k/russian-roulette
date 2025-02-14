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