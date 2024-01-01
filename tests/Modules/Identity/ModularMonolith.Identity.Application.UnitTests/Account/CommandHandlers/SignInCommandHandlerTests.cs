using FluentAssertions;
using Microsoft.Extensions.Options;
using ModularMonolith.Identity.Application.Account.CommandHandlers;
using ModularMonolith.Identity.Application.UnitTests.Account.Fakes;
using ModularMonolith.Identity.Contracts.Account.Commands;
using ModularMonolith.Identity.Core.Options;
using ModularMonolith.Identity.Domain.Common.Entities;
using ModularMonolith.Identity.Domain.Common.Events;
using ModularMonolith.Shared.Application.Events;
using ModularMonolith.Shared.Contracts.Errors;
using ModularMonolith.Shared.TestUtils.Assertions;
using NSubstitute;
using Xunit;


namespace ModularMonolith.Identity.Application.UnitTests.Account.CommandHandlers;

public class SignInCommandHandlerTests
{
    private readonly FakeUserManager _userManager = Substitute.For<FakeUserManager>();
    private readonly IEventBus _eventBus = Substitute.For<IEventBus>();
    private readonly AuthOptions _options = new()
    {
        Audience = "localhost", 
        Issuer = "localhost",
        Key = "12345678123456781234567812345678",
        ExpirationTimeInMinutes = 15
    };
    
    [Fact]
    public async Task ShouldSignIn_WhenUserNameAndPasswordIsCorrect()
    {
        // Arrange
        const string validEmail = "mail@mail.com";
        const string validPassword = "Pa$$word";
        var user = new User(validEmail) { Id = Guid.NewGuid() };
        
        _userManager.FindByEmailAsync(validEmail)
            .Returns(user);
        
        _userManager.CheckPasswordAsync(user, validPassword)
            .Returns(true);
        
        var command = new SignInCommand(validEmail, validPassword);

        var handler = new SignInCommandHandler(_userManager, _eventBus,
            new OptionsWrapper<AuthOptions>(_options), TimeProvider.System);

        // Act
        var result = await handler.Handle(command, default);

        // Assert
        result.Should().BeSuccessful();
        result.Value.Should().NotBeNull();
        result.Value!.Token.Should().NotBeNullOrEmpty();

        await _eventBus.Received(1)
            .PublishAsync(Arg.Is<SignInSucceeded>(s => s.UserId == user.Id), default);
    }
    
    [Fact]
    public async Task ShouldNotSignIn_WhenUserNameIsInvalid()
    {
        // Arrange
        const string validEmail = "mail@mail.com";
        const string validPassword = "Pa$$word";
        var user = new User(validEmail) { Id = Guid.NewGuid()};

        _userManager.FindByEmailAsync(validEmail)
            .Returns(user);
        
        _userManager.CheckPasswordAsync(user, validPassword)
            .Returns(true);
        
        var command = new SignInCommand("invalid@mail.com", "invalid");

        var handler = new SignInCommandHandler(_userManager, _eventBus,
            new OptionsWrapper<AuthOptions>(_options), TimeProvider.System);

        // Act
        var result = await handler.Handle(command, default);

        // Assert
        result.Should().NotBeSuccessful();
        
        result.Error.Should().BeMemberError()
            .And.HaveCode(ErrorCodes.InvalidValue)
            .And.HaveTarget(nameof(command.Password));
        
        await _eventBus.DidNotReceiveWithAnyArgs()
            .PublishAsync<SignInSucceeded>(default!, default);
        
        await _eventBus.DidNotReceiveWithAnyArgs()
            .PublishAsync<SignInFailed>(default!, default);
    }
    
    [Fact]
    public async Task ShouldReturnInvalidPasswordError_WhenPasswordIsInvalid()
    {
        // Arrange
        const string validEmail = "mail@mail.com";
        const string validPassword = "Pa$$word";
        var user = new User(validEmail) { Id = Guid.NewGuid()};

        _userManager.FindByEmailAsync(validEmail)
            .Returns(user);
        
        _userManager.CheckPasswordAsync(user, validPassword)
            .Returns(true);
        
        var command = new SignInCommand(validEmail, "invalid");

        var handler = new SignInCommandHandler(_userManager, _eventBus,
            new OptionsWrapper<AuthOptions>(_options), TimeProvider.System);

        // Act
        var result = await handler.Handle(command, default);

        // Assert
        result.Should().NotBeSuccessful();
        result.Error.Should().BeMemberError()
            .And.HaveCode(ErrorCodes.InvalidValue)
            .And.HaveTarget(nameof(command.Password));
        
        await _eventBus.Received(1)
            .PublishAsync(Arg.Is<SignInFailed>(s => s.UserId == user.Id), default);
    }
}
