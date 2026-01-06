public class MoveBallCommand
{
    public MoveBallCommand(string gameId, (float x, float y) newBallPosition)
    {
        this.GameId = gameId;
        this.NewBallPosition = newBallPosition;
    }

    public string GameId { get; }

    public (float X, float Y) NewBallPosition { get; }
}
