using UnityEngine;

public class LocalPlayerController : MonoBehaviour
{
    //private readonly IMovePlayerUseCase movePlayerUseCase = new MovePlayerUseCase();

    public PlayerType Type { get; set; }

    [SerializeField]
    private float speed;

    private bool canMoveUp = true;
    private bool canMoveDown = true;

    private void Update()
    {
        var playerAxis = this.Type == PlayerType.Player1 ? Input.GetAxisRaw("Player1") : Input.GetAxisRaw("Player2");

        if (playerAxis > 0 && this.canMoveUp)
        {
            //var movePlayerCommand = new MovePlayerCommand();
            //this.movePlayerUseCase.Execute(movePlayerCommand);
            this.transform.position += speed * Time.deltaTime * Vector3.up;
            this.canMoveDown = true;
        }
        else if (playerAxis < 0 && this.canMoveDown)
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
