using System;
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

    public event Action<PlayerType> GoalPassed;
    public event Action BallHit;

    [SerializeField]
    private float initialBallSpeed;

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

    private void OnEnable()
    {
        this.ballMovedHandler.BallMoved += OnBallMoved;
        this.ballDirectionUpdatedHandler.BallDirectionUpdated += OnBallDirectionUpdated;
        this.playerScoredHandler.PlayerScored += OnPlayerScored;
        (float x, float y) = this.getBallDirectionQuery.Execute("1", "1");
        this.ballDirection = new Vector2(x, y);
    }

    private void OnDisable()
    {
        this.ballMovedHandler.BallMoved -= OnBallMoved;
        this.ballDirectionUpdatedHandler.BallDirectionUpdated -= OnBallDirectionUpdated;
    }

    public void Move()
    {
        Vector3 nextPosition = this.transform.position + (Vector3)(this.CurrentBallSpeed * Time.deltaTime * this.ballDirection);
        this.moveBallUseCase.Execute(new MoveBallCommand("1", (nextPosition.x, nextPosition.y)));
    }

    public void UpdateSpeed(float newSpeed)
    {
        this.CurrentBallSpeed = newSpeed;
    }

    public void UpdateBallDirection(Vector2 newDirection)
    {
        this.ballDirection = newDirection;
    }

    public void ResetBall()
    {
        this.CurrentBallSpeed = this.initialBallSpeed;
        this.transform.position = Vector3.zero;
    }

    private void Start()
    {
        this.bounceSound = this.GetComponent<AudioSource>();
        this.CurrentBallSpeed = initialBallSpeed;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        this.bounceSound.Play();

        ContactPoint2D contact = collision.GetContact(0);

        var newDirection = Vector2.Reflect(this.ballDirection, contact.normal);
        bool isHitByPlayer = false;

        if (collision.gameObject.CompareTag(Constants.Player))
        {
            isHitByPlayer = true;
        }

        this.updateBallDirectionUseCase.Execute(new UpdateBallDirectionCommand("1", (newDirection.x, newDirection.y), isHitByPlayer));
    }

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
        this.updateBallDirectionUseCase.Execute(new UpdateBallDirectionCommand("1", playerScoredDomainEvent.PlayerType));
    }
}
