using Unity.Netcode;
using UnityEngine;
using VContainer;

[RequireComponent(typeof(AudioSource))]
public class BallController : NetworkBehaviour
{
    private IMoveBallUseCase moveBallUseCase;
    private IUpdateBallDirectionUseCase updateBallDirectionUseCase;
    private IGetBallDirectionQuery getBallDirectionQuery;
    private PlayerScoredDomainEventHandler playerScoredHandler;
    private BallMovedDomainEventHandler ballMovedHandler;
    private BallDirectionUpdatedDomainEventHandler ballDirectionUpdatedHandler;

    private AudioSource bounceSound;
    private Vector2 ballDirection;

    public float CurrentBallSpeed { get; private set; }

    [Inject]
    public void Construct(IMoveBallUseCase moveBallUseCase,
        IUpdateBallDirectionUseCase updateBallDirectionUseCase,
        IGetBallDirectionQuery getBallDirectionQuery,
        PlayerScoredDomainEventHandler playerScoredHandler,
        BallMovedDomainEventHandler ballMovedHandler,
        BallDirectionUpdatedDomainEventHandler ballDirectionUpdatedHandler) 
    {
        this.moveBallUseCase = moveBallUseCase;
        this.updateBallDirectionUseCase = updateBallDirectionUseCase;
        this.getBallDirectionQuery = getBallDirectionQuery;
        this.playerScoredHandler = playerScoredHandler;
        this.ballMovedHandler = ballMovedHandler;
        this.ballDirectionUpdatedHandler = ballDirectionUpdatedHandler;
    }

    public override void OnNetworkSpawn()
    {
        if (!this.IsServer)
        {
            return;
        }

        this.Setup();
        base.OnNetworkSpawn();
    }

    private void OnEnable()
    {
        if (this.NoOnlineAuthority())
        {
            return;
        }

        this.Setup();
    }

    private void OnDisable()
    {
        if (this.NoOnlineAuthority())
        {
            return;
        }

        this.ballMovedHandler.BallMoved -= OnBallMoved;
        this.ballDirectionUpdatedHandler.BallDirectionUpdated -= OnBallDirectionUpdated;
        this.playerScoredHandler.PlayerScored -= OnPlayerScored;
    }

    public void Move()
    {
        if (this.NoOnlineAuthority())
        {
            return;
        }

        Vector3 nextPosition = this.transform.position + (Vector3)(this.CurrentBallSpeed * Time.deltaTime * this.ballDirection);
        this.moveBallUseCase.Execute(new MoveBallCommand(GameManager.Instance.CurrentGameId, (nextPosition.x, nextPosition.y)));
    }

    private void Setup()
    {
        this.ballMovedHandler.BallMoved += OnBallMoved;
        this.ballDirectionUpdatedHandler.BallDirectionUpdated += OnBallDirectionUpdated;
        this.playerScoredHandler.PlayerScored += OnPlayerScored;
        (float x, float y) = this.getBallDirectionQuery.Execute(GameManager.Instance.CurrentGameId, GameManager.Instance.CurrentBallId);
        this.ballDirection = new Vector2(x, y);
    }

    private void Start()
    {
        this.bounceSound = this.GetComponent<AudioSource>();
        this.CurrentBallSpeed = GameManager.Instance.BallInitialSpeed;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (this.NoOnlineAuthority())
        {
            return;
        }

        this.bounceSound.Play();

        ContactPoint2D contact = collision.GetContact(0);

        var newDirection = Vector2.Reflect(this.ballDirection, contact.normal);
        bool isHitByPlayer = false;

        if (collision.gameObject.CompareTag(Constants.Player))
        {
            isHitByPlayer = true;
        }

        this.updateBallDirectionUseCase.Execute(new UpdateBallDirectionCommand(GameManager.Instance.CurrentGameId, (newDirection.x, newDirection.y), isHitByPlayer));
    }

    private bool NoOnlineAuthority() => GameManager.Instance.CurrentGameType == GameType.OnlinePvp && !this.IsServer;

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawLine(this.transform.position, new Vector2(1f, 1f));
        Gizmos.DrawLine(this.transform.position, new Vector2(1f, -1f));
        Gizmos.DrawLine(this.transform.position, new Vector2(-1f, -1f));
        Gizmos.DrawLine(this.transform.position, new Vector2(-1f, 1f));
    }

    private void OnBallMoved(BallMovedDomainEvent ballMovedDomainEvent)
    {
        this.transform.position = new Vector3(ballMovedDomainEvent.NewPosition.X, ballMovedDomainEvent.NewPosition.Y, this.transform.position.z);
    }

    private void OnBallDirectionUpdated(BallDirectionUpdatedDomainEvent ballDirectionUpdatedDomainEvent)
    {
        this.ballDirection = new Vector2(ballDirectionUpdatedDomainEvent.NewDirection.X, ballDirectionUpdatedDomainEvent.NewDirection.Y);
        this.CurrentBallSpeed = ballDirectionUpdatedDomainEvent.NewSpeed;
    }

    private void OnPlayerScored(PlayerScoredDomainEvent playerScoredDomainEvent)
    {
        this.updateBallDirectionUseCase.Execute(new UpdateBallDirectionCommand(GameManager.Instance.CurrentGameId, playerScoredDomainEvent.PlayerType));
    }
}
