namespace ModularMonolith.Shared.Api.Models.Errors;

public class PropertyError
{
    public required string PropertyName { get; init; }
    
    public required string ErrorCode { get; init; }
    
    public required string Message { get; init; }
    
    public object? Parameter { get; init; }
}
