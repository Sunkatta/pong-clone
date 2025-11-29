public interface IDomainEventDispatcherService
{
    public void Dispatch(IAggregateRoot aggregate);
}
