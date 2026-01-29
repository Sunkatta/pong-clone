using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using VContainer;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(AnticipatedNetworkTransform))]
public class OnlinePlayerController : NetworkBehaviour
{
    [SerializeField]
    private float smoothTime = 0.1f;

    [SerializeField]
    private float smoothDistance = 1f;

    private PlayerService playerService;

    private Rigidbody2D rb;
    private AnticipatedNetworkTransform anticipatedTransform;
    private float inputAxis;

    [Inject]
    public void Construct(PlayerService playerService)
    {
        this.playerService = playerService;
    }

    public override void OnNetworkSpawn()
    {
        this.rb = this.GetComponent<Rigidbody2D>();
        this.anticipatedTransform = this.GetComponent<AnticipatedNetworkTransform>();

        this.playerService.PlayerPositionUpdated += OnPlayerMoved;
    }

    public override void OnNetworkDespawn()
    {
        if (this.playerService != null)
        {
            this.playerService.PlayerPositionUpdated -= OnPlayerMoved;
        }
    }

    public override void OnReanticipate(double lastRoundTripTime)
    {
        var previousState = this.anticipatedTransform.PreviousAnticipatedState;

        if (this.smoothTime != 0.0)
        {
            var sqDist = Vector3.SqrMagnitude(previousState.Position - this.anticipatedTransform.AnticipatedState.Position);
            if (sqDist <= 0.25 * 0.25)
            {
                // This prevents small amounts of wobble from slight differences.
                this.anticipatedTransform.AnticipateState(previousState);
            }
            else if (sqDist < this.smoothDistance * this.smoothDistance)
            {
                // Server updates are not necessarily smooth, so applying reanticipation can also result in
                // hitchy, unsmooth animations. To compensate for that, we call this to smooth from the previous
                // anticipated state (stored in "anticipatedValue") to the new state (which, because we have used
                // the "Move" method that updates the anticipated state of the transform, is now the current
                // transform anticipated state)
                this.anticipatedTransform.Smooth(previousState, this.anticipatedTransform.AnticipatedState, this.smoothTime);
            }
        }
    }

    [ServerRpc]
    public void SubmitMoveInputServerRpc(float inputAxis, ServerRpcParams rpcParams = default)
    {
        var clientId = rpcParams.Receive.SenderClientId;
        var player = NetworkManager.Singleton.ConnectedClients[clientId];

        //var rb = player.PlayerObject.GetComponent<Rigidbody2D>();

        this.playerService.HandleMoveInput(clientId, this.anticipatedTransform, inputAxis);
    }

    [ClientRpc]
    public void ReceivePositionClientRpc(float newY, ClientRpcParams _ = default)
    {
        this.anticipatedTransform.AnticipateMove(new Vector3(this.transform.position.x, newY));
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
            this.playerService.HandleMoveInput(NetworkManager.Singleton.LocalClientId, this.anticipatedTransform, this.inputAxis);
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
        if (this.IsHost)
        {
            return;
        }

        this.ReceivePositionClientRpc(newY);
    }
}
