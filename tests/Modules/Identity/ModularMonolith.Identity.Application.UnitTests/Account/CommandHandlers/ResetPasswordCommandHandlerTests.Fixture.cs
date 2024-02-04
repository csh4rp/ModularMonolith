﻿using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using ModularMonolith.Identity.Application.Account.CommandHandlers;
using ModularMonolith.Identity.Application.UnitTests.Account.Fakes;
using ModularMonolith.Identity.Domain.Common.Entities;
using ModularMonolith.Identity.Domain.Common.Events;
using ModularMonolith.Shared.Application.Events;
using NSubstitute;

namespace ModularMonolith.Identity.Application.UnitTests.Account.CommandHandlers;

public partial class ResetPasswordCommandHandlerTests
{
    private class Fixture
    {
        private readonly FakeUserManager _userManager = Substitute.For<FakeUserManager>();
        private readonly IEventBus _eventBus = Substitute.For<IEventBus>();
        private readonly ILogger<ResetPasswordCommandHandler> _logger =
            Substitute.For<ILogger<ResetPasswordCommandHandler>>();

        private User? _user;
        
        public ResetPasswordCommandHandler CreateSut() => new(_userManager, _eventBus, _logger);

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
        
        public Task AssertThatPasswordResetEventWasPublished() => _eventBus.Received(1)
            .PublishAsync(Arg.Is<PasswordReset>(u => u.UserId == _user!.Id), Arg.Any<CancellationToken>());
        
        public Task AssertThatNoEventWasPublished() => _eventBus.DidNotReceiveWithAnyArgs().PublishAsync(default!, default);

    }
}
