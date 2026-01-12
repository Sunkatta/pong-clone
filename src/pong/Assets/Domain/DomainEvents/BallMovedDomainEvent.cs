public class BallMovedDomainEvent : IDomainEvent
{
    public BallMovedDomainEvent(Position2DValueObject newPosition)
    {
        this.NewPosition = newPosition;
    }

    public Position2DValueObject NewPosition { get; }
}
