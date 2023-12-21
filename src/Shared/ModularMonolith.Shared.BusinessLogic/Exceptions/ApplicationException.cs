namespace ModularMonolith.Shared.BusinessLogic.Exceptions;

public abstract class ApplicationException : Exception
{
    protected ApplicationException(string message) : base(message)
    {
    }

    public abstract string Code { get; }
}
