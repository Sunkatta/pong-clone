using System;

public class BallEntity : Entity
{
    public BallEntity(string id, float initialSpeed, Position2DValueObject initialDirection, Position2DValueObject initialPosition)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentNullException(nameof(id), "Id cannot be null, empty or whitespace");
        }

        if (initialSpeed <= 0)
        {
            throw new ArgumentException("Initial ball speed cannot be less than or equal to 0");
        }

        this.CurrentSpeed = initialSpeed;
        this.Direction = initialDirection;
        this.Position = initialPosition;
    }

    public string Id { get; }

    public float CurrentSpeed { get; private set; }

    public Position2DValueObject Direction { get; private set; }

    public Position2DValueObject Position { get; private set; }

    public void UpdatePosition(Position2DValueObject newPosition)
    {
        this.Position = newPosition;
        this.AddDomainEvent(new BallMovedDomainEvent(newPosition));
    }

    public void UpdateDirection(Position2DValueObject newDirection, bool hitByPlayer)
    {
        if (hitByPlayer)
        {
            this.CurrentSpeed++;
        }
        
        this.Direction = newDirection;
    }

    public void UpdateSpeed(float newSpeed)
    {
        this.CurrentSpeed = newSpeed;
    }
}
