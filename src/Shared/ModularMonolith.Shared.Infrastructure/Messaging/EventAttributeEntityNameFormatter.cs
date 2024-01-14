using System.Reflection;
using MassTransit;
using ModularMonolith.Shared.Domain.Attributes;

namespace ModularMonolith.Shared.Infrastructure.Messaging;

public class EventAttributeEntityNameFormatter : IEntityNameFormatter
{
    public string FormatEntityName<T>()
    {
        var type = typeof(T);
        var attribute = type.GetCustomAttribute<EventAttribute>();

        return attribute?.Topic ?? type.Name;
    }
}
