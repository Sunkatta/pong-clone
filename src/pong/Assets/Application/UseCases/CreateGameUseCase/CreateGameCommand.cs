public class CreateGameCommand
{
    public CreateGameCommand(string gameId,
        string player1Id,
        string player1Username,
        string player2Id,
        string player2Username,
        (float, float) bottomLeftCornerPosition,
        (float, float) bottomRightCornerPosition,
        (float, float) topRightCornerPosition,
        (float, float) topLeftCornerPosition,
        float paddleSped,
        float paddleLength)
    {
        this.GameId = gameId;
        this.Player1Id = player1Id;
        this.Player1Username = player1Username;
        this.Player2Id = player2Id;
        this.Player2Username = player2Username;
        this.BottomLeftCornerPosition = bottomLeftCornerPosition;
        this.BottomRightCornerPosition = bottomRightCornerPosition;
        this.TopRightCornerPosition = topRightCornerPosition;
        this.TopLeftCornerPosition = topLeftCornerPosition;
        this.PaddleSpeed = paddleSped;
        this.PaddleLength = paddleLength;
    }

    public string GameId { get; }

    public string Player1Id { get; }

    public string Player1Username { get; }

    public string Player2Id { get; }

    public string Player2Username { get; }

    public (float, float) BottomLeftCornerPosition { get; }

    public (float, float) BottomRightCornerPosition { get; }

    public (float, float) TopRightCornerPosition { get; }

    public (float, float) TopLeftCornerPosition { get; }

    public float PaddleSpeed { get; }

    public float PaddleLength { get; }
}
