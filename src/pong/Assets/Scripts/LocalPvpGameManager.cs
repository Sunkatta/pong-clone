using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VContainer;
using VContainer.Unity;

[RequireComponent(typeof(AudioSource))]
public class LocalPvpGameManager : MonoBehaviour, IGameManager
{
    private IObjectResolver resolver;
    private PlayerScoredDomainEventHandler playerScoredDomainEventHandler;
    private PlayerWonDomainEventHandler playerWonDomainEventHandler;

    public event Action<List<PlayerEntity>> PrepareInGameUi;
    public event Action<string, bool> PlayerDisconnected;
    public static event Action MainMenuLoaded;
    public static event Action<GameOverStatistics> MatchEnded;

    [SerializeField]
    private GameObject ballPrefab;

    [SerializeField]
    private GameObject playerPrefab;

    private bool isMatchRunning;
    
    private AudioSource goalSound;
    private GameObject ball;
    private BallController ballController;

    private readonly List<PlayerEntity> players = new List<PlayerEntity>();
    private readonly List<GameObject> fieldEdges = new List<GameObject>();

    [Inject]
    public void Construct(IObjectResolver resolver,
        PlayerScoredDomainEventHandler playerScoredDomainEventHandler,
        PlayerWonDomainEventHandler playerWonDomainEventHandler)
    {
        this.resolver = resolver;
        this.playerScoredDomainEventHandler = playerScoredDomainEventHandler;
        this.playerWonDomainEventHandler = playerWonDomainEventHandler;
    }

    public void BeginGame()
    {
        StartCoroutine(this.BeginGameCoroutine());
    }

    public void OnPlayerJoined(PlayerEntity player)
    {
        throw new NotImplementedException();
    }

    public void LeaveGame()
    {
        throw new NotImplementedException();
    }

    private void OnPlayerScored(PlayerScoredDomainEvent playerScoredDomainEvent)
    {
        this.goalSound.Play();
    }

    private void Start()
    {
        this.goalSound = this.GetComponent<AudioSource>();
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
        Destroy(this.ball);

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

        MainMenuLoaded();
        this.playerWonDomainEventHandler.PlayerWon -= OnPlayerWon;
        this.playerScoredDomainEventHandler.PlayerScored -= OnPlayerScored;

        Destroy(this.gameObject);
    }

    private IEnumerator BeginGameCoroutine()
    {
        this.ball = resolver.Instantiate(this.ballPrefab);
        this.ballController = this.ball.GetComponent<BallController>();
        this.playerScoredDomainEventHandler.PlayerScored += this.OnPlayerScored;
        this.playerWonDomainEventHandler.PlayerWon += OnPlayerWon;
        //PrepareInGameUi(this.players);
        yield return new WaitForSeconds(5);
        this.isMatchRunning = true;
    }

    private void OnPlayerWon(PlayerWonDomainEvent playerWonDomainEvent)
    {
        this.StartCoroutine(this.MatchEndedCouroutine(playerWonDomainEvent.WinnerPlayerUsername, playerWonDomainEvent.LoserPlayerUsername));
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

        EdgeCollider2D leftGoalCollider = leftGoalGameObject.AddComponent<EdgeCollider2D>();
        leftGoalCollider.isTrigger = true;
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

        EdgeCollider2D rightGoalCollider = rightGoalGameObject.AddComponent<EdgeCollider2D>();
        rightGoalCollider.isTrigger = true;
        rightGoalCollider.points = new Vector2[]
        {
            new Vector2(rUCorner.x, rUCorner.y),
            new Vector2(rUCorner.x, lDCorner.y)
        };

        this.fieldEdges.Add(rightGoalGameObject);
    }
}
