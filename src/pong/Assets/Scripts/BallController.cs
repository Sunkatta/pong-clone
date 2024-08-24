using System;
using UnityEngine;

public class BallController : MonoBehaviour
{
    public event Action<PlayerType> GoalPassed;
    public event Action BallHit;

    private new Rigidbody2D rigidbody;
    private AudioSource bounceSound;

    private void Start()
    {
        this.rigidbody = this.GetComponent<Rigidbody2D>();
        this.bounceSound = this.GetComponent<AudioSource>();
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

        // Formula for getting a reflected vector.
        Vector2 newVelocity = this.rigidbody.velocity - 2 * Vector2.Dot(this.rigidbody.velocity, contact.normal) * contact.normal;
        
        this.rigidbody.velocity = newVelocity;

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
