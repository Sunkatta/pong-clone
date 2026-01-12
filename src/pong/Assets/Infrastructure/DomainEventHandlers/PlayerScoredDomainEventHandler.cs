using System;

public class PlayerScoredDomainEventHandler : IDomainEventHandler<PlayerScoredDomainEvent>
{
    public event Action<PlayerScoredDomainEvent> PlayerScored;

    public void Handle(PlayerScoredDomainEvent domainEvent)
    {
        this.PlayerScored(domainEvent);
    }
}
