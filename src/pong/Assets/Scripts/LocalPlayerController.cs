using UnityEngine;
using VContainer;

public class LocalPlayerController : MonoBehaviour
{
    private IMovePlayerUseCase movePlayerUseCase;
    private PlayerMovedDomainEventHandler playerMovedHandler;

    public PlayerType Type { get; set; }

    public string Id { get; set; }

    [SerializeField]
    private float speed;

    [Inject]
    public void Construct(IMovePlayerUseCase movePlayerUseCase, PlayerMovedDomainEventHandler playerMovedHandler)
    {
        this.movePlayerUseCase = movePlayerUseCase;
        this.playerMovedHandler = playerMovedHandler;
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
        var playerAxis = this.Type == PlayerType.Player1 ? Input.GetAxisRaw("Player1") : Input.GetAxisRaw("Player2");
        var position = this.transform.position;

        if (playerAxis > 0)
        {
            position.y += speed * Time.deltaTime;
            var movePlayerCommand = new MovePlayerCommand("1", Id, position.y);
            this.movePlayerUseCase.Execute(movePlayerCommand);
        }
        else if (playerAxis < 0)
        {
            position.y -= speed * Time.deltaTime;
            var movePlayerCommand = new MovePlayerCommand("1", Id, position.y);
            this.movePlayerUseCase.Execute(movePlayerCommand);
        }
    }

    private void OnPlayerMoved(PlayerMovedDomainEvent domainEvent)
    {
        if (domainEvent.PlayerId != Id)
        {
            return;
        }

        var position = this.transform.position;
        position.y = domainEvent.NewPlayerPosition.Y;
        this.transform.position = position;
    }
}
