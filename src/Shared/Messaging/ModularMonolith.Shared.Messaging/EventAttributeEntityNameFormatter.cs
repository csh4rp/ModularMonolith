using System.Reflection;
using MassTransit;
using ModularMonolith.Shared.Events;

namespace ModularMonolith.Shared.Messaging;

public class EventAttributeEntityNameFormatter : IEntityNameFormatter
{
    public string FormatEntityName<T>()
    {
        var type = typeof(T);
        var attribute = type.GetCustomAttribute<EventAttribute>();

        return attribute?.Topic ?? type.Name;
    }
}
