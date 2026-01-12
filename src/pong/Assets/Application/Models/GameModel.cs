public class GameModel
{
    public GameModel(string gameId, string ballId)
    {
        this.GameId = gameId;
        this.BallId = ballId;
    }

    public string GameId { get; set; }

    public string BallId { get; set; }
}
