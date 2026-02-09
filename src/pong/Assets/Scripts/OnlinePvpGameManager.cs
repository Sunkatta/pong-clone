using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using VContainer;
using VContainer.Unity;

[RequireComponent(typeof(AudioSource))]
public class OnlinePvpGameManager : NetworkBehaviour
{
    private IObjectResolver resolver;
    private IJoinGameUseCase joinGameUseCase;
    private ILeaveGameUseCase leaveGameUseCase;
    private PlayerJoinedDomainEventHandler playerJoinedDomainEventHandler;
    private PlayerLeftDomainEventHandler playerLeftDomainEventHandler;
    private PlayerScoredDomainEventHandler playerScoredDomainEventHandler;
    private PlayerWonDomainEventHandler playerWonDomainEventHandler;
    private PlayerService playerService;

    [Inject]
    public void Construct(IObjectResolver resolver,
        IJoinGameUseCase joinGameUseCase,
        ILeaveGameUseCase leaveGameUseCase,
        PlayerJoinedDomainEventHandler playerJoinedDomainEventHandler,
        PlayerLeftDomainEventHandler playerLeftDomainEventHandler,
        PlayerScoredDomainEventHandler playerScoredDomainEventHandler,
        PlayerWonDomainEventHandler playerWonDomainEventHandler,
        PlayerService playerService)
    {
        this.resolver = resolver;
        this.joinGameUseCase = joinGameUseCase;
        this.leaveGameUseCase = leaveGameUseCase;
        this.playerJoinedDomainEventHandler = playerJoinedDomainEventHandler;
        this.playerLeftDomainEventHandler = playerLeftDomainEventHandler;
        this.playerScoredDomainEventHandler = playerScoredDomainEventHandler;
        this.playerWonDomainEventHandler = playerWonDomainEventHandler;
        this.playerService = playerService;
    }

    public event Action PrepareInGameUi;
    public event Action<string, bool> PlayerDisconnected;
    public event Action LobbyLoaded;
    public event Action<GameOverStatistics> MatchEnded;

    public NetworkVariable<int> Player1Score { get; private set; } = new NetworkVariable<int>();

    public NetworkVariable<int> Player2Score { get; private set; } = new NetworkVariable<int>();

    [SerializeField]
    private GameObject ballPrefab;

    [SerializeField]
    private NetworkObject playerPrefab;

    private readonly List<GameObject> fieldEdges = new List<GameObject>();

    private AudioSource goalSound;
    private BallController ballController;
    private bool isMatchRunning;

    private NetworkObject ball;

    public override void OnNetworkSpawn()
    {
        if (!this.IsServer)
        {
            return;
        }

        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        this.playerJoinedDomainEventHandler.PlayerJoined += OnPlayerJoined;
        this.playerLeftDomainEventHandler.PlayerLeft += OnPlayerLeft;
    }

    public void BeginGame()
    {
        StartCoroutine(this.BeginGameCoroutine());
    }

    public void LeaveGame()
    {
        NetworkManager.Singleton.Shutdown();
    }

    public override void OnNetworkDespawn()
    {
        var playerId = this.playerService.GetPlayerIdByClientId(NetworkManager.Singleton.LocalClientId);

        if (this.IsClient)
        {
            playerId = GameManager.Instance.CurrentPlayer2Id;
        }

        this.PlayerDisconnected(playerId, true);

        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;

        foreach (var edge in this.fieldEdges)
        {
            Destroy(edge);
        }

        Destroy(this.gameObject);

        base.OnNetworkDespawn();
    }

    public override void OnDestroy()
    {
        this.playerJoinedDomainEventHandler.PlayerJoined -= OnPlayerJoined;
        this.playerLeftDomainEventHandler.PlayerLeft -= OnPlayerLeft;
        base.OnDestroy();
    }

    private void OnClientConnected(ulong clientId)
    {
        var playerId = GameManager.Instance.CurrentPlayer1Id;
        var playerUsername = GameManager.Instance.CurrentPlayer1Username;
        var playerType = PlayerType.Player1;

        if (NetworkManager.ConnectedClients.Count == Constants.MaxPlayersCount)
        {
            playerId = GameManager.Instance.CurrentPlayer2Id;
            playerUsername = GameManager.Instance.CurrentPlayer2Username;
            playerType = PlayerType.Player2;
        }

        this.playerService.RegisterPlayerId(playerId, clientId);

        NetworkManager.SpawnManager.InstantiateAndSpawn(this.playerPrefab,
            ownerClientId: clientId,
            isPlayerObject: true,
            position: this.GetPlayerPosition(playerType));

        this.joinGameUseCase.Execute(new JoinGameCommand(GameManager.Instance.CurrentGameId, playerId, playerUsername));

        if (playerType == PlayerType.Player2)
        {
            // This works for now, but only with 2 players. If/when more players are added, every new client
            // will need to sync the previously joined players.
            var localPlayerNetworkModel = new LocalPlayerNetworkModel(GameManager.Instance.CurrentPlayer1Id,
                GameManager.Instance.CurrentPlayer1Username,
                PlayerType.Player1);

            this.SyncPlayerWithClientsRpc(localPlayerNetworkModel);
        }
    }

    [Rpc(SendTo.NotServer)]
    private void SyncPlayerWithClientsRpc(LocalPlayerNetworkModel localPlayerNetworkModel)
    {
        var localPlayer = new PlayerEntity(localPlayerNetworkModel.GetId(),
            localPlayerNetworkModel.GetUsername(),
            localPlayerNetworkModel.GetPlayerType());

        GameManager.Instance.SetPlayer1(localPlayer.Id, localPlayer.Username);
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

    private void OnClientDisconnected(ulong clientId)
    {
        if (!this.IsServer)
        {
            return;
        }

        if (clientId == this.OwnerClientId)
        {
            foreach (var clientIdToDisconnect in NetworkManager.Singleton.ConnectedClientsIds)
            {
                var playerIdToDisconnect = this.playerService.GetPlayerIdByClientId(clientIdToDisconnect);

                if (playerIdToDisconnect == null || clientIdToDisconnect == clientId)
                {
                    continue;
                }

                this.leaveGameUseCase.Execute(new LeaveGameCommand(GameManager.Instance.CurrentGameId, playerIdToDisconnect));
            }
        }

        var targetPlayerIdToDisconnect = this.playerService.GetPlayerIdByClientId(clientId);
        this.leaveGameUseCase.Execute(new LeaveGameCommand(GameManager.Instance.CurrentGameId, targetPlayerIdToDisconnect));
    }

    private void OnPlayerLeft(PlayerLeftDomainEvent playerLeftDomainEvent)
    {
        var clientId = this.playerService.GetClientIdByPlayerId(playerLeftDomainEvent.PlayerId);

        if (clientId != null)
        {
            this.playerService.RemoveClient(clientId.Value);
        }

        if (this.isMatchRunning)
        {
            this.ball.Despawn();

            this.MatchEndedRpc(GameManager.Instance.CurrentPlayer1Username, GameManager.Instance.CurrentPlayer2Username);
            this.isMatchRunning = false;
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

    private void Start()
    {
        this.goalSound = this.GetComponent<AudioSource>();
        this.GenerateCollidersAcrossScreen();

        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
    }

    private void Update()
    {
        if (this.isMatchRunning && this.IsServer)
        {
            this.ballController.Move();
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void PrepareInGameUiRpc()
    {
        PrepareInGameUi();
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
