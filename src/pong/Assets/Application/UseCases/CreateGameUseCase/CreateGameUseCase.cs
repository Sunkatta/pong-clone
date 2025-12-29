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
        var gameFieldValueObject = new GameFieldValueObject(
            new Position2DValueObject(createGameCommand.BottomLeftCornerPosition.Item1, createGameCommand.BottomLeftCornerPosition.Item2),
            new Position2DValueObject(createGameCommand.BottomRightCornerPosition.Item1, createGameCommand.BottomRightCornerPosition.Item2),
            new Position2DValueObject(createGameCommand.TopRightCornerPosition.Item1, createGameCommand.TopRightCornerPosition.Item2),
            new Position2DValueObject(createGameCommand.TopLeftCornerPosition.Item1, createGameCommand.TopLeftCornerPosition.Item2));

        GameAggregate gameAggregate = new GameAggregate(createGameCommand.GameId,
            player1Entity,
            player2Entity,
            gameFieldValueObject,
            createGameCommand.PaddleSpeed,
            createGameCommand.PaddleLength);

        this.gameService.Create(gameAggregate);
    }
}
