using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public event Action<PlayerType> GameEnded;

    [SerializeField]
    private new Camera camera;

    [SerializeField]
    private GameObject ball;

    [SerializeField]
    private TMP_Text player1ScoreText;

    [SerializeField]
    private TMP_Text player2ScoreText;

    [SerializeField]
    private float initialBallSpeed;

    [SerializeField]
    private float maxBallSpeed;

    [SerializeField]
    private int targetScore;

    private float currentBallSpeed;
    private Rigidbody2D ballRigidbody;
    private PlayerType? latestScorer;
    private AudioSource goalSound;

    private PlayerController player1;
    private PlayerController player2;

    public void NewGame()
    {
        this.player1.Score.Value = 0;
        this.player2.Score.Value = 0;
        this.player1ScoreText.text = "0";
        this.player2ScoreText.text = "0";

        this.SetInitialGameState();
    }

    private void Start()
    {
        this.goalSound = GetComponent<AudioSource>();
        this.ballRigidbody = this.ball.GetComponent<Rigidbody2D>();
        var ballController = this.ball.GetComponent<BallController>();
        ballController.PlayerScored += this.OnPlayerScored;
        ballController.BallHit += this.OnBallHit;
        PlayerController.PlayerJoined += this.OnPlayerJoined;

        this.GenerateCollidersAcrossScreen();
    }

    private void SetInitialGameState()
    {
        this.currentBallSpeed = this.initialBallSpeed;
        this.ball.transform.position = Vector3.zero;
        this.ballRigidbody.velocity = this.currentBallSpeed * GetBallDirection();
    }

    private Vector2 GetBallDirection()
    {
        if (this.latestScorer != null)
        {
            var isPlayer1 = this.latestScorer == PlayerType.Player1;

            return new Vector2(isPlayer1 ? 1 : -1, UnityEngine.Random.Range(-1f, 1f));
        }
        else
        {
            return new Vector2(UnityEngine.Random.value < 0.5 ? -1 : 1, UnityEngine.Random.Range(-1f, 1f));
        }
    }

    private void OnPlayerScored(PlayerType scorer)
    {
        this.goalSound.Play();
        this.latestScorer = scorer;

        if (scorer == PlayerType.Player1)
        {
            this.player1ScoreText.text = this.player1.Score.Value.ToString();
        }
        else
        {
            this.player2ScoreText.text = this.player2.Score.Value.ToString();
        }

        if (this.player1.Score.Value == targetScore || this.player2.Score.Value == targetScore)
        {
            var winner = this.player1.Score.Value == targetScore ? PlayerType.Player1 : PlayerType.Player2;

            this.GameEnded(winner);

            return;
        }

        this.SetInitialGameState();
    }

    private void OnBallHit()
    {
        if (this.currentBallSpeed >= this.maxBallSpeed)
        {
            return;
        }

        var oldSpeed = this.currentBallSpeed;
        this.currentBallSpeed++;

        this.ballRigidbody.velocity *= this.currentBallSpeed / oldSpeed;
    }

    private void OnPlayerJoined(PlayerController player)
    {
        if (this.player1 == null)
        {
            this.player1 = player;
            this.SetPlayerPosition(this.player1);
        }
        else
        {
            this.player2 = player;
            this.SetPlayerPosition(this.player2);
            this.StartCoroutine(this.BeginGame());
        }
    }

    private void SetPlayerPosition(PlayerController player)
    {
        Vector3 screenLeftSide = this.camera.ScreenToWorldPoint(new Vector2(0, Screen.height / 2));
        Vector3 screenRightSide = this.camera.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height / 2));

        if (player.Type == PlayerType.Player1)
        {
            player.transform.position = new Vector3(screenLeftSide.x + .5f, 0);
        }
        else
        {
            player.transform.position = new Vector3(screenRightSide.x - .5f, 0);
        }
    }

    private IEnumerator BeginGame()
    {
        yield return new WaitForSeconds(5);
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
