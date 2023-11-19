using System.Reflection;
using MassTransit;
using ModularMonolith.Shared.Domain.Attributes;

namespace ModularMonolith.Shared.Infrastructure.Messaging;

internal sealed class AttributeEntityNameFormatter : IEntityNameFormatter
{
    public string FormatEntityName<T>()
    {
        var type = typeof(T);
        var attribute = type.GetCustomAttribute<EventAttribute>();

        return attribute?.Topic ?? type.Name;
    }
}
