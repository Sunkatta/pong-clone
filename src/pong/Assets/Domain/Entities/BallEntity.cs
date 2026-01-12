using System;

public class BallEntity : Entity
{
    public BallEntity(string id, float initialSpeed, float maxSpeed, Position2DValueObject initialDirection, Position2DValueObject initialPosition)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentNullException(nameof(id), "Id cannot be null, empty or whitespace");
        }

        this.Id = id;

        if (initialSpeed <= 0)
        {
            throw new ArgumentException("Initial ball speed cannot be less than or equal to 0");
        }

        this.InitialSpeed = initialSpeed;
        this.CurrentSpeed = initialSpeed;

        if (maxSpeed <= 0)
        {
            throw new ArgumentException("Maximum ball speed cannot be less than or equal to 0");
        }

        this.MaxSpeed = maxSpeed;

        this.Direction = initialDirection;
        this.Position = initialPosition;
    }

    public string Id { get; }

    public float InitialSpeed { get; }

    public float CurrentSpeed { get; private set; }

    public float MaxSpeed { get; }

    public Position2DValueObject Direction { get; private set; }

    public Position2DValueObject Position { get; private set; }

    public void UpdatePosition(Position2DValueObject newPosition)
    {
        this.Position = newPosition;
        this.AddDomainEvent(new BallMovedDomainEvent(newPosition));
    }

    public void UpdateDirection(Position2DValueObject newDirection, bool isHitByPlayer)
    {
        if (isHitByPlayer && this.CurrentSpeed < this.MaxSpeed)
        {
            this.CurrentSpeed++;
        }
        
        this.Direction = newDirection;
        this.AddDomainEvent(new BallDirectionUpdatedDomainEvent(newDirection, this.CurrentSpeed));
    }

    public void UpdateSpeed(float newSpeed)
    {
        this.CurrentSpeed = newSpeed;
    }

    public Position2DValueObject GetDirection()
    {
        return this.Direction;
    }
}
