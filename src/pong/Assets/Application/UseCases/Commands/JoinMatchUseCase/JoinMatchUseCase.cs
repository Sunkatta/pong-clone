using System;

public class JoinMatchUseCase : IJoinMatchUseCase
{
    private readonly IGameService gameService;
    private readonly IDomainEventDispatcherService domainEventDispatcherService;

    public JoinMatchUseCase(IGameService gameService, IDomainEventDispatcherService domainEventDispatcherService)
    {
        this.gameService = gameService;
        this.domainEventDispatcherService = domainEventDispatcherService;
    }

    public void Execute(JoinMatchCommand joinMatchCommand)
    {
        GameAggregate gameAggregate = this.gameService.GetById(joinMatchCommand.GameId)
            ?? throw new InvalidOperationException($"Game with Id {joinMatchCommand.GameId} not found");

        var playerEntity = new PlayerEntity(joinMatchCommand.PlayerId, joinMatchCommand.Username);

        gameAggregate.AddPlayer(playerEntity);
        this.domainEventDispatcherService.Dispatch(gameAggregate);
    }
}
