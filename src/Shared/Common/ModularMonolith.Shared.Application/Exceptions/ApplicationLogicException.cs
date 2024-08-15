namespace ModularMonolith.Shared.Application.Exceptions;

[Serializable]
public abstract class ApplicationLogicException : Exception
{
    protected ApplicationLogicException(string message) : base(message)
    {
    }

    protected ApplicationLogicException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    public abstract string Code { get; }
}
