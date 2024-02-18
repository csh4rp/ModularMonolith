namespace ModularMonolith.Shared.Domain.Abstractions;

[Serializable]
public abstract class DomainException : Exception
{
    protected DomainException(string? message) : base(message)
    {
    }

    protected DomainException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
    
    public abstract string Code { get; }
}
