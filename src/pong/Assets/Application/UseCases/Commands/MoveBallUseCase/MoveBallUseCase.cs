using System;

public class MoveBallUseCase : IMoveBallUseCase
{
    private readonly IGameService gameService;
    private readonly IDomainEventDispatcherService domainEventDispatcherService;

    public MoveBallUseCase(IGameService gameService, IDomainEventDispatcherService domainEventDispatcherService)
    {
        this.gameService = gameService;
        this.domainEventDispatcherService = domainEventDispatcherService;
    }

    public void Execute(MoveBallCommand moveBallCommand)
    {
        GameAggregate gameAggregate = this.gameService.GetById(moveBallCommand.GameId)
            ?? throw new InvalidOperationException($"Game with Id {moveBallCommand.GameId} not found");

        gameAggregate.MoveBall(new Position2DValueObject(moveBallCommand.NewBallPosition.X, moveBallCommand.NewBallPosition.Y));
        this.domainEventDispatcherService.Dispatch(gameAggregate);
    }
}
