using System;
using VContainer;
using VContainer.Unity;

public class GameLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterInstance(new Random());

        // Application Use Cases
        builder.Register<IMovePlayerUseCase, MovePlayerUseCase>(Lifetime.Transient);
        builder.Register<ICreateGameUseCase, CreateGameUseCase>(Lifetime.Transient);
        builder.Register<IMoveBallUseCase, MoveBallUseCase>(Lifetime.Transient);
        builder.Register<IUpdateBallDirectionUseCase, UpdateBallDirectionUseCase>(Lifetime.Transient);
        builder.Register<IJoinGameUseCase, JoinGameUseCase>(Lifetime.Transient);

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

        // Presentation
        builder.RegisterComponentInHierarchy<MainMenuController>();
        builder.RegisterComponentInHierarchy<InGameHudController>();
        builder.RegisterComponentInHierarchy<GameManager>();
    }
}
