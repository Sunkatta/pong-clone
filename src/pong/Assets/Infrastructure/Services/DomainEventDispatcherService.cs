using System;
using System.Collections.Generic;
using System.Linq;

public class DomainEventDispatcherService : IDomainEventDispatcherService
{
    private readonly IDictionary<Type, List<object>> domainEventHandlers;

    // The drawback of this is that we need to inject each
    // IEnumerable<IDomainEventHandler<{DomainEvent}>> separately.
    public DomainEventDispatcherService(IEnumerable<IDomainEventHandler<PlayerMovedDomainEvent>> playerMovedHandlers,
        IEnumerable<IDomainEventHandler<BallMovedDomainEvent>> ballMovedHandlers,
        IEnumerable<IDomainEventHandler<BallDirectionUpdatedDomainEvent>> ballDirectionUpdatedHandlers,
        IEnumerable<IDomainEventHandler<PlayerScoredDomainEvent>> playerScoredHandlers,
        IEnumerable<IDomainEventHandler<PlayerWonDomainEvent>> playerWonHandlers,
        IEnumerable<IDomainEventHandler<PlayerJoinedDomainEvent>> playerJoinedHandlers)
    {
        this.domainEventHandlers = new Dictionary<Type, List<object>>
        {
            {
                typeof(PlayerMovedDomainEvent),
                new List<object>(playerMovedHandlers)
            },
            {
                typeof(BallMovedDomainEvent),
                new List<object>(ballMovedHandlers)
            },
            {
                typeof(BallDirectionUpdatedDomainEvent),
                new List<object>(ballDirectionUpdatedHandlers)
            },
            {
                typeof(PlayerScoredDomainEvent),
                new List<object>(playerScoredHandlers)
            },
            {
                typeof(PlayerWonDomainEvent),
                new List<object>(playerWonHandlers)
            },
            {
                typeof(PlayerJoinedDomainEvent),
                new List<object>(playerJoinedHandlers)
            }
        };
    }

    public void Dispatch<T>(T aggregate) where T : Entity, IAggregateRoot
    {
        List<IDomainEvent> domainEvents = new List<IDomainEvent>(aggregate.DomainEvents);
        aggregate.ClearDomainEvents();

        // TODO: If entities hold entities, consider making this loop recursive.
        foreach (var property in aggregate.GetType().GetProperties())
        {
            var propertyType = property.PropertyType;

            // Handle single Entity property
            if (typeof(Entity).IsAssignableFrom(propertyType))
            {
                if (property.GetValue(aggregate) is Entity entity)
                {
                    domainEvents.AddRange(entity.DomainEvents);
                    entity.ClearDomainEvents();
                }

                continue;
            }

            // Handle a collection of Entities property
            var enumerableInterface = propertyType
                .GetInterfaces()
                .FirstOrDefault(interfaceType =>
                    interfaceType.IsGenericType &&
                    interfaceType.GetGenericTypeDefinition() == typeof(IEnumerable<>) &&
                    typeof(Entity).IsAssignableFrom(interfaceType.GetGenericArguments()[0]));

            if (enumerableInterface != null)
            {
                var entityCollection = property.GetValue(aggregate) as IEnumerable<Entity>;

                foreach (var entity in entityCollection)
                {
                    domainEvents.AddRange(entity.DomainEvents);
                    entity.ClearDomainEvents();
                }
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
