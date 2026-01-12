using System;

public class MovePlayerUseCase : IMovePlayerUseCase
{
    private readonly IGameService gameService;
    private readonly IDomainEventDispatcherService domainEventDispatcherService;

    public MovePlayerUseCase(IGameService gameService, IDomainEventDispatcherService domainEventDispatcherService)
    {
        this.gameService = gameService;
        this.domainEventDispatcherService = domainEventDispatcherService;
    }

    public void Execute(MovePlayerCommand movePlayerCommand)
    {
        GameAggregate gameAggregate = this.gameService.GetById(movePlayerCommand.GameId)
            ?? throw new InvalidOperationException($"Game with Id {movePlayerCommand.GameId} not found");
        
        gameAggregate.MovePlayer(movePlayerCommand.PlayerId, movePlayerCommand.NewY);
        this.domainEventDispatcherService.Dispatch(gameAggregate);
    }
}
