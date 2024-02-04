using Microsoft.AspNetCore.Identity;
using ModularMonolith.Identity.Application.Account.CommandHandlers;
using ModularMonolith.Identity.Application.UnitTests.Account.Fakes;
using ModularMonolith.Identity.Domain.Common.Entities;
using ModularMonolith.Identity.Domain.Common.Events;
using ModularMonolith.Shared.Application.Events;
using NSubstitute;

namespace ModularMonolith.Identity.Application.UnitTests.Account.CommandHandlers;

public partial class RegisterCommandHandlerTests
{
    private class Fixture
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
            .PublishAsync(Arg.Any<UserRegistered>(), Arg.Any<CancellationToken>());
        
        public Task AssertThatNoEventWasPublished() => _eventBus.DidNotReceiveWithAnyArgs().PublishAsync(default!, default);
        
    }
}
