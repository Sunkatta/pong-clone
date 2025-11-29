public class GameFieldValueObject
{
    public GameFieldValueObject(Position2DValueObject bottomLeftCornerPosition,
        Position2DValueObject bottomRightCornerPosition,
        Position2DValueObject topRightCornerPosition,
        Position2DValueObject topLeftCornerPosition)
    {
        this.BottomLeftCornerPosition = bottomLeftCornerPosition;
        this.BottomRightCornerPosition = bottomRightCornerPosition;
        this.TopRightCornerPosition = topRightCornerPosition;
        this.TopLeftCornerPosition = topLeftCornerPosition;
    }

    public Position2DValueObject BottomLeftCornerPosition { get; }

    public Position2DValueObject BottomRightCornerPosition { get; }

    public Position2DValueObject TopRightCornerPosition { get; }

    public Position2DValueObject TopLeftCornerPosition { get; }
}
