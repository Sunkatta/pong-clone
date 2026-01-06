using System;

public class BallMovedDomainEventHandler : IDomainEventHandler<BallMovedDomainEvent>
{
    public event Action<BallMovedDomainEvent> BallMoved;

    public void Handle(BallMovedDomainEvent domainEvent)
    {
        this.BallMoved(domainEvent);
    }
}
