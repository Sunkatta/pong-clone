public class MovePlayerUseCase : IMovePlayerUseCase
{
    private readonly IGameService gameService;
    private readonly IDomainEventDispatcherService domainEventDispatcherService;

    public MovePlayerUseCase(IGameService gameService, IDomainEventDispatcherService domainEventDispatcherService)
    {
        this.gameService = gameService;
        this.domainEventDispatcherService = domainEventDispatcherService;
    }

    public void Execute(MovePlayerCommand movePlayerCommand)
    {
        GameAggregate gameAggregate = this.gameService.GetById(movePlayerCommand.GameId);

        gameAggregate.MovePlayer(movePlayerCommand.PlayerId, movePlayerCommand.NewY);
        this.domainEventDispatcherService.Dispatch(gameAggregate);
    }
}
