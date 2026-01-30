using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using VContainer;

[RequireComponent(typeof(AnticipatedNetworkTransform))]
public class OnlinePlayerController : NetworkBehaviour
{
    private PlayerService playerService;

    private AnticipatedNetworkTransform anticipatedTransform;
    private float inputAxis;

    [Inject]
    public void Construct(PlayerService playerService)
    {
        this.playerService = playerService;
    }

    public override void OnNetworkSpawn()
    {
        this.anticipatedTransform = this.GetComponent<AnticipatedNetworkTransform>();
        this.anticipatedTransform.enabled = IsOwner;

        this.playerService.PlayerPositionUpdated += OnPlayerMoved;
    }

    public override void OnNetworkDespawn()
    {
        if (this.playerService != null)
        {
            this.playerService.PlayerPositionUpdated -= OnPlayerMoved;
        }
    }

    [ServerRpc]
    public void SubmitMoveInputServerRpc(float inputAxis, ServerRpcParams rpcParams = default)
    {
        var clientId = rpcParams.Receive.SenderClientId;
        this.playerService.HandleMoveInput(clientId, this.transform, inputAxis);
    }

    private void Update()
    {
        this.inputAxis = Input.GetAxisRaw("Player1");
    }

    private void FixedUpdate()
    {
        if (inputAxis == 0f)
        {
            return;
        }

        if (!this.IsOwner)
        {
            return;
        }

        if (this.IsHost)
        {
            // If this is running on the Host (Server), skip RPC requests.
            this.playerService.HandleMoveInput(NetworkManager.Singleton.LocalClientId, this.transform, this.inputAxis);
            return;
        }

        // Move Client locally so that it doesn't rely on receiving RPC response, resulting in feeling slow.
        var newY = this.transform.position.y + inputAxis * GameManager.Instance.PaddleSpeed * Time.fixedDeltaTime;
        this.anticipatedTransform.AnticipateMove(new Vector3(this.transform.position.x, newY));

        // Send input to server asynchronously
        this.SubmitMoveInputServerRpc(this.inputAxis);
    }

    private void OnPlayerMoved(float newY, ulong clientId)
    {
        if (OwnerClientId != clientId)
        {
            return;
        }

        // Case 2: Host owner (local host player)
        if (IsServer && IsOwner)
        {
            // Apply authoritative domain event directly
            var pos = this.transform.position;
            pos.y = newY;
            this.transform.position = pos;
            return;
        }

        // Case 3: Non-host owner (client)
        if (!IsHost && IsOwner)
        {
            // Reconcile predicted ANT position
            var authoritative = this.anticipatedTransform.AuthoritativeState;
            authoritative.Position = new Vector3(authoritative.Position.x, newY, authoritative.Position.z);
            this.anticipatedTransform.AnticipateState(authoritative);
            return;
        }
    }
}
