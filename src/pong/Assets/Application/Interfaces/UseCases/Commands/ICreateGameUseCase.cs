public interface ICreateGameUseCase
{
    GameModel Execute(CreateGameCommand createGameCommand);
}
