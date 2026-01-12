using System;

public class PlayerWonDomainEventHandler : IDomainEventHandler<PlayerWonDomainEvent>
{
    public event Action<PlayerWonDomainEvent> PlayerWon;

    public void Handle(PlayerWonDomainEvent domainEvent)
    {
        this.PlayerWon(domainEvent);
    }
}
