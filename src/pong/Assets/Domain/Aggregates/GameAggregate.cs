using System;

public class GameAggregate : Entity, IAggregateRoot
{
    public GameAggregate(string id,
        PlayerEntity player1,
        PlayerEntity player2,
        GameFieldValueObject gameFieldValueObject,
        float paddleSpeed)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentNullException(nameof(id), "Id cannot be null, empty or whitespace");
        }

        this.Player1 = player1 ?? throw new ArgumentNullException(nameof(player1), "Player 1 cannot be null");
        this.Player2 = player2 ?? throw new ArgumentNullException(nameof(player2), "Player 2 cannot be null");
        
        this.PaddleSpeed = paddleSpeed;

        this.GameFieldValueObject = gameFieldValueObject ?? throw new ArgumentNullException(nameof(gameFieldValueObject), "Game field cannot be null");
    }

    public string Id { get;}

    public PlayerEntity Player1 { get; }

    public PlayerEntity Player2 { get; }

    public float PaddleSpeed { get; }

    public GameFieldValueObject GameFieldValueObject { get; }

    public void MovePlayer(string playerId, float newY)
    {
        if (newY < this.GameFieldValueObject.BottomLeftCornerPosition.Y || newY > this.GameFieldValueObject.TopLeftCornerPosition.Y)
        {
            // Player is outside the bounds of the game field. Do nothing.
            // Consider resetting them inside the game field.
            return;
        }

        if (playerId == this.Player1.Id)
        {
            this.Player1.UpdatePosition(new Position2DValueObject(this.Player1.PaddlePosition.X, newY));
        }
        else if (playerId == Player2.Id)
        {
            this.Player2.UpdatePosition(new Position2DValueObject(this.Player2.PaddlePosition.X, newY));
        }
    }
}
