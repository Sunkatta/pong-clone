public interface IDomainEventDispatcherService
{
    public void Dispatch<T>(T aggregate) where T : Entity, IAggregateRoot;
}
