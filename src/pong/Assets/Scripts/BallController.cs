using System;
using Unity.Netcode;
using UnityEngine;
using VContainer;

public class BallController : NetworkBehaviour
{
    private IMoveBallUseCase moveBallUseCase;
    private IUpdateBallDirectionUseCase updateBallDirectionUseCase;
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
        BallMovedDomainEventHandler ballMovedHandler,
        BallDirectionUpdatedDomainEventHandler ballDirectionUpdatedHandler) 
    {
        this.moveBallUseCase = moveBallUseCase;
        this.updateBallDirectionUseCase = updateBallDirectionUseCase;
        this.ballMovedHandler = ballMovedHandler;
        this.ballDirectionUpdatedHandler = ballDirectionUpdatedHandler;
    }

    private void OnEnable()
    {
        this.ballMovedHandler.BallMoved += OnBallMoved;
        this.ballDirectionUpdatedHandler.BallDirectionUpdated += OnBallDirectionUpdated;
    }

    private void OnDisable()
    {
        this.ballMovedHandler.BallMoved -= OnBallMoved;
        this.ballDirectionUpdatedHandler.BallDirectionUpdated -= OnBallDirectionUpdated;
    }

    public void Move()
    {
        var newPosition = this.CurrentBallSpeed * Time.deltaTime * (Vector3)this.ballDirection;
        this.moveBallUseCase.Execute(new MoveBallCommand("1", (newPosition.x, newPosition.y)));
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
        //if (collision.gameObject.CompareTag(Constants.RightGoal))
        //{
        //    this.GoalPassed(PlayerType.Player1);
        //    return;
        //}

        //if (collision.gameObject.CompareTag(Constants.LeftGoal))
        //{
        //    this.GoalPassed(PlayerType.Player2);
        //    return;
        //}

        this.bounceSound.Play();

        ContactPoint2D contact = collision.GetContact(0);

        var newDirection = Vector2.Reflect(this.ballDirection, contact.normal);
        bool isHitByPlayer = false;

        if (collision.gameObject.CompareTag(Constants.Player))
        {
            // this.BallHit();
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
        this.transform.position += new Vector3(ballMovedDomainEvent.NewPosition.X, ballMovedDomainEvent.NewPosition.Y);
    }

    private void OnBallDirectionUpdated(BallDirectionUpdatedDomainEvent ballDirectionUpdatedDomainEvent)
    {
        this.ballDirection = new Vector2(ballDirectionUpdatedDomainEvent.NewDirection.X, ballDirectionUpdatedDomainEvent.NewDirection.Y);
        this.CurrentBallSpeed = ballDirectionUpdatedDomainEvent.NewSpeed;
    }
}
