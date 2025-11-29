public class PlayerMovedDomainEvent : IDomainEvent
{
    public PlayerMovedDomainEvent(string playerId, Position2DValueObject newPlayerPosition)
    {
        this.PlayerId = playerId;
        this.NewPlayerPosition = newPlayerPosition;
    }

    public string PlayerId { get; }

    public Position2DValueObject NewPlayerPosition { get; }
}
