using System;

public class PlayerMovedDomainEventHandler : IDomainEventHandler<PlayerMovedDomainEvent>
{
    public event Action<PlayerMovedDomainEvent> PlayerMoved;

    public void Handle(PlayerMovedDomainEvent domainEvent)
    {
        this.PlayerMoved(domainEvent);
    }
}
