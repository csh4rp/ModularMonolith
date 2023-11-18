namespace ModularMonolith.Shared.Api.Models.Errors;

public interface IErrorResponse
{
    int Status { get; }
    
    string Code { get; }
    
    string Message { get; }
}
