namespace ModularMonolith.Shared.Application.Exceptions;

public sealed class ConflictException : AppException
{
    public ConflictException(string propertyName, string errorCode, string message) : base(message)
    {
        PropertyName = propertyName;
        ErrorCode = errorCode;
    }

    public override string Code => "CONFLICT";

    public string PropertyName { get; }

    public string ErrorCode { get; }
}
