using System;
using System.Collections.Generic;
using System.Linq;

public class GameAggregate : Entity, IAggregateRoot
{
    private const int MaxPlayersCount = 2;

    private readonly ICollection<PlayerEntity> players;

    public GameAggregate(string id,
        BallEntity ball,
        GameFieldValueObject gameFieldValueObject,
        float paddleSpeed,
        float paddleLength,
        int targetScore,
        ICollection<PlayerEntity> players = null)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentNullException(nameof(id), "Id cannot be null, empty or whitespace");
        }

        this.Id = id;
        this.Ball = ball ?? throw new ArgumentNullException(nameof(ball), "Ball cannot be null");

        if (paddleSpeed <= 0)
        {
            throw new ArgumentException("Paddle speed cannot be less than or equal to 0");
        }
        
        this.PaddleSpeed = paddleSpeed;

        if (paddleLength <= 0)
        {
            throw new ArgumentException("Paddle length cannot be less than or equal to 0");
        }

        this.PaddleLength = paddleLength;

        if (targetScore <= 0)
        {
            throw new ArgumentException("Target score cannot be less than or equal to 0");
        }

        this.TargetScore = targetScore;

        this.GameFieldValueObject = gameFieldValueObject ?? throw new ArgumentNullException(nameof(gameFieldValueObject), "Game field cannot be null");
        this.players = players ?? new List<PlayerEntity>();
    }

    public GameAggregate(BallEntity ball,
        GameFieldValueObject gameFieldValueObject,
        float paddleSpeed,
        float paddleLength,
        int targetScore)
        : this(Guid.NewGuid().ToString(), ball, gameFieldValueObject, paddleSpeed, paddleLength, targetScore)
    {
    }

    public string Id { get;}

    public IReadOnlyCollection<PlayerEntity> Players => new List<PlayerEntity>(this.players).AsReadOnly();

    public BallEntity Ball { get; }

    public float PaddleSpeed { get; }

    public float PaddleLength { get; }

    public int TargetScore { get; }

    public GameFieldValueObject GameFieldValueObject { get; }

    public void AddPlayer(PlayerEntity player)
    {
        if (this.players.Count == MaxPlayersCount)
        {
            throw new InvalidOperationException("Limit of players already reached");
        }

        if (this.players.Count == 0)
        {
            player.SetType(PlayerType.Player1);
        }

        if (this.players.Count == 1)
        {
            player.SetType(PlayerType.Player2);
        }

        this.players.Add(player);
        this.AddDomainEvent(new PlayerJoinedDomainEvent(player.Id, player.Username, player.PlayerType));
    }

    public void RemovePlayer(string playerId)
    {
        var player = this.players.FirstOrDefault(player => player.Id == playerId)
            ?? throw new ArgumentException($"Player with Id {playerId} not found");

        this.players.Remove(player);
        this.AddDomainEvent(new PlayerLeftDomainEvent(player.Id, player.Username));
    }

    public void MovePlayer(string playerId, float newY)
    {
        if (newY < this.GameFieldValueObject.BottomLeftCornerPosition.Y + (this.PaddleLength / 2) || newY > this.GameFieldValueObject.TopLeftCornerPosition.Y - (this.PaddleLength / 2))
        {
            // Player is outside the bounds of the game field. Do nothing.
            // Consider resetting them inside the game field.
            return;
        }

        var player = this.players.FirstOrDefault(player => player.Id == playerId)
            ?? throw new ArgumentException($"Player with Id {playerId} not found");

        player.UpdatePosition(new Position2DValueObject(player.PaddlePosition.X, newY));
    }

    public void MoveBall(Position2DValueObject newPosition)
    {
        if (newPosition.X > this.GameFieldValueObject.TopRightCornerPosition.X)
        {
            // Player 1 (left) scored.
            var player1 = this.players.FirstOrDefault(player => player.PlayerType == PlayerType.Player1)
                ?? throw new ArgumentException($"Player with Type {PlayerType.Player1} not found");
            var player2 = this.players.FirstOrDefault(player => player.PlayerType == PlayerType.Player2)
                ?? throw new ArgumentException($"Player with Type {PlayerType.Player2} not found");

            player1.ScorePoint();
            this.AddDomainEvent(new PlayerScoredDomainEvent(PlayerType.Player1, player1.Score));

            if (player1.Score == this.TargetScore)
            {
                this.AddDomainEvent(new PlayerWonDomainEvent(player1.Id, player1.Username, player2.Id, player2.Username));
                return;
            }

            this.Ball.UpdateSpeed(this.Ball.InitialSpeed);
            this.Ball.UpdatePosition(new Position2DValueObject(0, 0));
            return;
        }

        if (newPosition.X < this.GameFieldValueObject.TopLeftCornerPosition.X)
        {
            // Player 2 (right) scored.
            var player1 = this.players.FirstOrDefault(player => player.PlayerType == PlayerType.Player1)
                ?? throw new ArgumentException($"Player with Type {PlayerType.Player1} not found");
            var player2 = this.players.FirstOrDefault(player => player.PlayerType == PlayerType.Player2)
                ?? throw new ArgumentException($"Player with Type {PlayerType.Player2} not found");

            player2.ScorePoint();
            this.AddDomainEvent(new PlayerScoredDomainEvent(PlayerType.Player2, player2.Score));

            if (player2.Score == this.TargetScore)
            {
                this.AddDomainEvent(new PlayerWonDomainEvent(player2.Id, player2.Username, player1.Id, player1.Username));
                return;
            }

            this.Ball.UpdateSpeed(this.Ball.InitialSpeed);
            this.Ball.UpdatePosition(new Position2DValueObject(0, 0));
            return;
        }

        this.Ball.UpdatePosition(newPosition);
    }

    public void UpdateBallDirection(Position2DValueObject newDirection, bool hitByPlayer)
    {
        this.Ball.UpdateDirection(newDirection, hitByPlayer);
    }

    public Position2DValueObject GetBallDirection(string ballId)
    {
        if (this.Ball.Id != ballId)
        {
            throw new InvalidOperationException($"Cannot find ball with Id {ballId}");
        }

        return this.Ball.GetDirection();
    }
}
