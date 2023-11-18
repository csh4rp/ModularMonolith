using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using ModularMonolith.Shared.Contracts;
using ModularMonolith.Shared.Domain.Abstractions;

namespace ModularMonolith.Shared.Infrastructure.Events;

public sealed class EventMapper
{
    private readonly FrozenDictionary<Type, Func<object, IIntegrationEvent>> _eventMappings;

    public EventMapper(Assembly[] assemblies) =>
        _eventMappings = assemblies.SelectMany(a => a.GetTypes())
            .Where(t => t.IsGenericType && t.GetGenericTypeDefinition().IsAssignableTo(typeof(IEventMapping<>)))
            .ToFrozenDictionary(t => t, t =>
            {
                var mappingCtor = t.GetConstructors(BindingFlags.Default | BindingFlags.Public)[0];
                var eventType = t.GenericTypeArguments[0];
                var method = t.GetMethod(nameof(IEventMapping<IEvent>.Map))!;
                
                var eventParameter = Expression.Parameter(typeof(object), "ev");

                var convertExpression = Expression.Convert(eventParameter, eventType);
                var creationExpression = Expression.New(mappingCtor);

                var invocationExpression = Expression.Call(method, creationExpression,convertExpression);

                return Expression.Lambda<Func<object, IIntegrationEvent>>(invocationExpression,  eventParameter).Compile();
            });

    public bool TryMap(object @event, [NotNullWhen(true)] out IIntegrationEvent? integrationEvent) 
    {
        if (_eventMappings.TryGetValue(@event.GetType(), out var mapping))
        {
            integrationEvent = mapping(@event);
            return true;
        }

        integrationEvent = null;
        return false;
    }
}
