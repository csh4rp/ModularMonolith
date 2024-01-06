using System.Collections.Frozen;
using System.Text.Json;
using Microsoft.Extensions.Options;
using ModularMonolith.Shared.Domain.Abstractions;
using ModularMonolith.Shared.Infrastructure.Events.Options;

namespace ModularMonolith.Shared.Infrastructure.Events.Utils;

internal sealed class EventSerializer
{
    private readonly FrozenDictionary<string, Type> _typesDictionary;

    public EventSerializer(IOptionsMonitor<EventOptions> optionsMonitor) =>
        _typesDictionary = optionsMonitor.CurrentValue.Assemblies.SelectMany(a => a.GetTypes())
            .Where(t => t.IsAssignableTo(typeof(IEvent)))
            .ToFrozenDictionary(k => k.FullName!);

    public string Serialize<T>(T @event) where T : IEvent => JsonSerializer.Serialize(@event);
    
    public object Deserialize(string eventTypeName, string eventPayload)
    {
        var type = _typesDictionary[eventTypeName];

        return JsonSerializer.Deserialize(eventPayload, type)!;
    }

    public TEvent Deserialize<TEvent>(string eventPayload) => JsonSerializer.Deserialize<TEvent>(eventPayload)!;
}
