using System;

public class GameAggregate : Entity, IAggregateRoot
{
    public GameAggregate(string id,
        PlayerEntity player1,
        PlayerEntity player2,
        BallEntity ball,
        GameFieldValueObject gameFieldValueObject,
        float paddleSpeed,
        float paddleLength)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentNullException(nameof(id), "Id cannot be null, empty or whitespace");
        }

        this.Id = id;

        this.Player1 = player1 ?? throw new ArgumentNullException(nameof(player1), "Player 1 cannot be null");
        this.Player2 = player2 ?? throw new ArgumentNullException(nameof(player2), "Player 2 cannot be null");
        this.Ball = ball ?? throw new ArgumentNullException(nameof(ball), "Ball cannot be null");
        
        this.PaddleSpeed = paddleSpeed;

        if (paddleLength <= 0)
        {
            throw new ArgumentException("Paddle length cannot be less than or equal to 0");
        }

        this.PaddleLength = paddleLength;

        this.GameFieldValueObject = gameFieldValueObject ?? throw new ArgumentNullException(nameof(gameFieldValueObject), "Game field cannot be null");
    }

    public GameAggregate(PlayerEntity player1,
        PlayerEntity player2,
        BallEntity ball,
        GameFieldValueObject gameFieldValueObject,
        float paddleSpeed,
        float paddleLength)
        : this(Guid.NewGuid().ToString(), player1, player2, ball, gameFieldValueObject, paddleSpeed, paddleLength)
    {
    }

    public string Id { get;}

    public PlayerEntity Player1 { get; }

    public PlayerEntity Player2 { get; }

    public BallEntity Ball { get; }

    public float PaddleSpeed { get; }

    public float PaddleLength { get; }

    public GameFieldValueObject GameFieldValueObject { get; }

    public void MovePlayer(string playerId, float newY)
    {
        if (newY < this.GameFieldValueObject.BottomLeftCornerPosition.Y + (this.PaddleLength / 2) || newY > this.GameFieldValueObject.TopLeftCornerPosition.Y - (this.PaddleLength / 2))
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
        else
        {
            throw new ArgumentException($"Player with Id {playerId} not found");
        }
    }

    public void MoveBall(Position2DValueObject newPosition)
    {
        if (newPosition.X > this.GameFieldValueObject.TopRightCornerPosition.X)
        {
            // Player 1 (left) scored.
            return;
        }

        if (newPosition.X < this.GameFieldValueObject.TopLeftCornerPosition.X)
        {
            // Player 2 (right) scored.
            return;
        }

        this.Ball.UpdatePosition(newPosition);
    }

    public void UpdateBallDirection(Position2DValueObject newDirection, bool hitByPlayer)
    {
        this.Ball.UpdateDirection(newDirection, hitByPlayer);
    }
}
