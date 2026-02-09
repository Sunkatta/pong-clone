using Unity.Netcode;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class GameLifetimeScope : LifetimeScope
{
    [SerializeField]
    private NetworkManager networkManager;

    [SerializeField]
    private GameObject onlinePlayerPrefab;

    protected override void Awake()
    {
        base.Awake();
        RegisterNetworkPrefabHandlers();
    }

    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterInstance(new System.Random());

        // Application Use Cases
        builder.Register<IMovePlayerUseCase, MovePlayerUseCase>(Lifetime.Transient);
        builder.Register<ICreateGameUseCase, CreateGameUseCase>(Lifetime.Transient);
        builder.Register<IMoveBallUseCase, MoveBallUseCase>(Lifetime.Transient);
        builder.Register<IUpdateBallDirectionUseCase, UpdateBallDirectionUseCase>(Lifetime.Transient);
        builder.Register<IJoinGameUseCase, JoinGameUseCase>(Lifetime.Transient);
        builder.Register<ILeaveGameUseCase, LeaveGameUseCase > (Lifetime.Transient);

        // Application Queries
        builder.Register<IGetBallDirectionQuery, GetBallDirectionQuery>(Lifetime.Transient);

        // Application Boundaries
        builder.Register<IDomainEventDispatcherService, DomainEventDispatcherService>(Lifetime.Singleton);

        // Persistence
        builder.Register<IGameService, GameService>(Lifetime.Singleton); // Consider transitioning to transient lifetime

        // Infrastructure
        // TODO: Consider exposing a dedicated event stream and rely on IDomainEventHandler instead.
        builder.Register<PlayerMovedDomainEventHandler>(Lifetime.Singleton)
            .AsSelf()
            .AsImplementedInterfaces();

        builder.Register<BallMovedDomainEventHandler>(Lifetime.Singleton)
            .AsSelf()
            .AsImplementedInterfaces();

        builder.Register<BallDirectionUpdatedDomainEventHandler>(Lifetime.Singleton)
            .AsSelf()
            .AsImplementedInterfaces();

        builder.Register<PlayerScoredDomainEventHandler>(Lifetime.Singleton)
            .AsSelf()
            .AsImplementedInterfaces();

        builder.Register<PlayerWonDomainEventHandler>(Lifetime.Singleton)
            .AsSelf()
            .AsImplementedInterfaces();

        builder.Register<PlayerJoinedDomainEventHandler>(Lifetime.Singleton)
           .AsSelf()
           .AsImplementedInterfaces();

        builder.Register<GameCreatedDomainEventHandler>(Lifetime.Transient)
           .AsSelf()
           .AsImplementedInterfaces();

        builder.Register<PlayerLeftDomainEventHandler>(Lifetime.Singleton)
           .AsSelf()
           .AsImplementedInterfaces();

        // Presentation
        builder.RegisterComponentInHierarchy<MainMenuController>();
        builder.RegisterComponentInHierarchy<InGameHudController>();
        builder.RegisterComponentInHierarchy<LobbyController>();
        builder.RegisterComponentInHierarchy<GameManager>();
        builder.RegisterComponentInHierarchy<LobbyManager>();

        builder.Register<PlayerService>(Lifetime.Singleton);
    }

    private void RegisterNetworkPrefabHandlers()
    {
        var resolver = Container;

        var handler = new VContainerNetworkPrefabHandler(
            resolver,
            onlinePlayerPrefab);

        this.networkManager.PrefabHandler.AddHandler(
            onlinePlayerPrefab,
            handler);
    }
}
