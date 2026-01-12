public class UpdateBallDirectionCommand
{
    public UpdateBallDirectionCommand(string gameId, (float x, float y) newDirection, bool isHitByPlayer)
    {
        this.GameId = gameId;
        this.NewDirection = newDirection;
        this.IsHitByPlayer = isHitByPlayer;
    }

    public string GameId { get; }

    public (float X, float Y) NewDirection { get; set; }

    public bool IsHitByPlayer { get; }
}
