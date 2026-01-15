public class PlayerLeftDomainEvent : IDomainEvent
{
    public PlayerLeftDomainEvent(string playerId, string username)
    {
        this.PlayerId = playerId;
        this.Username = username;
    }

    public string PlayerId { get; }

    public string Username { get; }
}
