public class UpdateBallDirectionCommand
{
    public UpdateBallDirectionCommand(string gameId, (float x, float y) newDirection, bool isHitByPlayer)
    {
        this.GameId = gameId;
        this.NewDirection = newDirection;
        this.IsHitByPlayer = isHitByPlayer;
        this.Scorer = null;
    }

    public UpdateBallDirectionCommand(string gameId, PlayerType scorer)
    {
        this.GameId = gameId;
        this.Scorer = scorer;
        this.NewDirection = null;
    }

    public string GameId { get; }

    public (float X, float Y)? NewDirection { get; }

    public bool IsHitByPlayer { get; }

    public PlayerType? Scorer { get; set; }
}
