﻿using Microsoft.AspNetCore.Identity;
using ModularMonolith.Identity.Application.Account.Registration;
using ModularMonolith.Identity.Application.UnitTests.Account.Shared;
using ModularMonolith.Identity.Domain.Users;
using ModularMonolith.Shared.Events;
using NSubstitute;

namespace ModularMonolith.Identity.Application.UnitTests.Account.Registration;

internal sealed class RegisterCommandHandlerTestsFixture
{
    private readonly FakeUserManager _userManager = Substitute.For<FakeUserManager>();
    private readonly IEventBus _eventBus = Substitute.For<IEventBus>();

    public RegisterCommandHandler CreateSut() => new(_userManager, _eventBus);

    public void SetupUser(string email)
    {
        var user = new User(email);

        _userManager.FindByEmailAsync(email)
            .Returns(user);
    }

    public void SetupUserCreationToSucceed()
    {
        _userManager.FindByEmailAsync(Arg.Any<string>())
            .Returns((User?)null);

        _userManager.CreateAsync(Arg.Any<User>(), Arg.Any<string>())
            .Returns(IdentityResult.Success);
    }

    public void SetupUserCreationToFailWithError(IdentityError error)
    {
        _userManager.FindByEmailAsync(Arg.Any<string>())
            .Returns((User?)null);

        _userManager.CreateAsync(Arg.Any<User>(), Arg.Any<string>())
            .Returns(IdentityResult.Failed(error));
    }

    public Task AssertThatUserRegisteredEventWasPublished() => _eventBus.Received(1)
        .PublishAsync(Arg.Any<UserRegisteredEvent>(), Arg.Any<CancellationToken>());

    public Task AssertThatNoEventWasPublished() => _eventBus.DidNotReceiveWithAnyArgs().PublishAsync(default!, default);
}
