namespace ModularMonolith.Shared.BusinessLogic.Exceptions;

public abstract class AppException : Exception
{
    protected AppException(string message) : base(message)
    {
    }

    public abstract string Code { get; }
}
