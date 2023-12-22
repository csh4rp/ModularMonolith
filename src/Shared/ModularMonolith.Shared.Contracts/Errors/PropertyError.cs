namespace ModularMonolith.Shared.Contracts.Errors;

public class PropertyError
{
    public required string PropertyName { get; init; }

    public required string ErrorCode { get; init; }

    public required string Message { get; init; }

    public object? Parameter { get; init; }

    public static PropertyError NotUnique(string propertyName, object? parameter) => new()
    {
        PropertyName = propertyName,
        ErrorCode = "NOT_UNIQUE",
        Message = $"Item with {propertyName} equal to {parameter} already exists",
        Parameter = parameter
    };
    
    public static PropertyError InvalidArgument(string propertyName, object? parameter) => new()
    {
        PropertyName = propertyName,
        ErrorCode = "INVALID_ARGUMENT",
        Message = $"Value for  {propertyName} is invalid",
        Parameter = parameter
    };

}
