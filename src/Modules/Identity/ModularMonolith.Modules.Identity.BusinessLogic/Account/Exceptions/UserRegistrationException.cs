using Microsoft.AspNetCore.Identity;
using ModularMonolith.Shared.BusinessLogic.Exceptions;

namespace ModularMonolith.Modules.Identity.BusinessLogic.Account.Exceptions;

public sealed class UserRegistrationException : AppException
{
    public UserRegistrationException(IEnumerable<IdentityError> errors)
        : base("One or more errors occured while registering user")
    {
        Data["Errors"] = errors.ToList();
    }

    public override string Code => "USER_REGISTRATION_ERROR";
}
