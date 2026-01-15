public class PlayerJoinedDomainEvent : IDomainEvent
{
    public PlayerJoinedDomainEvent(string playerId, string username, PlayerType playerType)
    {
        this.PlayerId = playerId;
        this.Username = username;
        this.PlayerType = playerType;
    }

    public string PlayerId { get; }

    public string Username { get; }

    public PlayerType PlayerType { get; }
}
