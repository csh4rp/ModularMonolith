using Microsoft.AspNetCore.Identity;
using ModularMonolith.Identity.Application.Account.CommandHandlers;
using ModularMonolith.Identity.Application.UnitTests.Account.Fakes;
using ModularMonolith.Identity.Contracts.Account.Commands;
using ModularMonolith.Identity.Domain.Common.Entities;
using ModularMonolith.Identity.Domain.Common.Events;
using ModularMonolith.Shared.Application.Events;
using ModularMonolith.Shared.Application.Identity;
using NSubstitute;
using Xunit;

namespace ModularMonolith.Identity.Application.UnitTests.Account.CommandHandlers;

public class ChangePasswordCommandHandlerTests
{
    private const string UserId = "4F9391D5-E8EE-4DAB-A839-8BADA2C9A45B";
    
    private readonly FakeUserManager _userManager = Substitute.For<FakeUserManager>();
    private readonly IIdentityContextAccessor _identityContextAccessor = Substitute.For<IIdentityContextAccessor>();
    private readonly IEventBus _eventBus = Substitute.For<IEventBus>();

    public ChangePasswordCommandHandlerTests()
    {
        _identityContextAccessor.Context.Returns(
            new IdentityContext(Guid.Parse(UserId), "User"));
    }
    
    [Fact]
    public async Task ShouldChangePassword_WhenCurrentPasswordIsValid()
    {
        // Arrange
        const string currentPassword = "Pa$$word";
        const string newPassword = "Pa$$word123";
        var user = new User { Id = Guid.Parse(UserId) };

        _userManager.FindByIdAsync(user.Id.ToString()).Returns(user);
        _userManager.ChangePasswordAsync(user, currentPassword, newPassword).Returns(IdentityResult.Success);
        
        var command = new ChangePasswordCommand(currentPassword, newPassword, newPassword);
        
        var handler = new ChangePasswordCommandHandler(_userManager, _identityContextAccessor, _eventBus);
        
        // Act
        await handler.Handle(command, default);
        
        // Assert
        await _eventBus.Received(1).PublishAsync(Arg.Is<PasswordChanged>(e => e.UserId == user.Id), default);
    }
    
    [Fact]
    public async Task ShouldChangePassword_WhenCurrentPasswordIsInvalid()
    {
        // Arrange
        const string currentPassword = "Pa$$word";
        const string newPassword = "Pa$$word123";
        var user = new User { Id = Guid.Parse(UserId) };

        _userManager.FindByIdAsync(user.Id.ToString()).Returns(user);
        // _userManager.ChangePasswordAsync(user, Arg.Is<string>(s => s != currentPassword), newPassword)
        //     .Returns(IdentityResult.Failed(new IdentityError{Code = ""}));
        _userManager.ChangePasswordAsync(Arg.Any<User>(), Arg.Any<string>(), Arg.Any<string>())
            .Returns(IdentityResult.Failed(new IdentityError{Code = ""}));
        
        var command = new ChangePasswordCommand(currentPassword, newPassword, newPassword);
        
        var handler = new ChangePasswordCommandHandler(_userManager, _identityContextAccessor, _eventBus);
        
        // Act
        await handler.Handle(command, default);
        
        // Assert
        await _eventBus.DidNotReceiveWithAnyArgs().PublishAsync(Arg.Any<PasswordChanged>(), default);
    }

}
