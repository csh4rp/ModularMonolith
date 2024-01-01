using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using ModularMonolith.Identity.Application.Account.CommandHandlers;
using ModularMonolith.Identity.Application.UnitTests.Account.Fakes;
using ModularMonolith.Identity.Contracts.Account.Commands;
using ModularMonolith.Identity.Domain.Common.Entities;
using ModularMonolith.Identity.Domain.Common.Events;
using ModularMonolith.Shared.Application.Events;
using ModularMonolith.Shared.Contracts.Errors;
using ModularMonolith.Shared.TestUtils.Assertions;
using NSubstitute;
using Xunit;

namespace ModularMonolith.Identity.Application.UnitTests.Account.CommandHandlers;

public class ResetPasswordCommandHandlerTests
{
    private readonly FakeUserManager _userManager = Substitute.For<FakeUserManager>();
    private readonly IEventBus _eventBus = Substitute.For<IEventBus>();
    private readonly ILogger<ResetPasswordCommandHandler> _logger =
        Substitute.For<ILogger<ResetPasswordCommandHandler>>();
    
    [Fact]
    public async Task ShouldResetPassword_WhenPasswordResetTokenIsValid()
    {
        // Arrange
        var userId = Guid.Parse("EAC1106F-A02D-4243-9BEC-A7CEE36D45C1");
        const string token = "123";
        const string password = "Pa$$word123!@#";

        var user = new User("mail@mail.com") { Id = userId };

        _userManager.FindByIdAsync(userId.ToString()).Returns(user);
        _userManager.ResetPasswordAsync(user, token, password).Returns(IdentityResult.Success);

        var command = new ResetPasswordCommand(userId, token, password, password);
        
        var handler = new ResetPasswordCommandHandler(_userManager, _eventBus, _logger);
        
        // Act
        var result = await handler.Handle(command, default);
        
        // Assert
        result.Should().BeSuccessful();

        await _eventBus.Received(1)
            .PublishAsync(Arg.Is<PasswordReset>(e => e.UserId == userId), default);
    }
    [Fact]
    public async Task ShouldReturnTokenError_WhenPasswordResetTokenIsInvalid()
    {
        // Arrange
        var userId = Guid.Parse("EAC1106F-A02D-4243-9BEC-A7CEE36D45C1");
        const string token = "123";
        const string password = "Pa$$word123!@#";

        var user = new User("mail@mail.com") { Id = userId };

        _userManager.FindByIdAsync(userId.ToString()).Returns(user);
        _userManager.ResetPasswordAsync(user, token, password)
            .Returns(IdentityResult.Failed(new IdentityErrorDescriber().InvalidToken()));

        var command = new ResetPasswordCommand(userId, token, password, password);
        
        var handler = new ResetPasswordCommandHandler(_userManager, _eventBus, _logger);
        
        // Act
        var result = await handler.Handle(command, default);
        
        // Assert
        result.Should().NotBeSuccessful();

        result.Error.Should().BeMemberError()
            .And.HaveCode(ErrorCodes.InvalidValue)
            .And.HaveTarget(nameof(command.ResetPasswordToken));
        
        await _eventBus.DidNotReceiveWithAnyArgs()
            .PublishAsync<PasswordReset>(default!, default);
    }
    
}
