public class MovePlayerCommand
{
    public MovePlayerCommand(string gameId, string playerId, float newY)
    {
        this.GameId = gameId;
        this.PlayerId = playerId;
        this.NewY = newY;
    }

    public string GameId { get; set; }

    public string PlayerId { get; }

    public float NewY { get; }
}
