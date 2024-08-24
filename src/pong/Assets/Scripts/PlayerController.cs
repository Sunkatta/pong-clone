using System;
using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    public static event Action<PlayerController> PlayerJoined;
    public event Action<PlayerType> PlayerScored;

    public PlayerType Type { get; private set; }

    public NetworkVariable<int> Score { get; set; } = new NetworkVariable<int>();

    [SerializeField]
    private float speed;

    [SerializeField]
    private GameObject ball;

    private bool canMoveUp = true;
    private bool canMoveDown = true;

    public override void OnNetworkSpawn()
    {
        this.Type = PlayerType.Player1;

        if (NetworkManager.Singleton.ConnectedClients.Count > 1)
        {
            this.Type = PlayerType.Player2;
        }

        PlayerJoined(this);
    }

    private void Start()
    {
        var ballController = this.ball.GetComponent<BallController>();
        ballController.GoalPassed += this.OnGoalPassed;
    }

    private void Update()
    {
        if (!this.IsOwner)
        {
            return;
        }

        var playerAxis = Input.GetAxis("Player1");

        if (playerAxis > 0 && this.canMoveUp)
        {
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

    private void OnGoalPassed(PlayerType scorer)
    {
        this.Score.Value++;
        this.PlayerScored(scorer);
    }
}
