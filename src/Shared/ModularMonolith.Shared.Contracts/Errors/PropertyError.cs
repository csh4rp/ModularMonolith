namespace ModularMonolith.Shared.Contracts.Errors;

public class PropertyError
{
    public required string PropertyName { get; init; }

    public required string ErrorCode { get; init; }

    public required string Message { get; init; }

    public object? Parameter { get; init; }

    public static PropertyError NotUnique(string propertyName, string parameter) => new()
    {
        PropertyName = propertyName,
        ErrorCode = "NOT_UNIQUE",
        Message = $"Item with {propertyName} equal to {parameter} already exists",
        Parameter = parameter
    };
}
