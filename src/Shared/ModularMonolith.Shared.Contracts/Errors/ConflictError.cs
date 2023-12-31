namespace ModularMonolith.Shared.Contracts.Errors;

public class ConflictError : Error
{
    public ConflictError(string target) : base(ErrorCodes.Conflict, "message")
    {
        Target = target;
    }
    
    public string Target { get; }
}
