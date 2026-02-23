using UnityEngine;
using VContainer;

[RequireComponent(typeof(Rigidbody2D))]
public class LocalPlayerController : MonoBehaviour
{
    private IMovePlayerUseCase movePlayerUseCase;
    private PlayerMovedDomainEventHandler playerMovedHandler;

    public PlayerType Type { get; set; }

    public string Id { get; set; }

    private Rigidbody2D rb;
    private PlayerInputActions playerInputActions;
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
        this.playerInputActions = new PlayerInputActions();
        this.playerInputActions.Player.Enable();
    }

    private void OnEnable()
    {
        this.playerMovedHandler.PlayerMoved += OnPlayerMoved;
    }

    private void OnDisable()
    {
        this.playerMovedHandler.PlayerMoved -= OnPlayerMoved;
    }

    private void Update()
    {
        this.inputAxis = this.Type == PlayerType.Player1
            ? this.playerInputActions.Player.PrimaryMovement.ReadValue<Vector2>().y
            : this.playerInputActions.Player.SecondaryMovement.ReadValue<Vector2>().y;
    }

    private void FixedUpdate()
    {
        if (this.inputAxis == 0f)
        {
            return;
        }

        float newY = this.rb.position.y + this.inputAxis * GameManager.Instance.PaddleSpeed * Time.fixedDeltaTime;
        var movePlayerCommand = new MovePlayerCommand(GameManager.Instance.CurrentGameId, Id, newY);
        this.movePlayerUseCase.Execute(movePlayerCommand);
    }

    private void OnPlayerMoved(PlayerMovedDomainEvent domainEvent)
    {
        if (domainEvent.PlayerId != Id)
        {
            return;
        }

        Vector2 newPosition = this.rb.position;
        newPosition.y = domainEvent.NewPlayerPosition.Y;

        this.rb.MovePosition(newPosition);
    }
}
