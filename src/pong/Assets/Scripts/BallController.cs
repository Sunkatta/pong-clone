using System;
using Unity.Netcode;
using UnityEngine;

public class BallController : NetworkBehaviour
{
    public event Action<PlayerType> GoalPassed;
    public event Action BallHit;

    [SerializeField]
    private float initialBallSpeed;

    private AudioSource bounceSound;
    private Vector2 ballDirection;

    public float CurrentBallSpeed { get; private set; }

    public void Move()
    {
        this.transform.position += this.CurrentBallSpeed * Time.deltaTime * (Vector3)this.ballDirection;
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
        if (collision.gameObject.CompareTag(Constants.RightGoal))
        {
            this.GoalPassed(PlayerType.Player1);
            return;
        }

        if (collision.gameObject.CompareTag(Constants.LeftGoal))
        {
            this.GoalPassed(PlayerType.Player2);
            return;
        }

        this.bounceSound.Play();

        ContactPoint2D contact = collision.GetContact(0);

        this.ballDirection = Vector2.Reflect(this.ballDirection, contact.normal);

        if (collision.gameObject.CompareTag(Constants.Player))
        {
            this.BallHit();
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawLine(this.transform.position, new Vector2(1f, 1f));
        Gizmos.DrawLine(this.transform.position, new Vector2(1f, -1f));
        Gizmos.DrawLine(this.transform.position, new Vector2(-1f, -1f));
        Gizmos.DrawLine(this.transform.position, new Vector2(-1f, 1f));
    }
}
