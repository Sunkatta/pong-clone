using System;

public class GetBallDirectionQuery : IGetBallDirectionQuery
{
    private readonly IGameService gameService;

    public GetBallDirectionQuery(IGameService gameService)
    {
        this.gameService = gameService;
    }

    public (float X, float Y) Execute(string gameId, string ballId)
    {
        GameAggregate gameAggregate = this.gameService.GetById(gameId)
            ?? throw new InvalidOperationException($"Game with Id {gameId} not found");

        var ballDirection = gameAggregate.GetBallDirection(ballId);

        return (ballDirection.X, ballDirection.Y);
    }
}
