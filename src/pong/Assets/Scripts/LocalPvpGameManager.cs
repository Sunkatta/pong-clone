using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LocalPvpGameManager : MonoBehaviour, IGameManager
{
    public static event Action MainMenuLoaded;
    public static event Action<int, PlayerType> ScoreChanged;
    public static event Action<string, string> MatchEnded;

    public int Player1Score { get; set; }

    public int Player2Score { get; set; }

    [SerializeField]
    private GameObject ballPrefab;

    [SerializeField]
    private GameObject playerPrefab;

    [SerializeField]
    private float initialBallSpeed;

    [SerializeField]
    private float maxBallSpeed;

    [SerializeField]
    private int targetScore;

    private float currentBallSpeed;
    private PlayerType? latestScorer;
    private Rigidbody2D ballRigidbody;
    private AudioSource goalSound;
    private GameObject ball;

    private readonly List<LocalPlayer> players = new List<LocalPlayer>();
    private readonly List<GameObject> fieldEdges = new List<GameObject>();

    public void BeginGame()
    {
        StartCoroutine(this.BeginGameCouroutine());
    }

    public void OnPlayerJoined(LocalPlayer player)
    {
        this.players.Add(player);

        var playerGameObject = Instantiate(this.playerPrefab);
        var playerInstanceController = playerGameObject.GetComponent<LocalPlayerController>();
        playerInstanceController.Type = player.PlayerType;
        playerGameObject.transform.position = this.GetPlayerPosition(player);
    }

    public void OnPlayerLeft(string playerId)
    {
        throw new NotImplementedException();
    }

    private void EndGame(string winnerName, string loserName)
    {
        this.StartCoroutine(this.MatchEndedCouroutine(winnerName, loserName));
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

    private void OnPlayerScored(PlayerType scorer)
    {
        this.goalSound.Play();

        this.latestScorer = scorer;

        if (scorer == PlayerType.Player1)
        {
            this.Player1Score++;
            ScoreChanged(this.Player1Score, PlayerType.Player1);
        }
        else
        {
            this.Player2Score++;
            ScoreChanged(this.Player2Score, PlayerType.Player2);
        }

        if (this.Player1Score == targetScore || this.Player2Score == targetScore)
        {
            var winnerType = this.Player1Score == targetScore ? PlayerType.Player1 : PlayerType.Player2;
            var winnerPlayer = this.players.First(player => player.PlayerType == winnerType);
            var loserPlayer = this.players.First(player => player.PlayerType != winnerType);

            Destroy(this.ball);
            this.EndGame(winnerPlayer.Username, loserPlayer.Username);

            return;
        }

        this.SetInitialGameState();
    }

    private void Start()
    {
        this.goalSound = GetComponent<AudioSource>();
        this.GenerateCollidersAcrossScreen();
    }

    private IEnumerator MatchEndedCouroutine(string winnerName, string loserName)
    {
        MatchEnded(winnerName, loserName);
        yield return new WaitForSeconds(5);

        foreach (var player in FindObjectsOfType<LocalPlayerController>())
        {
            Destroy(player.gameObject);
        }

        foreach (var edge in this.fieldEdges)
        {
            Destroy(edge);
        }

        MainMenuLoaded();

        Destroy(this.gameObject);
    }

    private Vector3 GetPlayerPosition(LocalPlayer player)
    {
        Vector3 screenLeftSide = Camera.main.ScreenToWorldPoint(new Vector2(0, Screen.height / 2));
        Vector3 screenRightSide = Camera.main.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height / 2));

        return player.PlayerType == PlayerType.Player1 ? new Vector3(screenLeftSide.x + .5f, 0) : new Vector3(screenRightSide.x - .5f, 0);
    }

    private IEnumerator BeginGameCouroutine()
    {
        this.ball = Instantiate(this.ballPrefab);
        this.ballRigidbody = this.ball.GetComponent<Rigidbody2D>();
        var ballController = this.ball.GetComponent<BallController>();
        ballController.BallHit += this.OnBallHit;
        ballController.GoalPassed += this.OnPlayerScored;
        ScoreChanged(Player1Score, PlayerType.Player1);
        ScoreChanged(Player2Score, PlayerType.Player2);
        yield return new WaitForSeconds(5);
        this.SetInitialGameState();
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

    private void GenerateCollidersAcrossScreen()
    {
        Vector2 lDCorner = Camera.main.ViewportToWorldPoint(new Vector3(0, 0f, Camera.main.nearClipPlane));
        Vector2 rUCorner = Camera.main.ViewportToWorldPoint(new Vector3(1f, 1f, Camera.main.nearClipPlane));
        Vector2[] colliderpoints;

        var upperEdgeGameObject = new GameObject(Constants.UpperEdge);
        upperEdgeGameObject.tag = Constants.UpperEdge;
        EdgeCollider2D upperEdge = upperEdgeGameObject.AddComponent<EdgeCollider2D>();
        colliderpoints = upperEdge.points;
        colliderpoints[0] = new Vector2(lDCorner.x, rUCorner.y);
        colliderpoints[1] = new Vector2(rUCorner.x, rUCorner.y);
        upperEdge.points = colliderpoints;

        this.fieldEdges.Add(upperEdgeGameObject);

        var lowerEdgeGameObject = new GameObject(Constants.LowerEdge);
        lowerEdgeGameObject.tag = Constants.LowerEdge;
        EdgeCollider2D lowerEdge = lowerEdgeGameObject.AddComponent<EdgeCollider2D>();
        colliderpoints = lowerEdge.points;
        colliderpoints[0] = new Vector2(lDCorner.x, lDCorner.y);
        colliderpoints[1] = new Vector2(rUCorner.x, lDCorner.y);
        lowerEdge.points = colliderpoints;

        this.fieldEdges.Add(lowerEdgeGameObject);

        var leftGoalGameObject = new GameObject(Constants.LeftGoal);
        leftGoalGameObject.tag = Constants.LeftGoal;
        EdgeCollider2D leftGoalCollider = leftGoalGameObject.AddComponent<EdgeCollider2D>();
        colliderpoints = leftGoalCollider.points;
        colliderpoints[0] = new Vector2(lDCorner.x, lDCorner.y);
        colliderpoints[1] = new Vector2(lDCorner.x, rUCorner.y);
        leftGoalCollider.points = colliderpoints;

        this.fieldEdges.Add(leftGoalGameObject);

        var rightGoalGameObject = new GameObject(Constants.RightGoal);
        rightGoalGameObject.tag = Constants.RightGoal;
        EdgeCollider2D rightGoalCollider = rightGoalGameObject.AddComponent<EdgeCollider2D>();
        colliderpoints = rightGoalCollider.points;
        colliderpoints[0] = new Vector2(rUCorner.x, rUCorner.y);
        colliderpoints[1] = new Vector2(rUCorner.x, lDCorner.y);
        rightGoalCollider.points = colliderpoints;

        this.fieldEdges.Add(rightGoalGameObject);
    }
}
