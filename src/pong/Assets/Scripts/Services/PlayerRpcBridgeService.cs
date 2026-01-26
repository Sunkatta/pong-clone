using Unity.Netcode;
using UnityEngine;
using VContainer;

public class PlayerRpcBridgeService : NetworkBehaviour
{
    private PlayerService playerService;

    [Inject]
    public void Construct(PlayerService playerService)
    {
        this.playerService = playerService;
    }

    public static PlayerRpcBridgeService Instance { get; private set; }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            Instance = this;
        }
    }

    [ServerRpc]
    public void SubmitMoveInputServerRpc(float inputAxis, ServerRpcParams rpcParams = default)
    {
        var clientId = rpcParams.Receive.SenderClientId;
        var player = NetworkManager.Singleton.ConnectedClients[clientId];

        var rb = player.PlayerObject.GetComponent<Rigidbody2D>();

        this.playerService.HandleMoveInput(clientId, rb, inputAxis);
    }

    [ClientRpc]
    public void ReceivePositionClientRpc(float newY, ClientRpcParams _ = default)
    {
        this.playerService.ApplyRemotePosition(newY);
    }

    public static void SendPositionToClient(ulong clientId, float newY)
    {
        Instance.ReceivePositionClientRpc(
            newY,
            new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new[] { clientId }
                }
            });
    }
}
