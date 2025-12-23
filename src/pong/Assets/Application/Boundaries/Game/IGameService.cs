public interface IGameService
{
    void Create(GameAggregate gameAggregate);

    GameAggregate GetById(string id);
}
