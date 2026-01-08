public class CreateGameUseCase : ICreateGameUseCase
{
    private readonly IGameService gameService;

    public CreateGameUseCase(IGameService gameService)
    {
        this.gameService = gameService;
    }

    public void Execute(CreateGameCommand createGameCommand)
    {
        var player1Entity = new PlayerEntity(createGameCommand.Player1Id, createGameCommand.Player1Username, PlayerType.Player1);
        var player2Entity = new PlayerEntity(createGameCommand.Player2Id, createGameCommand.Player2Username, PlayerType.Player2);

        var ballEntity = new BallEntity("1", 6, 15, new Position2DValueObject(0, 0), new Position2DValueObject(0, 0));

        var gameFieldValueObject = new GameFieldValueObject(
            new Position2DValueObject(createGameCommand.BottomLeftCornerPosition.X, createGameCommand.BottomLeftCornerPosition.Y),
            new Position2DValueObject(createGameCommand.BottomRightCornerPosition.X, createGameCommand.BottomRightCornerPosition.Y),
            new Position2DValueObject(createGameCommand.TopRightCornerPosition.X, createGameCommand.TopRightCornerPosition.Y),
            new Position2DValueObject(createGameCommand.TopLeftCornerPosition.X, createGameCommand.TopLeftCornerPosition.Y));

        GameAggregate gameAggregate = new GameAggregate(createGameCommand.GameId,
            player1Entity,
            player2Entity,
            ballEntity,
            gameFieldValueObject,
            createGameCommand.PaddleSpeed,
            createGameCommand.PaddleLength);

        this.gameService.Create(gameAggregate);
    }
}
