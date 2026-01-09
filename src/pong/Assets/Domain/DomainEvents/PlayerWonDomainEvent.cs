public class PlayerWonDomainEvent : IDomainEvent
{
    public PlayerWonDomainEvent(string winnerPlayerId, string winnerPlayerUsername, string loserPlayerId, string loserPlayerUsername)
    {
        this.WinnerPlayerId = winnerPlayerId;
        this.WinnerPlayerUsername = winnerPlayerUsername;
        this.LoserPlayerId = loserPlayerId;
        this.LoserPlayerUsername = loserPlayerUsername;
    }

    public string WinnerPlayerId { get; }

    public string WinnerPlayerUsername { get; }

    public string LoserPlayerId { get; }

    public string LoserPlayerUsername { get; }
}
