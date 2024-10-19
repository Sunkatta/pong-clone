using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static event Action PrepareInGameUi;
    public static event Action<int, PlayerType> ScoreChanged;
    public static event Action<string, string> MatchEnded;

    public NetworkVariable<int> Player1Score { get; set; } = new NetworkVariable<int>();

    public NetworkVariable<int> Player2Score { get; set; } = new NetworkVariable<int>();

    [SerializeField]
    private new Camera camera;

    [SerializeField]
    private LobbyManager lobbyManager;

    [SerializeField]
    private GameObject ballPrefab;

    [SerializeField]
    private NetworkObject playerPrefab;

    [SerializeField]
    private float initialBallSpeed;

    [SerializeField]
    private float maxBallSpeed;

    [SerializeField]
    private int targetScore;

    [SerializeField]
    private GameType gameType;

    private readonly List<LocalPlayer> players = new List<LocalPlayer>();

    private float currentBallSpeed;
    private Rigidbody2D ballRigidbody;
    private PlayerType? latestScorer;
    private AudioSource goalSound;

    private NetworkObject ball;

    public override void OnNetworkSpawn()
    {
        this.ClientConnectedRpc();

        this.Player1Score.OnValueChanged += (int previousValue, int newValue) =>
        {
            ScoreChanged(newValue, PlayerType.Player1);
        };

        this.Player2Score.OnValueChanged += (int previousValue, int newValue) =>
        {
            ScoreChanged(newValue, PlayerType.Player2);
        };
    }

    public void NewGame()
    {
        this.Player1Score.Value = 0;
        this.Player2Score.Value = 0;

        this.SetInitialGameState();
    }

    [Rpc(SendTo.Server)]
    private void ClientConnectedRpc(RpcParams rpcParams = default)
    {
        var playerInstanceController = this.playerPrefab.GetComponent<PlayerController>();
        playerInstanceController.Type = PlayerType.Player1;

        if (NetworkManager.ConnectedClients.Count == Constants.MaxPlayersCount)
        {
            playerInstanceController.Type = PlayerType.Player2;
        }

        NetworkManager.SpawnManager.InstantiateAndSpawn(this.playerPrefab,
            ownerClientId: rpcParams.Receive.SenderClientId,
            isPlayerObject: true,
            position: this.GetPlayerPosition(playerInstanceController));
    }

    private void Start()
    {
        this.goalSound = GetComponent<AudioSource>();

        this.lobbyManager.PlayerJoined += this.OnPlayerJoined;
        this.lobbyManager.BeginGame += OnBeginGame;

        this.GenerateCollidersAcrossScreen();
    }

    private void OnPlayerJoined(LocalPlayer player)
    {
        this.players.Add(player);

        switch (player.PlayerType)
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

        if (this.IsServer)
        {
            this.latestScorer = scorer;

            if (scorer == PlayerType.Player1)
            {
                this.Player1Score.Value++;
            }
            else
            {
                this.Player2Score.Value++;
            }

            if (this.Player1Score.Value == targetScore || this.Player2Score.Value == targetScore)
            {
                var winnerType = this.Player1Score.Value == targetScore ? PlayerType.Player1 : PlayerType.Player2;
                var winnerPlayer = this.players.First(player => player.PlayerType == winnerType);
                var loserPlayer = this.players.First(player => player.PlayerType != winnerType);

                this.MatchEndedRpc(winnerPlayer.Username, loserPlayer.Username);

                return;
            }

            this.SetInitialGameState();
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void MatchEndedRpc(string winnerName, string loserName)
    {
        MatchEnded(winnerName, loserName);
    }

    private void OnBallHit()
    {
        if (this.IsServer)
        {
            if (this.currentBallSpeed >= this.maxBallSpeed)
            {
                return;
            }

            var oldSpeed = this.currentBallSpeed;
            this.currentBallSpeed++;

            this.ballRigidbody.velocity *= this.currentBallSpeed / oldSpeed;
        }
    }

    private Vector3 GetPlayerPosition(PlayerController player)
    {
        Vector3 screenLeftSide = this.camera.ScreenToWorldPoint(new Vector2(0, Screen.height / 2));
        Vector3 screenRightSide = this.camera.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height / 2));

        return player.Type == PlayerType.Player1 ? new Vector3(screenLeftSide.x + .5f, 0) : new Vector3(screenRightSide.x - .5f, 0);
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
