namespace ModularMonolith.Shared.Application.Exceptions;

[Serializable]
public abstract class AppException : Exception
{
    protected AppException(string message) : base(message)
    {
    }

    public abstract string Code { get; }
}
