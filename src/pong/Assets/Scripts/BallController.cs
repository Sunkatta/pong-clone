using UnityEngine;

public class BallController : MonoBehaviour
{
    public float speed;

    private new Rigidbody2D rigidbody;

    void Start()
    {
        this.rigidbody = this.GetComponent<Rigidbody2D>();
        this.rigidbody.velocity = speed * new Vector2(1.5f, 1.5f);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        ContactPoint2D contact = collision.GetContact(0);

        // Formula for getting a reflected vector.
        Vector2 newVelocity = this.rigidbody.velocity - 2 * Vector2.Dot(this.rigidbody.velocity, contact.normal) * contact.normal;
        
        this.rigidbody.velocity = newVelocity;
    }
}
