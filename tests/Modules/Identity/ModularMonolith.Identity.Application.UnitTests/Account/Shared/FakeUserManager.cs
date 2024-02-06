using Microsoft.AspNetCore.Identity;
using ModularMonolith.Identity.Domain.Users;
using NSubstitute;

namespace ModularMonolith.Identity.Application.UnitTests.Account.Shared;

public class FakeUserManager : UserManager<User>
{
    public FakeUserManager() :
        base(Substitute.For<IUserStore<User>>(), default!, default!, default!,
            default!, default!, default!, default!, default!)
    {
    }
}
