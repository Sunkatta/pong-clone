using System;
using UnityEngine;

public class BallController : MonoBehaviour
{
    public event Action<PlayerType> playerScored;

    private new Rigidbody2D rigidbody;

    void Start()
    {
        this.rigidbody = this.GetComponent<Rigidbody2D>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag(Constants.RightGoal))
        {
            this.playerScored(PlayerType.Player1);
            return;
        }

        if (collision.gameObject.CompareTag(Constants.LeftGoal))
        {
            this.playerScored(PlayerType.Player2);
            return;
        }

        ContactPoint2D contact = collision.GetContact(0);

        // Formula for getting a reflected vector.
        Vector2 newVelocity = this.rigidbody.velocity - 2 * Vector2.Dot(this.rigidbody.velocity, contact.normal) * contact.normal;
        
        this.rigidbody.velocity = newVelocity;
    }
}
