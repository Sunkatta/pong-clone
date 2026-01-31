using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using VContainer;

[RequireComponent(typeof(AudioSource))]
public class OnlinePvpGameManager : NetworkBehaviour, IGameManager
{
    private IJoinGameUseCase joinGameUseCase;
    private PlayerJoinedDomainEventHandler playerJoinedDomainEventHandler;
    private PlayerService playerService;

    [Inject]
    public void Construct(IJoinGameUseCase joinGameUseCase, PlayerJoinedDomainEventHandler playerJoinedDomainEventHandler, PlayerService playerService)
    {
        this.joinGameUseCase = joinGameUseCase;
        this.playerJoinedDomainEventHandler = playerJoinedDomainEventHandler;
        this.playerService = playerService; 
    }

    public event Action<List<PlayerEntity>> PrepareInGameUi;
    public event Action<string, bool> PlayerDisconnected;
    public static event Action LobbyLoaded;
    public static event Action<int, PlayerType> ScoreChanged;
    public static event Action<GameOverStatistics> MatchEnded;

    public NetworkVariable<int> Player1Score { get; set; } = new NetworkVariable<int>();

    public NetworkVariable<int> Player2Score { get; set; } = new NetworkVariable<int>();

    [SerializeField]
    private GameObject ballPrefab;

    [SerializeField]
    private NetworkObject playerPrefab;

    [SerializeField]
    private float maxBallSpeed;

    [SerializeField]
    private int targetScore;

    private readonly List<PlayerEntity> players = new List<PlayerEntity>();
    private readonly List<GameObject> fieldEdges = new List<GameObject>();

    private PlayerType? latestScorer;
    private AudioSource goalSound;
    private BallController ballController;
    private bool isMatchRunning;
    private PlayerEntity localPlayer;

    private NetworkObject ball;

    public override void OnNetworkSpawn()
    {
        if (!this.IsServer)
        {
            return;
        }

        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        this.playerJoinedDomainEventHandler.PlayerJoined += OnPlayerJoined;

        this.Player1Score.OnValueChanged += (int previousValue, int newValue) =>
        {
            ScoreChanged(newValue, PlayerType.Player1);
        };

        this.Player2Score.OnValueChanged += (int previousValue, int newValue) =>
        {
            ScoreChanged(newValue, PlayerType.Player2);
        };
    }

    public void BeginGame()
    {
        StartCoroutine(this.BeginGameCoroutine());
    }

    public void OnPlayerJoined(PlayerEntity player)
    {
        this.players.Add(player);
        this.localPlayer = player;

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

    public void LeaveGame()
    {
        NetworkManager.Singleton.Shutdown();
    }

    private void OnPlayerJoined(PlayerJoinedDomainEvent playerJoinedDomainEvent)
    {
        this.SyncGameInfoWithClientRpc(playerJoinedDomainEvent.PlayerPositionMinY, playerJoinedDomainEvent.PlayerPositionMaxY);
    }

    [ClientRpc]
    private void SyncGameInfoWithClientRpc(float playerPositionMinY, float playerPositionMaxY, ClientRpcParams _ = default)
    {
        GameManager.Instance.SetPlayerLimits(playerPositionMinY, playerPositionMaxY);
    }

    private void OnPlayerLeft(ulong _)
    {
        if (this.IsServer)
        {
            var player2 = this.players.FirstOrDefault(player => player.PlayerType == PlayerType.Player2);

            if (player2 == null)
            {
                this.PlayerDisconnected(this.localPlayer.Id, true);

                NetworkManager.Singleton.Shutdown();

                foreach (var edge in this.fieldEdges)
                {
                    Destroy(edge);
                }

                Destroy(this.gameObject);
                return;
            }

            this.players.Remove(player2);
            this.PlayerDisconnected(player2.Id, false);

            if (this.isMatchRunning)
            {
                var winnerPlayer = this.players.First();
                var loserPlayer = player2;

                this.ball.Despawn();

                this.MatchEndedRpc(winnerPlayer.Username, loserPlayer.Username);
                this.isMatchRunning = false;
            }
        }
        else
        {
            this.PlayerDisconnected(this.localPlayer.Id, true);

            NetworkManager.Singleton.Shutdown();

            foreach (var edge in this.fieldEdges)
            {
                Destroy(edge);
            }

            Destroy(this.gameObject);
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

                this.ball.Despawn();

                this.MatchEndedRpc(winnerPlayer.Username, loserPlayer.Username);
                this.isMatchRunning = false;

                return;
            }

            this.SetInitialGameState();
        }
    }

