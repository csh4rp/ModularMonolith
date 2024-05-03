namespace ModularMonolith.Shared.Application.Exceptions;

public abstract class ConflictException : ApplicationLogicException
{
    protected ConflictException(string message, string reference) : base(message)
    {
        Reference = reference;
    }

    public string Reference { get; }
}
