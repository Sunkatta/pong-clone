using System;

public class PlayerJoinedDomainEventHandler : IDomainEventHandler<PlayerJoinedDomainEvent>
{
    public event Action<PlayerJoinedDomainEvent> PlayerJoined;

    public void Handle(PlayerJoinedDomainEvent domainEvent)
    {
        this.PlayerJoined(domainEvent);
    }
}
