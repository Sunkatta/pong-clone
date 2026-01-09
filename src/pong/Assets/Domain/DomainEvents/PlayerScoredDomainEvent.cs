public class PlayerScoredDomainEvent : IDomainEvent
{
    public PlayerScoredDomainEvent(PlayerType playerType, int playerNewScore)
    {
        this.PlayerType = playerType;
        this.PlayerNewScore = playerNewScore;
    }

    public PlayerType PlayerType { get; }

    public int PlayerNewScore { get; }
}
