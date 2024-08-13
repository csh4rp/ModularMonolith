using Microsoft.AspNetCore.Identity;
using ModularMonolith.Identity.Application.Account.PasswordChange;
using ModularMonolith.Identity.Application.UnitTests.Account.Shared;
using ModularMonolith.Identity.Domain.Users;
using ModularMonolith.Shared.Events;
using ModularMonolith.Shared.Identity;
using ModularMonolith.Shared.Messaging;
using NSubstitute;

namespace ModularMonolith.Identity.Application.UnitTests.Account.PasswordChange;

internal sealed class ChangePasswordCommandHandlerTestsFixture
{
    private readonly FakeUserManager _userManager = Substitute.For<FakeUserManager>();
    private readonly IIdentityContextAccessor _identityContextAccessor = Substitute.For<IIdentityContextAccessor>();
    private readonly IMessageBus _eventBus = Substitute.For<IMessageBus>();

    private User? _user;
    private string? _password;

    public ChangePasswordCommandHandler CreateSut() => new(_userManager,
        _identityContextAccessor,
        _eventBus);

    public void SetupUser(Guid id, string password)
    {
        const string userName = "mail@mail.com";
        _user = new User(userName) { Id = id };
        _password = password;

        _userManager
            .FindByNameAsync(Arg.Is<string>(arg =>
                _user.UserName.Equals(arg, StringComparison.InvariantCultureIgnoreCase)))
            .Returns(_user);

        _userManager.ChangePasswordAsync(Arg.Any<User>(), Arg.Is<string>(arg => arg != _password), Arg.Any<string>())
            .Returns(IdentityResult.Failed(new IdentityError { Code = "PasswordMismatch" }));

        _userManager.ChangePasswordAsync(_user, Arg.Is<string>(a => a == _password), Arg.Any<string>())
            .Returns(IdentityResult.Success);

        _identityContextAccessor.IdentityContext.Returns(new IdentityContext(userName));
    }


    public Task AssertThatPasswordChangedEventWasPublished() => _eventBus.Received(1)
        .PublishAsync(Arg.Is<PasswordChangedEvent>(c => c.UserId == _user!.Id), Arg.Any<CancellationToken>());

    public Task AssertThatNoEventWasPublished() => _eventBus.DidNotReceiveWithAnyArgs()
        .PublishAsync(Arg.Any<IEvent>(), default);
}
