using System;

public class BallDirectionUpdatedDomainEventHandler : IDomainEventHandler<BallDirectionUpdatedDomainEvent>
{
    public event Action<BallDirectionUpdatedDomainEvent> BallDirectionUpdated;

    public void Handle(BallDirectionUpdatedDomainEvent domainEvent)
    {
        this.BallDirectionUpdated(domainEvent);
    }
}
