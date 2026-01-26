using System;

public class PlayerEntity : Entity
{
    public PlayerEntity(string id, string username, PlayerType playerType = default)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentNullException(nameof(id), "Id cannot be null, empty or whitespace");
        }

        this.Id = id;
        
        if (string.IsNullOrWhiteSpace(username))
        {
            throw new ArgumentNullException(nameof(username), "Username cannot be null, empty or whitespace");
        }
        
        this.Username = username;
        this.PlayerType = playerType;
    }

    public PlayerEntity(string username, PlayerType playerType = default)
        : this(Guid.NewGuid().ToString(), username, playerType)
    {
    }

    public string Id { get; }

    public string Username { get; }

    public PlayerType PlayerType { get; private set; }

    public int Score { get; private set; }

    public Position2DValueObject PaddlePosition { get; private set; }

    public void ScorePoint() => this.Score++;

    public void SetType(PlayerType playerType)
    {
        this.PlayerType = playerType;
    }

    public void UpdatePosition(Position2DValueObject position)
    {
        this.PaddlePosition = new Position2DValueObject(position.X, position.Y);
        this.AddDomainEvent(new PlayerMovedDomainEvent(this.Id, this.PaddlePosition));
    }
}
