using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using VContainer;
using VContainer.Unity;

[RequireComponent(typeof(AudioSource))]
public class OnlinePvpGameManager : NetworkBehaviour, IGameManager
{
    private IObjectResolver resolver;
    private IJoinGameUseCase joinGameUseCase;
    private PlayerJoinedDomainEventHandler playerJoinedDomainEventHandler;
    private PlayerScoredDomainEventHandler playerScoredDomainEventHandler;
    private PlayerWonDomainEventHandler playerWonDomainEventHandler;
    private PlayerService playerService;

    [Inject]
    public void Construct(IObjectResolver resolver,
        IJoinGameUseCase joinGameUseCase,
        PlayerJoinedDomainEventHandler playerJoinedDomainEventHandler,
        PlayerScoredDomainEventHandler playerScoredDomainEventHandler,
        PlayerWonDomainEventHandler playerWonDomainEventHandler,
        PlayerService playerService)
    {
        this.resolver = resolver;
        this.joinGameUseCase = joinGameUseCase;
        this.playerJoinedDomainEventHandler = playerJoinedDomainEventHandler;
        this.playerScoredDomainEventHandler = playerScoredDomainEventHandler;
        this.playerWonDomainEventHandler = playerWonDomainEventHandler;
        this.playerService = playerService;
    }

    public event Action<List<PlayerEntity>> PrepareInGameUi;
    public event Action<string, bool> PlayerDisconnected;
    public static event Action LobbyLoaded;
    public static event Action<GameOverStatistics> MatchEnded;

    public static NetworkVariable<int> Player1Score { get; private set; } = new NetworkVariable<int>();

    public static NetworkVariable<int> Player2Score { get; private set; } = new NetworkVariable<int>();

    [SerializeField]
    private GameObject ballPrefab;

    [SerializeField]
    private NetworkObject playerPrefab;

    private readonly List<PlayerEntity> players = new List<PlayerEntity>();
    private readonly List<GameObject> fieldEdges = new List<GameObject>();

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

    private void OnPlayerScored(PlayerScoredDomainEvent playerScoredDomainEvent)
    {
        this.goalSound.Play();

        if (this.IsServer)
        {
            if (playerScoredDomainEvent.PlayerType == PlayerType.Player1)
            {
                Player1Score.Value = playerScoredDomainEvent.PlayerNewScore;
            }
            else
            {
                Player2Score.Value = playerScoredDomainEvent.PlayerNewScore;
            }
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
        GameManager.Instance.SetPlayer1(localPlayer.Id, localPlayer.Username);
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
            var ballInstance = this.resolver.Instantiate(this.ballPrefab);
            var ballInstanceNetworkObject = ballInstance.GetComponent<NetworkObject>();
            ballInstanceNetworkObject.Spawn();
            yield return new WaitForSeconds(1);
            this.ball = ballInstanceNetworkObject;
            this.ballController = this.ball.GetComponent<BallController>();
            this.playerScoredDomainEventHandler.PlayerScored += this.OnPlayerScored;
            this.playerWonDomainEventHandler.PlayerWon += OnPlayerWon;
            this.PrepareInGameUiRpc();
            yield return new WaitForSeconds(5);
            this.isMatchRunning = true;
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void PrepareInGameUiRpc()
    {
        PrepareInGameUi(this.players);
    }

    private void OnPlayerWon(PlayerWonDomainEvent playerWonDomainEvent)
    {
        this.isMatchRunning = false;
        this.ball.Despawn();

        this.playerScoredDomainEventHandler.PlayerScored -= this.OnPlayerScored;
        this.playerWonDomainEventHandler.PlayerWon -= OnPlayerWon;

        this.MatchEndedRpc(playerWonDomainEvent.WinnerPlayerUsername, playerWonDomainEvent.LoserPlayerUsername);
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
            Player1Score.Value = 0;
            Player2Score.Value = 0;
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
