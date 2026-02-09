using System;

public class PlayerLeftDomainEventHandler : IDomainEventHandler<PlayerLeftDomainEvent>
{
    public event Action<PlayerLeftDomainEvent> PlayerLeft;

    public void Handle(PlayerLeftDomainEvent domainEvent)
    {
        this.PlayerLeft(domainEvent);
    }
}
