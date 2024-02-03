using Microsoft.Extensions.Logging;
using MockQueryable.NSubstitute;
using ModularMonolith.Identity.Application.Account.CommandHandlers;
using ModularMonolith.Identity.Application.UnitTests.Account.Fakes;
using ModularMonolith.Identity.Contracts.Account.Commands;
using ModularMonolith.Identity.Domain.Common.Entities;
using ModularMonolith.Identity.Domain.Common.Events;
using ModularMonolith.Shared.Application.Events;
using ModularMonolith.Shared.Application.Exceptions;
using NSubstitute;

namespace ModularMonolith.Identity.Application.UnitTests.Account.CommandHandlers;

public class InitializePasswordResetCommandHandlerTests
{
    private readonly FakeUserManager _userManager = Substitute.For<FakeUserManager>();
    private readonly IEventBus _eventBus = Substitute.For<IEventBus>();

    private readonly ILogger<InitializePasswordResetCommandHandler> _logger =
        Substitute.For<ILogger<InitializePasswordResetCommandHandler>>();


    [Fact]
    public async Task ShouldInitializePasswordReset_WhenEmailMatchesUser()
    {
        // Arrange
        const string validEmail = "mail@mail.com";
        var user = new User(validEmail) { Id = Guid.NewGuid() };
        var users = new[] { user }.BuildMock();

        _userManager.NormalizeEmail(Arg.Any<string>()).Returns(c => c.Args().First().ToString()?.ToUpper());
        _userManager.Users.Returns(users);

        var command = new InitializePasswordResetCommand(validEmail);

        var handler = new InitializePasswordResetCommandHandler(_userManager, _eventBus, _logger);

        // Act
        await handler.Handle(command, default);

        // Assert
        await _eventBus.Received(1)
            .PublishAsync(Arg.Is<PasswordResetInitialized>(e => e.UserId == user.Id), default);
    }

    [Fact]
    public async Task ShouldNotInitializePasswordReset_WhenEmailDoesNotMatchUser()
    {
        // Arrange
        const string validEmail = "mail@mail.com";
        var user = new User(validEmail) { Id = Guid.NewGuid() };
        var users = new[] { user }.BuildMock();

        _userManager.NormalizeEmail(Arg.Any<string>()).Returns(c => c.Args().First().ToString()?.ToUpper());
        _userManager.Users.Returns(users);

        var command = new InitializePasswordResetCommand("invalid@mail.com");

        var handler = new InitializePasswordResetCommandHandler(_userManager, _eventBus, _logger);

        // Act
        var exception =  await Assert.ThrowsAsync<ValidationException>(() => handler.Handle(command, default));

        // Assert
        await _eventBus.DidNotReceiveWithAnyArgs()
            .PublishAsync<PasswordResetInitialized>(default!, default);
    }
}
