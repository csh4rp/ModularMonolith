using Microsoft.Extensions.Options;
using ModularMonolith.Identity.Application.Account.SigningIn;
using ModularMonolith.Identity.Application.UnitTests.Account.Shared;
using ModularMonolith.Identity.Core.Options;
using ModularMonolith.Identity.Domain.Users;
using ModularMonolith.Shared.Application.Events;
using NSubstitute;

namespace ModularMonolith.Identity.Application.UnitTests.Account.SigningIn;

public partial class SignInCommandHandlerTests
{
    private class Fixture
    {
        private readonly FakeUserManager _userManager = Substitute.For<FakeUserManager>();
        private readonly IEventBus _eventBus = Substitute.For<IEventBus>();
        private readonly TimeProvider _timeProvider = TimeProvider.System;
        private readonly IOptions<AuthOptions> _options = new OptionsWrapper<AuthOptions>(new AuthOptions
        {
            Audience = "localhost",
            Issuer = "localhost",
            Key = "12345678123456781234567812345678",
            ExpirationTimeInMinutes = 15
        });

        private User? _user;
        private string? _password;

        public SignInCommandHandler CreateSut() => new(_userManager, _eventBus, _options, _timeProvider);

        public void SetupUser(string email, string password)
        {
            _user = new User(email);
            _password = password;

            _userManager.FindByEmailAsync(email)
                .Returns(_user);

            _userManager.CheckPasswordAsync(Arg.Any<User>(), Arg.Any<string>())
                .Returns(false);
            
            _userManager.CheckPasswordAsync(_user, _password)
                .Returns(true);
        }

        public Task AssertThatSignInSucceededEventWasPublished() => _eventBus.Received(1)
            .PublishAsync(Arg.Is<SignInSucceededEvent>(u => u.UserId == _user!.Id), Arg.Any<CancellationToken>());

        public Task AssertThatSignInFailedEventWasPublished() => _eventBus.Received(1)
            .PublishAsync(Arg.Is<SignInFailedEvent>(u => u.UserId == _user!.Id), Arg.Any<CancellationToken>());
        
        public Task AssertThatNoEventWasPublished() => _eventBus.DidNotReceiveWithAnyArgs().PublishAsync(default!, default);

    }
}
