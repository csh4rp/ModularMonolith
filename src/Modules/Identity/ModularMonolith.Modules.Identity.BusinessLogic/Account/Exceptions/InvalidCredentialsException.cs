using ModularMonolith.Shared.BusinessLogic.Exceptions;

namespace ModularMonolith.Modules.Identity.BusinessLogic.Account.Exceptions;

public sealed class InvalidCredentialsException : AppException
{
    public InvalidCredentialsException() : base("Username or password is invalid")
    {
    }

    public override string Code => "INVLID_CREDENTIALS";
}
