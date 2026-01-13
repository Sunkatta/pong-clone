using System;

public class CreateGameUseCase : ICreateGameUseCase
{
    private readonly Random random;
    private readonly IGameService gameService;
    private readonly IDomainEventDispatcherService domainEventDispatcherService;

    public CreateGameUseCase(Random random, IGameService gameService, IDomainEventDispatcherService domainEventDispatcherService)
    {
        this.random = random;
        this.gameService = gameService;
        this.domainEventDispatcherService = domainEventDispatcherService;
    }

    public GameModel Execute(CreateGameCommand createGameCommand)
    {
        var player1Entity = new PlayerEntity(createGameCommand.Player1Id, createGameCommand.Player1Username, PlayerType.Player1);
        var player2Entity = new PlayerEntity(createGameCommand.Player2Id, createGameCommand.Player2Username, PlayerType.Player2);

        // Randomise the ball direction on game start.
        var initialDirection = new Position2DValueObject(this.random.NextDouble() < 0.5 ? -1 : 1, (float)(this.random.NextDouble() * 2.0 - 1.0));

        var ballEntity = new BallEntity(createGameCommand.BallInitialSpeed,
            createGameCommand.BallMaximumSpeed,
            initialDirection,
            new Position2DValueObject(0, 0));

        var gameFieldValueObject = new GameFieldValueObject(
            new Position2DValueObject(createGameCommand.BottomLeftCornerPosition.X, createGameCommand.BottomLeftCornerPosition.Y),
            new Position2DValueObject(createGameCommand.BottomRightCornerPosition.X, createGameCommand.BottomRightCornerPosition.Y),
            new Position2DValueObject(createGameCommand.TopRightCornerPosition.X, createGameCommand.TopRightCornerPosition.Y),
            new Position2DValueObject(createGameCommand.TopLeftCornerPosition.X, createGameCommand.TopLeftCornerPosition.Y));

        GameAggregate gameAggregate = new GameAggregate(player1Entity,
            player2Entity,
            ballEntity,
            gameFieldValueObject,
            createGameCommand.PaddleSpeed,
            createGameCommand.PaddleLength,
            createGameCommand.TargetScore);

        var gameId = this.gameService.Create(gameAggregate);
        this.domainEventDispatcherService.Dispatch(gameAggregate);

        return new GameModel(gameId, ballEntity.Id);
    }
}
