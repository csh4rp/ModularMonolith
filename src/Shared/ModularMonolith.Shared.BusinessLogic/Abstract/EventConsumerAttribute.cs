namespace ModularMonolith.Shared.BusinessLogic.Abstract;

public sealed class EventConsumerAttribute : Attribute
{
    public string? Queue { get; set; }
}
