using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using MockQueryable.NSubstitute;
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

public class RegisterCommandHandlerTests
{
    private readonly FakeUserManager _userManager = Substitute.For<FakeUserManager>();
    private readonly IEventBus _eventBus = Substitute.For<IEventBus>();
    
    [Fact]
    public async Task ShouldRegisterUser_WhenEmailIsNotUsed()
    {
        // Arrange
        _userManager.Users.Returns(Array.Empty<User>().BuildMock());
        _userManager.CreateAsync(Arg.Any<User>(), Arg.Any<string>()).Returns(IdentityResult.Success);
        
        var command = new RegisterCommand("mail@mail.com", "Pa$$word123", "Pa$$word123");

        var handler = new RegisterCommandHandler(_userManager, _eventBus);
        
        // Act
        var result = await handler.Handle(command, default);
        
        // Assert
        result.Should().BeSuccessful();

        await _eventBus.Received(1)
            .PublishAsync(Arg.Is<UserRegistered>(e => e.Email == command.Email), default);
    }
    
    [Fact]
    public async Task ShouldReturnConflict_WhenEmailIsAlreadyUsed()
    {
        // Arrange
        const string email = "mail@mail.com";
        var existingUser = new User { Id = Guid.NewGuid(), NormalizedEmail = email.ToUpper() };
        
        _userManager.Users.Returns(new[]{existingUser}.BuildMock());
        _userManager.NormalizeEmail(Arg.Any<string>()).Returns(c => c.Arg<string>().ToUpper());
        _userManager.CreateAsync(Arg.Any<User>(), Arg.Any<string>()).Returns(IdentityResult.Success);
        
        var command = new RegisterCommand(email, "Pa$$word123", "Pa$$word123");

        var handler = new RegisterCommandHandler(_userManager, _eventBus);
        
        // Act
        var result = await handler.Handle(command, default);
        
        // Assert
        result.Should().NotBeSuccessful();
        result.Error.Should().BeConflictError()
            .And.HaveTarget(nameof(command.Email));
            
        await _eventBus.DidNotReceiveWithAnyArgs().PublishAsync<UserRegistered>(default!, default);
    }
    
    [Fact]
    public async Task ShouldReturnPasswordError_WhenPasswordDoesNotMatchPolicy()
    {
        // Arrange
        _userManager.Users.Returns(Array.Empty<User>().BuildMock());
        _userManager.CreateAsync(Arg.Any<User>(), Arg.Any<string>())
            .Returns(IdentityResult.Failed(new IdentityErrorDescriber().PasswordTooShort(20)));
        
        var command = new RegisterCommand("mail@mail.com", "Pa$$word123", "Pa$$word123");

        var handler = new RegisterCommandHandler(_userManager, _eventBus);
        
        // Act
        var result = await handler.Handle(command, default);
        
        // Assert
        result.Should().NotBeSuccessful();
        result.Error.Should().BeMemberError()
            .And.HaveCode(ErrorCodes.InvalidValue)
            .And.HaveTarget(nameof(command.Password));
        
        await _eventBus.DidNotReceiveWithAnyArgs().PublishAsync<UserRegistered>(default!, default);
    }
}
