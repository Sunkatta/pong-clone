public class GameService : IGameService
{
    private GameAggregate gameAggregate;

    public void Create(GameAggregate gameAggregate)
    {
        this.gameAggregate = gameAggregate;
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
