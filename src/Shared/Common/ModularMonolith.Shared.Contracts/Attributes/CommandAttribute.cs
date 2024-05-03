namespace ModularMonolith.Shared.Contracts.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class CommandAttribute : Attribute
{
    public string? Target { get; set; }
}
