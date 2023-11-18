using System.Collections.Frozen;
using System.Reflection;
using System.Text.Json;
using ModularMonolith.Shared.Domain.Abstractions;

namespace ModularMonolith.Shared.Infrastructure.Events;

public sealed class EventSerializer
{
    private readonly FrozenDictionary<string, Type> _typesDictionary;

    public EventSerializer(Assembly[] assemblies) =>
        _typesDictionary = assemblies.SelectMany(a => a.GetTypes())
            .Where(t => t.IsAssignableTo(typeof(IEvent)))
            .ToFrozenDictionary(k => k.FullName!);

    public string Serialize<T>(T @event) where T : IEvent => JsonSerializer.Serialize(@event);
    
    public string Serialize(object @event, Type eventType) => JsonSerializer.Serialize(@event, eventType);

    public object Deserialize(string eventTypeName, string eventPayload)
    {
        var type = _typesDictionary[eventTypeName];
        
        return JsonSerializer.Deserialize(eventPayload, type)!;
    }
}
