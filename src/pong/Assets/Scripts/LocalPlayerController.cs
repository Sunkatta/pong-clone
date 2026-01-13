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
        this.inputAxis = Type == PlayerType.Player1
            ? Input.GetAxisRaw("Player1")
            : Input.GetAxisRaw("Player2");
    }

    private void FixedUpdate()
    {
        if (inputAxis == 0f)
        {
            return;
        }

        float newY = this.rb.position.y + inputAxis * GameManager.Instance.PaddleSpeed * Time.fixedDeltaTime;
        var movePlayerCommand = new MovePlayerCommand(GameManager.Instance.CurrentGameId, Id, newY);
        movePlayerUseCase.Execute(movePlayerCommand);
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
