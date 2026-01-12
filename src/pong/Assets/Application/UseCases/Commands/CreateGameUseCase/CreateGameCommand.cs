public class CreateGameCommand
{
    public CreateGameCommand(string player1Id,
        string player1Username,
        string player2Id,
        string player2Username,
        (float x, float y) bottomLeftCornerPosition,
        (float x, float y) bottomRightCornerPosition,
        (float x, float y) topRightCornerPosition,
        (float x, float y) topLeftCornerPosition,
        float paddleSped,
        float paddleLength,
        int targetScore)
    {
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
        this.TargetScore = targetScore;
    }

    public string Player1Id { get; }

    public string Player1Username { get; }

    public string Player2Id { get; }

    public string Player2Username { get; }

    public (float X, float Y) BottomLeftCornerPosition { get; }

    public (float X, float Y) BottomRightCornerPosition { get; }

    public (float X, float Y) TopRightCornerPosition { get; }

    public (float X, float Y) TopLeftCornerPosition { get; }

    public float PaddleSpeed { get; }

    public float PaddleLength { get; }

    public int TargetScore { get; }
}
