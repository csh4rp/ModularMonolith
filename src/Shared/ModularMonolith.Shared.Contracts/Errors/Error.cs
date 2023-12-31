namespace ModularMonolith.Shared.Contracts.Errors;

public abstract class Error
{
    protected Error(string code, string message)
    {
        Code = code;
        Message = message;
    }
    
    public string Code { get; protected set; }
    
    public string Message { get; protected set; }
}
