using Unity.Netcode;
using UnityEngine;
using VContainer;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerRpcBridgeService))]
public class OnlinePlayerController : NetworkBehaviour
{
    private PlayerService playerService;
    private PlayerRpcBridgeService rpcBridge;

    private Rigidbody2D rb;
    private float inputAxis;

    [Inject]
    public void Construct(PlayerService playerService)
    {
        this.playerService = playerService;
    }

    public override void OnNetworkSpawn()
    {
        this.rb = this.GetComponent<Rigidbody2D>();
        this.rpcBridge = this.GetComponent<PlayerRpcBridgeService>();

        this.playerService.PlayerPositionUpdated += OnPlayerMoved;
    }

    public override void OnNetworkDespawn()
    {
        if (this.playerService != null)
        {
            this.playerService.PlayerPositionUpdated -= OnPlayerMoved;
        }
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

        this.rpcBridge.SubmitMoveInputServerRpc(this.inputAxis);
    }

    private void OnPlayerMoved(float newY)
    {
        Vector2 newPosition = this.rb.position;
        newPosition.y = newY;

        this.rb.MovePosition(newPosition);
    }
}
