using System.Collections.Generic;

public abstract class Entity
{
    private readonly List<IDomainEvent> domainEvents;

    protected Entity()
    {
        this.domainEvents = new List<IDomainEvent>();
    }

    public IReadOnlyCollection<IDomainEvent> DomainEvents => domainEvents?.AsReadOnly();

    public void AddDomainEvent(IDomainEvent domainEvent)
    {
        domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        domainEvents?.Clear();
    }
}
