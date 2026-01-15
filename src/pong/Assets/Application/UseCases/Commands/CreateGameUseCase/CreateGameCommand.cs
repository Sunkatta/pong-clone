public class CreateGameCommand
{
    public CreateGameCommand((float x, float y) bottomLeftCornerPosition,
        (float x, float y) bottomRightCornerPosition,
        (float x, float y) topRightCornerPosition,
        (float x, float y) topLeftCornerPosition,
        float paddleSpeed,
        float paddleLength,
        int targetScore,
        float ballInitialSpeed,
        float ballMaximumSpeed)
    {
        this.BottomLeftCornerPosition = bottomLeftCornerPosition;
        this.BottomRightCornerPosition = bottomRightCornerPosition;
        this.TopRightCornerPosition = topRightCornerPosition;
        this.TopLeftCornerPosition = topLeftCornerPosition;
        this.PaddleSpeed = paddleSpeed;
        this.PaddleLength = paddleLength;
        this.TargetScore = targetScore;
        this.BallInitialSpeed = ballInitialSpeed;
        this.BallMaximumSpeed = ballMaximumSpeed;
    }

    public (float X, float Y) BottomLeftCornerPosition { get; }

    public (float X, float Y) BottomRightCornerPosition { get; }

    public (float X, float Y) TopRightCornerPosition { get; }

    public (float X, float Y) TopLeftCornerPosition { get; }

    public float PaddleSpeed { get; }

    public float PaddleLength { get; }

    public int TargetScore { get; }

    public float BallInitialSpeed { get; }

    public float BallMaximumSpeed { get; }
}
