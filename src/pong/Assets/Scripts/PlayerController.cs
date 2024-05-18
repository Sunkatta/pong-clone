using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float speed = 2f;

    private bool canMoveUp = true;
    private bool canMoveDown = true;

    private void Update()
    {
        var verticalAxis = Input.GetAxis("Vertical");

        if (verticalAxis > 0 && this.canMoveUp)
        {
            this.transform.position += speed * Time.deltaTime * Vector3.up;
            this.canMoveDown = true;
        }
        else if (verticalAxis < 0 && this.canMoveDown)
        {
            this.transform.position -= speed * Time.deltaTime * Vector3.up;
            this.canMoveUp = true;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag(Constants.UpperEdge))
        {
            this.canMoveUp = false;
        }

        if (collision.gameObject.CompareTag(Constants.LowerEdge))
        {
            this.canMoveDown = false;
        }
    }
}
