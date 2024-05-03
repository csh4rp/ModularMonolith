using Microsoft.Extensions.Logging;
using ModularMonolith.Identity.Application.Account.PasswordReset;
using ModularMonolith.Identity.Application.UnitTests.Account.Shared;
using ModularMonolith.Identity.Domain.Users;
using ModularMonolith.Shared.Messaging;
using NSubstitute;

namespace ModularMonolith.Identity.Application.UnitTests.Account.PasswordReset;

internal sealed class InitializePasswordResetCommandHandlerTestsFixture
{
    private readonly FakeUserManager _userManager = Substitute.For<FakeUserManager>();
    private readonly IMessageBus _messageBus = Substitute.For<IMessageBus>();

    private readonly ILogger<InitializePasswordResetCommandHandler> _logger =
        Substitute.For<ILogger<InitializePasswordResetCommandHandler>>();

    private User? _user;

    public InitializePasswordResetCommandHandler CreateSut() => new(
        _userManager,
        _messageBus,
        _logger);

    public void SetupUser(string email)
    {
        _user = new User(email) { Id = Guid.NewGuid() };
        _userManager.FindByEmailAsync(email)
            .Returns(_user);
    }

    public Task AssertThatResetPasswordInitializedEventWasPublished() =>
        _messageBus.Received(1)
            .PublishAsync(Arg.Is<PasswordResetInitializedEvent>(e => e.UserId == _user!.Id), default);

    public Task AssertThatNoEventWasPublished() => _messageBus.DidNotReceiveWithAnyArgs().PublishAsync(default!, default);
}
