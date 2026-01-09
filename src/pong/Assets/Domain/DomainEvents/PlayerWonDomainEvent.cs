public class PlayerWonDomainEvent : IDomainEvent
{
    public PlayerWonDomainEvent(string winnerPlayerId, PlayerType winnerPlayerType)
    {
        this.WinnerPlayerId = winnerPlayerId;
        this.WinnerPlayerType = winnerPlayerType;
    }

    public string WinnerPlayerId { get; }

    public PlayerType WinnerPlayerType { get; }
}
