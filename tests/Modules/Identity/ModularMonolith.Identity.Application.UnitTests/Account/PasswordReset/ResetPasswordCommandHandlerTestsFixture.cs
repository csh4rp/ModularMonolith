using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using ModularMonolith.Identity.Application.Account.PasswordReset;
using ModularMonolith.Identity.Application.UnitTests.Account.Shared;
using ModularMonolith.Identity.Domain.Users;
using ModularMonolith.Shared.Messaging;
using NSubstitute;

namespace ModularMonolith.Identity.Application.UnitTests.Account.PasswordReset;

internal sealed class ResetPasswordCommandHandlerTestsFixture
{
    private readonly FakeUserManager _userManager = Substitute.For<FakeUserManager>();
    private readonly IMessageBus _messageBus = Substitute.For<IMessageBus>();

    private readonly ILogger<ResetPasswordCommandHandler> _logger =
        Substitute.For<ILogger<ResetPasswordCommandHandler>>();

    private User? _user;

    public ResetPasswordCommandHandler CreateSut() => new(_userManager, _messageBus, _logger);

    public void SetupUser(Guid id)
    {
        _user = new User("mail@mail.com") { Id = id };

        _userManager.FindByIdAsync(id.ToString())
            .Returns(_user);
    }

    public void SetupPasswordReset(string token, IdentityError identityError)
    {
        _userManager.ResetPasswordAsync(Arg.Any<User>(), Arg.Any<string>(), Arg.Any<string>())
            .Returns(IdentityResult.Failed(identityError));

        _userManager.ResetPasswordAsync(_user!, token, Arg.Any<string>())
            .Returns(IdentityResult.Success);
    }

    public Task AssertThatPasswordResetEventWasPublished() => _messageBus.Received(1)
        .PublishAsync(Arg.Is<PasswordResetEvent>(u => u.UserId == _user!.Id), Arg.Any<CancellationToken>());

    public Task AssertThatNoEventWasPublished() => _messageBus.DidNotReceiveWithAnyArgs().PublishAsync(default!, default);
}
