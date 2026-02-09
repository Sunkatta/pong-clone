public class JoinGameCommand
{
    public JoinGameCommand(string gameId, string playerId, string username)
    {
        this.GameId = gameId;
        this.PlayerId = playerId;
        this.Username = username;
    }

    public string GameId { get; }

    public string PlayerId { get; }

    public string Username { get; }
}
