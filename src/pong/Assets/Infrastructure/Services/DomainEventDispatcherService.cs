using System;
using System.Collections.Generic;

public class DomainEventDispatcherService : IDomainEventDispatcherService
{
    private readonly IDictionary<Type, List<object>> domainEventHandlers;

    // The drawback of this is that we need to inject each
    // IEnumerable<IDomainEventHandler<{DomainEvent}>> separately.
    public DomainEventDispatcherService(IEnumerable<IDomainEventHandler<PlayerMovedDomainEvent>> playerMovedHandlers)
    {
        domainEventHandlers = new Dictionary<Type, List<object>>
        {
            {
                typeof(PlayerMovedDomainEvent),
                new List<object>(playerMovedHandlers)
            }
        };
    }

    public void Dispatch<T>(T aggregate) where T : Entity, IAggregateRoot
    {
        List<IDomainEvent> domainEvents = new List<IDomainEvent>(aggregate.DomainEvents);
        aggregate.ClearDomainEvents();

        foreach (var property in aggregate.GetType().GetProperties())
        {
            // TODO: This only works for top level entities in the aggregate root.
            // In the future support for collection of entities could be added if the need arises.
            if (typeof(Entity).IsAssignableFrom(property.PropertyType))
            {
                if (property.GetValue(aggregate) is Entity entity)
                {
                    domainEvents.AddRange(entity.DomainEvents);
                    entity.ClearDomainEvents();
                }

                // TODO: If entities hold entities, consider making this loop recursive.
            }
        }

        foreach (var domainEvent in domainEvents)
        {
            var eventType = domainEvent.GetType();

            if (!domainEventHandlers.TryGetValue(eventType, out var handlers))
            {
                continue;
            }

            foreach (var handler in handlers)
            {
                var handlerInterface = typeof(IDomainEventHandler<>)
                    .MakeGenericType(eventType);

                var handleMethod = handlerInterface.GetMethod("Handle")
                    ?? throw new InvalidOperationException($"Handler {handler.GetType().Name} does not define Handle({eventType.Name}).");

                handleMethod.Invoke(handler, new object[] { domainEvent });
            }
        }
    }
}
