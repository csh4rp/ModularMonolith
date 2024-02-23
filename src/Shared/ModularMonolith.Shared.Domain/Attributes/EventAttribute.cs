namespace ModularMonolith.Shared.Domain.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class EventAttribute : Attribute
{
    public EventAttribute(string name) => Name = name;

    public string Name { get; set; }

    public string? Topic { get; set; }

    public bool IsPersisted { get; init; }
}
