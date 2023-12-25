using ModularMonolith.Shared.Application.Exceptions;

namespace ModularMonolith.Identity.Application.Account.Exceptions;

public sealed class InvalidCredentialsException : AppException
{
    public InvalidCredentialsException() : base("Username or password is invalid")
    {
    }

    public override string Code => "INVALID_CREDENTIALS";
}
