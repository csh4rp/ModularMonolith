namespace ModularMonolith.Shared.Application.Exceptions;

public abstract class ConflictException : AppException
{
    protected ConflictException(string message, string reference) : base(message)
    {
        Reference = reference;
    }

    public string Reference { get; }
}
