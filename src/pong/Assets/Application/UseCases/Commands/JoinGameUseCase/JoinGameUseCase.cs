using System;

public class JoinGameUseCase : IJoinGameUseCase
{
    private readonly IGameService gameService;
    private readonly IDomainEventDispatcherService domainEventDispatcherService;

    public JoinGameUseCase(IGameService gameService, IDomainEventDispatcherService domainEventDispatcherService)
    {
        this.gameService = gameService;
        this.domainEventDispatcherService = domainEventDispatcherService;
    }

    public void Execute(JoinGameCommand joinGameCommand)
    {
        GameAggregate gameAggregate = this.gameService.GetById(joinGameCommand.GameId)
            ?? throw new InvalidOperationException($"Game with Id {joinGameCommand.GameId} not found");

        var playerEntity = new PlayerEntity(joinGameCommand.PlayerId, joinGameCommand.Username);

        gameAggregate.AddPlayer(playerEntity);
        this.domainEventDispatcherService.Dispatch(gameAggregate);
    }
}
