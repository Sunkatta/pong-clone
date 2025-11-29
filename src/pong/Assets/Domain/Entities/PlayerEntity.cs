using System;

public class PlayerEntity : Entity
{
    public PlayerEntity(string id, string username, PlayerType playerType)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentNullException("Id cannot be null, empty or whitespace");
        }

        this.Id = id;
        
        if (string.IsNullOrWhiteSpace(username))
        {
            throw new ArgumentNullException("Username cannot be null, empty or whitespace");
        }
        
        this.Username = username;
        this.PlayerType = playerType;
    }

    public string Id { get; }

    public string Username { get; }

    public PlayerType PlayerType { get; }

    public int Score { get; private set; }

    public Position2DValueObject PaddlePosition { get; private set; }

    public void ScorePoint() => this.Score++;

    public void UpdatePosition(Position2DValueObject position)
    {
        if (position.X == this.PaddlePosition.X && position.Y == this.PaddlePosition.Y)
        {
            return;
        }

        this.PaddlePosition = new Position2DValueObject(position.X, position.Y);
        this.AddDomainEvent(new PlayerMovedDomainEvent(this.Id, this.PaddlePosition));
    }
}
