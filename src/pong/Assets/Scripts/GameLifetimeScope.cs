using VContainer;
using VContainer.Unity;

public class GameLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        // Application Use Cases
        builder.Register<IMovePlayerUseCase, MovePlayerUseCase>(Lifetime.Transient);
        builder.Register<ICreateGameUseCase, CreateGameUseCase>(Lifetime.Transient);

        // Application Boundaries
        builder.Register<IDomainEventDispatcherService, DomainEventDispatcherService>(Lifetime.Singleton);

        // Persistence
        builder.Register<IGameService, GameService>(Lifetime.Singleton); // Consider transitioning to transient lifetime

        // Infrastructure
        // TODO: Consider exposing a dedicated event stream and rely on IDomainEventHandler instead.
        builder.Register<PlayerMovedDomainEventHandler>(Lifetime.Singleton)
            .AsSelf()
            .AsImplementedInterfaces();

        // Presentation
        builder.RegisterComponentInHierarchy<MainMenuController>();
    }
}
