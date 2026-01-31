public class PlayerJoinedDomainEvent : IDomainEvent
{
    public PlayerJoinedDomainEvent(string playerId,
        string username,
        PlayerType playerType,
        float playerMinPositionY,
        float playerMaxPositionY)
    {
        this.PlayerId = playerId;
        this.Username = username;
        this.PlayerType = playerType;
        this.PlayerPositionMinY = playerMinPositionY;
        this.PlayerPositionMaxY = playerMaxPositionY;
    }

    public string PlayerId { get; }

    public string Username { get; }

    public PlayerType PlayerType { get; }

    public float PlayerPositionMinY { get; }

    public float PlayerPositionMaxY { get; }
}
