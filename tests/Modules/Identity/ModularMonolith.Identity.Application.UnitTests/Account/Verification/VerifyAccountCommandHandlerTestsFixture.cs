using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using ModularMonolith.Identity.Application.Account.Verification;
using ModularMonolith.Identity.Application.UnitTests.Account.Shared;
using ModularMonolith.Identity.Domain.Users;
using ModularMonolith.Shared.Events;
using ModularMonolith.Shared.Messaging;
using NSubstitute;

namespace ModularMonolith.Identity.Application.UnitTests.Account.Verification;

internal sealed class VerifyAccountCommandHandlerTestsFixture
{
    private readonly FakeUserManager _userManager = Substitute.For<FakeUserManager>();
    private readonly IMessageBus _eventBus = Substitute.For<IMessageBus>();

    private readonly ILogger<VerifyAccountCommandHandler> _logger =
        Substitute.For<ILogger<VerifyAccountCommandHandler>>();

    private User? _user;

    public VerifyAccountCommandHandler CreateSut() => new(_userManager,
        _eventBus,
        _logger);

    public void SetupUser(Guid id)
    {
        _user = new User("mail@mail.com") { Id = id };

        _userManager.FindByIdAsync(_user.Id.ToString())
            .Returns(_user);
    }

    public void SetupEmailConfirmation(string token)
    {
        _userManager.ConfirmEmailAsync(Arg.Any<User>(), Arg.Any<string>())
            .Returns(IdentityResult.Failed());

        _userManager.ConfirmEmailAsync(_user!, token)
            .Returns(IdentityResult.Success);
    }

    public Task AssertThatAccountVerifiedEventWasPublished() =>
        _eventBus.Received(1).PublishAsync(Arg.Is<AccountVerifiedEvent>(a => a.UserId == _user!.Id),
            Arg.Any<CancellationToken>());

    public Task AssertThatNoEventWasPublished() =>
        _eventBus.DidNotReceiveWithAnyArgs().PublishAsync(Arg.Any<IEvent>(), default);
}
