using System;

public class UpdateBallDirectionUseCase : IUpdateBallDirectionUseCase
{
    private readonly IGameService gameService;
    private readonly IDomainEventDispatcherService domainEventDispatcherService;

    public UpdateBallDirectionUseCase(IGameService gameService, IDomainEventDispatcherService domainEventDispatcherService)
    {
        this.gameService = gameService;
        this.domainEventDispatcherService = domainEventDispatcherService;
    }

    public void Execute(UpdateBallDirectionCommand updateBallDirectionCommand)
    {
        GameAggregate gameAggregate = this.gameService.GetById(updateBallDirectionCommand.GameId)
            ?? throw new InvalidOperationException($"Game with Id {updateBallDirectionCommand.GameId} not found");

        gameAggregate.UpdateBallDirection(new Position2DValueObject(updateBallDirectionCommand.NewDirection.X, updateBallDirectionCommand.NewDirection.Y),
            updateBallDirectionCommand.IsHitByPlayer);
        this.domainEventDispatcherService.Dispatch(gameAggregate);
    }
}
