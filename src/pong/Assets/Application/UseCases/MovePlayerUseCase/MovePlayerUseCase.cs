public class MovePlayerUseCase : IMovePlayerUseCase
{
    private readonly IGameService gameService;
    private readonly IDomainEventDispatcherService domainEventDispatcherService;

    public MovePlayerUseCase(IGameService gameService)
    {
        this.gameService = gameService;
    }

    public void Execute(MovePlayerCommand movePlayerCommand)
    {
        GameAggregate gameAggregate = this.gameService.GetGame(movePlayerCommand.GameId);

        gameAggregate.MovePlayer(movePlayerCommand.PlayerId, movePlayerCommand.NewY);
        this.domainEventDispatcherService.Dispatch(gameAggregate);
    }
}
