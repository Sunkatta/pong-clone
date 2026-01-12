public class GameService : IGameService
{
    private GameAggregate gameAggregate;

    public string Create(GameAggregate gameAggregate)
    {
        this.gameAggregate = gameAggregate;
        return gameAggregate.Id;
    }

    public GameAggregate GetById(string id)
    {
        if (this.gameAggregate == null || this.gameAggregate.Id != id)
        {
            return null;
        } 

        return this.gameAggregate;
    }
}
