using System;

public class LeaveGameUseCase : ILeaveGameUseCase
{
    private readonly IGameService gameService;
    private readonly IDomainEventDispatcherService domainEventDispatcherService;

    public LeaveGameUseCase(IGameService gameService, IDomainEventDispatcherService domainEventDispatcherService)
    {
        this.gameService = gameService;
        this.domainEventDispatcherService = domainEventDispatcherService;
    }

    public void Execute(LeaveGameCommand leaveGameCommand)
    {
        GameAggregate gameAggregate = this.gameService.GetById(leaveGameCommand.GameId)
            ?? throw new InvalidOperationException($"Game with Id {leaveGameCommand.GameId} not found");

        gameAggregate.RemovePlayer(leaveGameCommand.PlayerId);
        this.domainEventDispatcherService.Dispatch(gameAggregate);
    }
}