    private void OnBallHit()
    {
        if (this.IsServer)
        {
            if (this.ballController.CurrentBallSpeed >= this.maxBallSpeed)
            {
                return;
            }

            var newSpeed = this.ballController.CurrentBallSpeed + 1;
            this.ballController.UpdateSpeed(newSpeed);
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        var player = this.players.FirstOrDefault(player => player.PlayerType == PlayerType.Player1);

        if (NetworkManager.ConnectedClients.Count == Constants.MaxPlayersCount)
        {
            player = this.players.FirstOrDefault(player => player.PlayerType == PlayerType.Player2);
        }

        this.playerService.RegisterPlayerId(player.Id, clientId);

        NetworkManager.SpawnManager.InstantiateAndSpawn(this.playerPrefab,
            ownerClientId: clientId,
            isPlayerObject: true,
            position: this.GetPlayerPosition(player.PlayerType));

        this.joinGameUseCase.Execute(new JoinGameCommand(GameManager.Instance.CurrentGameId, player.Id, player.Username));

        if (player.PlayerType == PlayerType.Player2)
        {
            // This works for now, but only with 2 players. If/when more players are added, every new client
            // will need to sync the previously joined players.
            var player1 = this.players.FirstOrDefault(player => player.PlayerType == PlayerType.Player1);
            var localPlayerNetworkModel = new LocalPlayerNetworkModel(player1.Id, player1.Username, player1.PlayerType);
            this.SyncPlayerWithClientsRpc(localPlayerNetworkModel);
        }
    }

    [Rpc(SendTo.NotServer)]
    private void SyncPlayerWithClientsRpc(LocalPlayerNetworkModel localPlayerNetworkModel)
    {
        var localPlayer = new PlayerEntity(localPlayerNetworkModel.GetId(),
            localPlayerNetworkModel.GetUsername(),
            localPlayerNetworkModel.GetPlayerType());

        this.players.Add(localPlayer);
    }

    private void Start()
    {
        this.goalSound = this.GetComponent<AudioSource>();
        this.GenerateCollidersAcrossScreen();

        NetworkManager.Singleton.OnClientDisconnectCallback += OnPlayerLeft;
    }

    private void Update()
    {
        if (this.isMatchRunning && this.IsServer)
        {
            this.ballController.Move();
        }
    }

    public override void OnDestroy()
    {
        this.playerJoinedDomainEventHandler.PlayerJoined -= OnPlayerJoined;
        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnPlayerLeft;
        base.OnDestroy();
    }

    private IEnumerator BeginGameCoroutine()
    {
        if (this.IsServer)
        {
            var ballInstance = Instantiate(this.ballPrefab);
            var ballInstanceNetworkObject = ballInstance.GetComponent<NetworkObject>();
            ballInstanceNetworkObject.Spawn();
            yield return new WaitForSeconds(1);
            this.BallSpawnedRpc(ballInstanceNetworkObject);
            this.PrepareInGameUiRpc();
            yield return new WaitForSeconds(5);
            this.SetInitialGameState();
            this.isMatchRunning = true;
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void BallSpawnedRpc(NetworkObjectReference ballNetworkObject)
    {
        this.ball = ballNetworkObject;
        this.ballController = this.ball.GetComponent<BallController>();
        ballController.BallHit += this.OnBallHit;
        ballController.GoalPassed += this.OnPlayerScored;
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void PrepareInGameUiRpc()
    {
        PrepareInGameUi(this.players);
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

    [Rpc(SendTo.ClientsAndHost)]
    private void MatchEndedRpc(string winnerName, string loserName)
    {
        this.EndGame(winnerName, loserName);
        
    }

    public void EndGame(string winnerName, string loserName)
    {
        this.StartCoroutine(this.MatchEndedCoroutine(winnerName, loserName));
    }

    private IEnumerator MatchEndedCoroutine(string winnerName, string loserName)
    {
        var gameOverStatistics = new GameOverStatistics
        {
            WinnerName = winnerName,
            LoserName = loserName,
            NavigatingToMessage = Constants.ReturningToLobbyText,
        };

        MatchEnded(gameOverStatistics);
        yield return new WaitForSeconds(5);

        if (this.IsServer)
        {
            this.Player1Score.Value = 0;
            this.Player2Score.Value = 0;

            this.latestScorer = null;
        }

        LobbyLoaded();
    }

    private Vector3 GetPlayerPosition(PlayerType playerType)
    {
        Vector3 screenLeftSide = Camera.main.ScreenToWorldPoint(new Vector2(0, Screen.height / 2));
        Vector3 screenRightSide = Camera.main.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height / 2));

        return playerType == PlayerType.Player1 ? new Vector3(screenLeftSide.x + .5f, 0) : new Vector3(screenRightSide.x - .5f, 0);
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
