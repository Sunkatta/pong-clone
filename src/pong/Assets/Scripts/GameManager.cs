using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public new Camera camera;
    public GameObject ball;
    public GameObject player1;
    public GameObject player2;
    public TMP_Text player1ScoreText;
    public TMP_Text player2ScoreText;
    public float ballSpeed;

    private int player1Score = 0;
    private int player2Score = 0;
    private Rigidbody2D ballRigidbody;

    private void Start()
    {
        this.ballRigidbody = this.ball.GetComponent<Rigidbody2D>();
        var ballController = this.ball.GetComponent<BallController>();
        ballController.PlayerScored += this.OnPlayerScored;

        this.GenerateCollidersAcrossScreen();
        this.SetInitialGameState();
    }

    private void SetInitialGameState()
    {
        this.ball.transform.position = Vector3.zero;
        this.ballRigidbody.velocity = this.ballSpeed * new Vector2(1f, 0f);

        this.player1.transform.position = new Vector3(this.player1.transform.position.x, 0);
        this.player2.transform.position = new Vector3(this.player2.transform.position.x, 0);
    }

    private void OnPlayerScored(PlayerType playerType)
    {
        if (playerType == PlayerType.Player1)
        {
            this.player1Score++;
            this.player1ScoreText.text = this.player1Score.ToString();
        }
        else
        {
            this.player2Score++;
            this.player2ScoreText.text = this.player2Score.ToString();
        }

        this.SetInitialGameState();
    }

    private void GenerateCollidersAcrossScreen()
    {
        Vector2 lDCorner = this.camera.ViewportToWorldPoint(new Vector3(0, 0f, this.camera.nearClipPlane));
        Vector2 rUCorner = this.camera.ViewportToWorldPoint(new Vector3(1f, 1f, this.camera.nearClipPlane));
        Vector2[] colliderpoints;

        var upperEdgeGameObject = new GameObject(Constants.UpperEdge);
        upperEdgeGameObject.tag = Constants.UpperEdge;
        EdgeCollider2D upperEdge = upperEdgeGameObject.AddComponent<EdgeCollider2D>();
        colliderpoints = upperEdge.points;
        colliderpoints[0] = new Vector2(lDCorner.x, rUCorner.y);
        colliderpoints[1] = new Vector2(rUCorner.x, rUCorner.y);
        upperEdge.points = colliderpoints;

        var lowerEdgeGameObject = new GameObject(Constants.LowerEdge);
        lowerEdgeGameObject.tag = Constants.LowerEdge;
        EdgeCollider2D lowerEdge = lowerEdgeGameObject.AddComponent<EdgeCollider2D>();
        colliderpoints = lowerEdge.points;
        colliderpoints[0] = new Vector2(lDCorner.x, lDCorner.y);
        colliderpoints[1] = new Vector2(rUCorner.x, lDCorner.y);
        lowerEdge.points = colliderpoints;

        var leftGoalGameObject = new GameObject(Constants.LeftGoal);
        leftGoalGameObject.tag = Constants.LeftGoal;
        EdgeCollider2D leftGoalCollider = leftGoalGameObject.AddComponent<EdgeCollider2D>();
        colliderpoints = leftGoalCollider.points;
        colliderpoints[0] = new Vector2(lDCorner.x, lDCorner.y);
        colliderpoints[1] = new Vector2(lDCorner.x, rUCorner.y);
        leftGoalCollider.points = colliderpoints;

        var rightGoalGameObject = new GameObject(Constants.RightGoal);
        rightGoalGameObject.tag = Constants.RightGoal;
        EdgeCollider2D rightGoalCollider = rightGoalGameObject.AddComponent<EdgeCollider2D>();
        colliderpoints = rightGoalCollider.points;
        colliderpoints[0] = new Vector2(rUCorner.x, rUCorner.y);
        colliderpoints[1] = new Vector2(rUCorner.x, lDCorner.y);
        rightGoalCollider.points = colliderpoints;
    }
}
