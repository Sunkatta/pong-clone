public class BallDirectionUpdatedDomainEvent : IDomainEvent
{
    public BallDirectionUpdatedDomainEvent(Position2DValueObject newDirection, float newSpeed)
    {
        this.NewDirection = newDirection;
        this.NewSpeed = newSpeed;
    }

    public Position2DValueObject NewDirection { get; }

    public float NewSpeed { get; }
}
