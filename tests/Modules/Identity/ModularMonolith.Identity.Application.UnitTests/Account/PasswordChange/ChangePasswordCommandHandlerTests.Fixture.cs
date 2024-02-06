using Microsoft.AspNetCore.Identity;
using ModularMonolith.Identity.Application.Account.PasswordChange;
using ModularMonolith.Identity.Application.UnitTests.Account.Shared;
using ModularMonolith.Identity.Domain.Users;
using ModularMonolith.Shared.Application.Events;
using ModularMonolith.Shared.Application.Identity;
using NSubstitute;

namespace ModularMonolith.Identity.Application.UnitTests.Account.PasswordChange;

public partial class ChangePasswordCommandHandlerTests
{
    private class Fixture
    {
        private readonly FakeUserManager _userManager = Substitute.For<FakeUserManager>();
        private readonly IIdentityContextAccessor _identityContextAccessor = Substitute.For<IIdentityContextAccessor>();
        private readonly IEventBus _eventBus = Substitute.For<IEventBus>();

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
            
            _userManager.FindByIdAsync(_user.Id.ToString())
                .Returns(_user);
            
            _userManager.ChangePasswordAsync(Arg.Any<User>(), Arg.Any<string>(), Arg.Any<string>())
                .Returns(IdentityResult.Failed(new IdentityError{Code = "PasswordMismatch"}));
            
            _userManager.ChangePasswordAsync(_user, _password, Arg.Any<string>())
                .Returns(IdentityResult.Success);

            _identityContextAccessor.Context.Returns(new IdentityContext(id, userName));
        }

        public Task AssertThatPasswordChangedEventWasPublished() => _eventBus.Received(1)
            .PublishAsync(Arg.Is<PasswordChangedEvent>(c => c.UserId == _user!.Id), Arg.Any<CancellationToken>());
        
        public Task AssertThatNoEventWasPublished() => _eventBus.DidNotReceiveWithAnyArgs()
            .PublishAsync(default!, default);

    }
}
