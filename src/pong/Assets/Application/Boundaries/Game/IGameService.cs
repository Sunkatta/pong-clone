public interface IGameService
{
    string Create(GameAggregate gameAggregate);

    GameAggregate GetById(string id);
}
