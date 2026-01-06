using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class LocalPvpGameManager : MonoBehaviour, IGameManager
{
    private IObjectResolver resolver;

    public event Action<List<PlayerEntity>> PrepareInGameUi;
    public event Action<string, bool> PlayerDisconnected;
    public static event Action MainMenuLoaded;
    public static event Action<int, PlayerType> ScoreChanged;
    public static event Action<GameOverStatistics> MatchEnded;

    public int Player1Score { get; set; }

    public int Player2Score { get; set; }

    [SerializeField]
    private GameObject ballPrefab;

    [SerializeField]
    private GameObject playerPrefab;

    [SerializeField]
    private float maxBallSpeed;

    [SerializeField]
    private int targetScore;

    private bool isMatchRunning;
    
    private PlayerType? latestScorer;
    private AudioSource goalSound;
    private GameObject ball;
    private BallController ballController;

    private readonly List<PlayerEntity> players = new List<PlayerEntity>();
    private readonly List<GameObject> fieldEdges = new List<GameObject>();

    [Inject]
    public void Construct(IObjectResolver resolver)
    {
        this.resolver = resolver;
    }

    public void BeginGame()
    {
        StartCoroutine(this.BeginGameCouroutine());
    }

    public void OnPlayerJoined(PlayerEntity player)
    {
        this.players.Add(player);

        var playerGameObject = resolver.Instantiate(this.playerPrefab);
        var playerInstanceController = playerGameObject.GetComponent<LocalPlayerController>();
        playerInstanceController.Type = player.PlayerType;
        playerInstanceController.Id = player.Id;
        playerGameObject.transform.position = this.GetPlayerPosition(player);
    }

    public void LeaveGame()
    {
        throw new NotImplementedException();
    }

    private void EndGame(string winnerName, string loserName)
    {
        this.StartCoroutine(this.MatchEndedCouroutine(winnerName, loserName));
    }

    private void OnBallHit()
    {
        if (this.ballController.CurrentBallSpeed >= this.maxBallSpeed)
        {
            return;
        }

        var newSpeed = this.ballController.CurrentBallSpeed + 1;
        this.ballController.UpdateSpeed(newSpeed);
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

    private void Update()
    {
        if (this.isMatchRunning)
        {
            this.ballController.Move();
        }
    }

    private IEnumerator MatchEndedCouroutine(string winnerName, string loserName)
    {
        this.isMatchRunning = false;

        var gameOverStatistics = new GameOverStatistics
        {
            WinnerName = winnerName,
            LoserName = loserName,
            NavigatingToMessage = Constants.ReturningToMainMenuText,
        };

        MatchEnded(gameOverStatistics);
        yield return new WaitForSeconds(5);

        foreach (var player in FindObjectsByType<LocalPlayerController>(FindObjectsSortMode.None))
        {
            Destroy(player.gameObject);
        }

        foreach (var edge in this.fieldEdges)
        {
            Destroy(edge);
        }

        ScoreChanged(0, PlayerType.Player1);
        ScoreChanged(0, PlayerType.Player2);

        MainMenuLoaded();

        Destroy(this.gameObject);
    }

    private Vector3 GetPlayerPosition(PlayerEntity player)
    {
        Vector3 screenLeftSide = Camera.main.ScreenToWorldPoint(new Vector2(0, Screen.height / 2));
        Vector3 screenRightSide = Camera.main.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height / 2));

        return player.PlayerType == PlayerType.Player1 ? new Vector3(screenLeftSide.x + .5f, 0) : new Vector3(screenRightSide.x - .5f, 0);
    }

    private IEnumerator BeginGameCouroutine()
    {
        this.ball = resolver.Instantiate(this.ballPrefab);
        this.ballController = this.ball.GetComponent<BallController>();
        this.ballController.BallHit += this.OnBallHit;
        this.ballController.GoalPassed += this.OnPlayerScored;
        PrepareInGameUi(this.players);
        yield return new WaitForSeconds(5);
        this.SetInitialGameState();
        this.isMatchRunning = true;
    }

    private void SetInitialGameState()
    {
        this.ballController.ResetBall();
        this.ballController.UpdateBallDirection(this.GetBallDirection());
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

        var upperEdgeGameObject = new GameObject(Constants.UpperEdge)
        {
            tag = Constants.UpperEdge
        };

        Rigidbody2D upperEdgeRigidbody = upperEdgeGameObject.AddComponent<Rigidbody2D>();
        upperEdgeRigidbody.bodyType = RigidbodyType2D.Static;

        EdgeCollider2D upperEdgeCollider = upperEdgeGameObject.AddComponent<EdgeCollider2D>();
        upperEdgeCollider.points = new Vector2[]
        {
            new Vector2(lDCorner.x, rUCorner.y),
            new Vector2(rUCorner.x, rUCorner.y)
        };

        this.fieldEdges.Add(upperEdgeGameObject);

        var lowerEdgeGameObject = new GameObject(Constants.LowerEdge)
        {
            tag = Constants.LowerEdge
        };

        Rigidbody2D lowerEdgeRigidbody = lowerEdgeGameObject.AddComponent<Rigidbody2D>();
        lowerEdgeRigidbody.bodyType = RigidbodyType2D.Static;

        EdgeCollider2D lowerEdgeCollider = lowerEdgeGameObject.AddComponent<EdgeCollider2D>();
        lowerEdgeCollider.points = new Vector2[]
        {
            new Vector2(lDCorner.x, lDCorner.y),
            new Vector2(rUCorner.x, lDCorner.y)
        };

        this.fieldEdges.Add(lowerEdgeGameObject);

        var leftGoalGameObject = new GameObject(Constants.LeftGoal)
        {
            tag = Constants.LeftGoal
        };

        Rigidbody2D leftGoalRigidbody = leftGoalGameObject.AddComponent<Rigidbody2D>();
        leftGoalRigidbody.bodyType = RigidbodyType2D.Static;

        EdgeCollider2D leftGoalCollider = leftGoalGameObject.AddComponent<EdgeCollider2D>();
        leftGoalCollider.points = new Vector2[]
        {
            new Vector2(lDCorner.x, lDCorner.y),
            new Vector2(lDCorner.x, rUCorner.y)
        };

        this.fieldEdges.Add(leftGoalGameObject);

        var rightGoalGameObject = new GameObject(Constants.RightGoal)
        {
            tag = Constants.RightGoal
        };

        Rigidbody2D rightGoalRigidbody = rightGoalGameObject.AddComponent<Rigidbody2D>();
        rightGoalRigidbody.bodyType = RigidbodyType2D.Static;

        EdgeCollider2D rightGoalCollider = rightGoalGameObject.AddComponent<EdgeCollider2D>();
        rightGoalCollider.points = new Vector2[]
        {
            new Vector2(rUCorner.x, rUCorner.y),
            new Vector2(rUCorner.x, lDCorner.y)
        };

        this.fieldEdges.Add(rightGoalGameObject);
    }
}
