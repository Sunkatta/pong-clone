using System.Collections.Generic;

public abstract class Entity
{
    private List<IDomainEvent> domainEvents;

    public IReadOnlyCollection<IDomainEvent> DomainEvents => domainEvents?.AsReadOnly();

    public void AddDomainEvent(IDomainEvent domainEvent)
    {
        domainEvents ??= new List<IDomainEvent>();
        domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        domainEvents?.Clear();
    }
}
