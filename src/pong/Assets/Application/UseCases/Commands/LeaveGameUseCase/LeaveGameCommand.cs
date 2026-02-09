public class LeaveGameCommand
{
    public LeaveGameCommand(string gameId, string playerId)
    {
        this.GameId = gameId;
        this.PlayerId = playerId;
    }

    public string GameId { get; }

    public string PlayerId { get; }
}
