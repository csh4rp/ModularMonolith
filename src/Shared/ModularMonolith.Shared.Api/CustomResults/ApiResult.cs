using ModularMonolith.Shared.Contracts;
using ModularMonolith.Shared.Contracts.Errors;

namespace ModularMonolith.Shared.Api.CustomResults;

public class ApiResult
{
    public static IResult From(Result result) => result.IsSuccessful ? Results.NoContent() : Error(result.Error);

    public static IResult From<T>(Result<T> result) =>
        result.IsSuccessful ? Results.Ok(result.Value) : Error(result.Error);

    public static IResult From(Result<CreatedResponse> result) =>
        result.IsSuccessful ? new CreatedAtResult(result.Value) : Error(result.Error);

    private static IResult Error(Error error) =>
        error switch
        {
            MemberError memberError => new MemberErrorResult(memberError),
            ConflictError conflictError => new ConflictResult(conflictError),
            EntityNotFoundError entityNotFoundError => new NotFoundResult(entityNotFoundError),
            _ => Results.BadRequest()
        };
}
