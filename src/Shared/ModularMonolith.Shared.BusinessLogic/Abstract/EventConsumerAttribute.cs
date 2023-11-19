namespace ModularMonolith.Shared.BusinessLogic.Abstract;

public class EventConsumerAttribute : Attribute
{
    public string? Queue { get; set; }
}
