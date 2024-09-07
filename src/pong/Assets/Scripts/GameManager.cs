using System;
using System.Collections;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static event Action PrepareInGameUi;
    public event Action<PlayerType> MatchEnded;

    [SerializeField]
    private new Camera camera;

    [SerializeField]
    private LobbyManager lobbyManager;

    [SerializeField]
    private GameObject ballPrefab;

    [SerializeField]
    private GameObject playerPrefab;

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

    [SerializeField]
    private GameType gameType;

    private float currentBallSpeed;
    private Rigidbody2D ballRigidbody;
    private PlayerType? latestScorer;
    private AudioSource goalSound;

    private PlayerController player1;
    private PlayerController player2;

    private NetworkObject ball;

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

        this.lobbyManager.PlayerJoined += this.OnPlayerJoined;
        this.lobbyManager.BeginGame += OnBeginGame;

        PlayerController.PlayerInstantiated += this.OnPlayerInstantiated;

        this.GenerateCollidersAcrossScreen();
    }

    private void OnPlayerJoined(PlayerType playerType)
    {
        switch (playerType)
        {
            case PlayerType.Player1:
                NetworkManager.Singleton.StartHost();
                break;
            case PlayerType.Player2:
                NetworkManager.Singleton.StartClient();
                break;
            default:
                break;
        }
    }

    private void OnBeginGame(GameType gameType)
    {
        switch (gameType)
        {
            case GameType.LocalPvp:
                break;
            case GameType.OnlinePvp:
                StartCoroutine(this.BeginGame());

                break;
            default:
                break;
        }
    }
    private IEnumerator BeginGame()
    {
        yield return new WaitForSeconds(3);

        if (this.IsServer)
        {
            var ballInstance = Instantiate(this.ballPrefab);
            var ballInstanceNetworkObject = ballInstance.GetComponent<NetworkObject>();
            ballInstanceNetworkObject.Spawn();
            yield return new WaitForSeconds(1);
            this.BallSpawnedRpc(ballInstanceNetworkObject);
        }

        this.PrepareInGameUiRpc();

        yield return new WaitForSeconds(5);

        this.SetInitialGameState();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void BallSpawnedRpc(NetworkObjectReference ballNetworkObject)
    {
        this.ball = ballNetworkObject;
        this.ballRigidbody = this.ball.GetComponent<Rigidbody2D>();
        var ballController = this.ball.GetComponent<BallController>();
        ballController.BallHit += this.OnBallHit;
        ballController.GoalPassed += this.OnPlayerScored;
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void PrepareInGameUiRpc()
    {
        PrepareInGameUi();
    }

    private void OnPlayerInstantiated(PlayerController player)
    {
        this.SetupPlayer(player);

        if (NetworkManager.Singleton.ConnectedClients.Count == Constants.MaxPlayersCount)
        {
            this.SyncPlayers(this.player1, this.player2);
        }
    }

    private void SyncPlayers(PlayerController player1, PlayerController player2)
    {
        this.SyncPlayerRpc(player1.gameObject);
        this.SyncPlayerRpc(player2.gameObject);
    }

    [Rpc(SendTo.NotServer)]
    private void SyncPlayerRpc(NetworkObjectReference playerNetworkObjectReference)
    {
        NetworkObject playerNetworkObject = playerNetworkObjectReference;
        this.SetupPlayer(playerNetworkObject.GetComponent<PlayerController>());
    }

    private void SetupPlayer(PlayerController player)
    {
        if (this.player1 == null)
        {
            this.player1 = player;
            this.SetPlayerPosition(this.player1);
            this.player1.Score.OnValueChanged += (int previousValue, int newValue) =>
            {
                this.player1ScoreText.text = newValue.ToString();
            };
        }
        else
        {
            this.player2 = player;
            this.SetPlayerPosition(this.player2);
            this.player2.Score.OnValueChanged += (int previousValue, int newValue) =>
            {
                this.player2ScoreText.text = newValue.ToString();
            };
        }
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

        if (this.player1.Type == scorer)
        {
            this.player1.Score.Value++;
        }
        else
        {
            this.player2.Score.Value++;
        }

        if (this.player1.Score.Value == targetScore || this.player2.Score.Value == targetScore)
        {
            var winner = this.player1.Score.Value == targetScore ? PlayerType.Player1 : PlayerType.Player2;

            this.MatchEnded(winner);

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
