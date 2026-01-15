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

    public void Execute(JoinGameCommand joinMatchCommand)
    {
        GameAggregate gameAggregate = this.gameService.GetById(joinMatchCommand.GameId)
            ?? throw new InvalidOperationException($"Game with Id {joinMatchCommand.GameId} not found");

        var playerEntity = new PlayerEntity(joinMatchCommand.PlayerId, joinMatchCommand.Username);

        gameAggregate.AddPlayer(playerEntity);
        this.domainEventDispatcherService.Dispatch(gameAggregate);
    }
}
