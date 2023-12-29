using Microsoft.AspNetCore.Identity;
using ModularMonolith.Identity.Domain.Common.Entities;
using NSubstitute;

namespace ModularMonolith.Identity.Application.UnitTests.Account.Fakes;

public class FakeUserManager : UserManager<User>
{
    public FakeUserManager() : 
        base(Substitute.For<IUserStore<User>>() , default!, default!, default!, 
            default!, default!, default!, default!, default!)
    {
    }
}
