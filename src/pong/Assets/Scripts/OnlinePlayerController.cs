using Unity.Netcode;
using UnityEngine;
using VContainer;

[RequireComponent(typeof(Rigidbody2D))]
public class OnlinePlayerController : NetworkBehaviour
{
    private IMovePlayerUseCase movePlayerUseCase;
    private PlayerMovedDomainEventHandler playerMovedHandler;

    private Rigidbody2D rb;
    private float inputAxis;

    [Inject]
    public void Construct(IMovePlayerUseCase movePlayerUseCase, PlayerMovedDomainEventHandler playerMovedHandler)
    {
        this.movePlayerUseCase = movePlayerUseCase;
        this.playerMovedHandler = playerMovedHandler;
    }

    private void Awake()
    {
        this.rb = this.GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        playerMovedHandler.PlayerMoved += OnPlayerMoved;
    }

    private void OnDisable()
    {
        playerMovedHandler.PlayerMoved -= OnPlayerMoved;
    }

    private void Update()
    {
        if (!this.IsOwner)
        {
            return;
        }

        this.inputAxis = Input.GetAxisRaw("Player1");
    }

    private void FixedUpdate()
    {
        if (inputAxis == 0f)
        {
            return;
        }

        float newY = this.rb.position.y + inputAxis * GameManager.Instance.PaddleSpeed * Time.fixedDeltaTime;
        var movePlayerCommand = new MovePlayerCommand(GameManager.Instance.CurrentGameId, GameManager.Instance.CurrentPlayer1Id, newY);
        movePlayerUseCase.Execute(movePlayerCommand);
    }

    private void OnPlayerMoved(PlayerMovedDomainEvent domainEvent)
    {
        if (!this.IsOwner)
        {
            return;
        }

        Vector2 newPosition = this.rb.position;
        newPosition.y = domainEvent.NewPlayerPosition.Y;

        this.rb.MovePosition(newPosition);
    }
}
