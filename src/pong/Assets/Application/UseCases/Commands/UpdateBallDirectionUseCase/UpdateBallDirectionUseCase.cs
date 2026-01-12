using System;

public class UpdateBallDirectionUseCase : IUpdateBallDirectionUseCase
{
    private readonly Random random;
    private readonly IGameService gameService;
    private readonly IDomainEventDispatcherService domainEventDispatcherService;

    public UpdateBallDirectionUseCase(Random random, IGameService gameService, IDomainEventDispatcherService domainEventDispatcherService)
    {
        this.random = random;
        this.gameService = gameService;
        this.domainEventDispatcherService = domainEventDispatcherService;
    }

    public void Execute(UpdateBallDirectionCommand updateBallDirectionCommand)
    {
        GameAggregate gameAggregate = this.gameService.GetById(updateBallDirectionCommand.GameId)
            ?? throw new InvalidOperationException($"Game with Id {updateBallDirectionCommand.GameId} not found");

        if (updateBallDirectionCommand.Scorer != null)
        {
            var isPlayer1 = updateBallDirectionCommand.Scorer == PlayerType.Player1;
            var randomDirection = new Position2DValueObject(isPlayer1 ? 1 : -1, (float)(this.random.NextDouble() * 2.0 - 1.0));
            gameAggregate.UpdateBallDirection(randomDirection, updateBallDirectionCommand.IsHitByPlayer);
        }
        else
        {
            gameAggregate.UpdateBallDirection(new Position2DValueObject(updateBallDirectionCommand.NewDirection.Value!.X, updateBallDirectionCommand.NewDirection.Value!.Y),
                updateBallDirectionCommand.IsHitByPlayer);
        }
            
        this.domainEventDispatcherService.Dispatch(gameAggregate);
    }
}
