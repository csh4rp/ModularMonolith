using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using ModularMonolith.Identity.Application.Account.CommandHandlers;
using ModularMonolith.Identity.Application.UnitTests.Account.Fakes;
using ModularMonolith.Identity.Contracts.Account.Commands;
using ModularMonolith.Identity.Domain.Common.Entities;
using ModularMonolith.Shared.Application.Events;
using ModularMonolith.Shared.Contracts.Errors;
using ModularMonolith.Shared.TestUtils.Assertions;
using NSubstitute;

namespace ModularMonolith.Identity.Application.UnitTests.Account.CommandHandlers;

public class VerifyAccountCommandHandlerTests
{
    private readonly FakeUserManager _userManager = Substitute.For<FakeUserManager>();
    private readonly IEventBus _eventBus = Substitute.For<IEventBus>();

    private readonly ILogger<VerifyAccountCommandHandler> _logger =
        Substitute.For<ILogger<VerifyAccountCommandHandler>>();

    [Fact]
    public async Task ShouldVerifyAccount_WhenIdMatchesUser()
    {
        // Arrange
        var userId = Guid.Parse("4B992E53-CA70-4910-BB44-AEE860F084FD");
        const string token = "123";

        var user = new User("mail@mail.com") { Id = userId };

        _userManager.FindByIdAsync(userId.ToString()).Returns(user);
        _userManager.ConfirmEmailAsync(user, token).Returns(IdentityResult.Success);

        var command = new VerifyAccountCommand(userId, token);

        var handler = new VerifyAccountCommandHandler(_userManager, _eventBus, _logger);

        // Act
        var result = await handler.Handle(command, default);

        // Assert
        result.Should().BeSuccessful();
    }

    [Fact]
    public async Task ShouldReturnIInvalidValueError_WhenUserWithIdDoesNotExist()
    {
        // Arrange
        var userId = Guid.Parse("4B992E53-CA70-4910-BB44-AEE860F084FD");
        const string token = "123";

        var command = new VerifyAccountCommand(userId, token);

        var handler = new VerifyAccountCommandHandler(_userManager, _eventBus, _logger);

        // Act
        var result = await handler.Handle(command, default);

        // Assert
        result.Should().NotBeSuccessful();

        result.Error.Should().BeMemberError()
            .And.HaveCode(ErrorCodes.InvalidValue)
            .And.HaveTarget(nameof(command.UserId));
    }

    [Fact]
    public async Task ShouldReturnIInvalidValueError_WhenTokenIsInvalid()
    {
        // Arrange
        var userId = Guid.Parse("4B992E53-CA70-4910-BB44-AEE860F084FD");
        const string token = "123";

        var user = new User("mail@mail.com") { Id = userId };

        _userManager.FindByIdAsync(userId.ToString()).Returns(user);
        _userManager.ConfirmEmailAsync(user, token)
            .Returns(IdentityResult.Failed(new IdentityErrorDescriber().InvalidToken()));

        var command = new VerifyAccountCommand(userId, token);

        var handler = new VerifyAccountCommandHandler(_userManager, _eventBus, _logger);

        // Act
        var result = await handler.Handle(command, default);

        // Assert
        result.Should().NotBeSuccessful();

        result.Error.Should().BeMemberError()
            .And.HaveCode(ErrorCodes.InvalidValue)
            .And.HaveTarget(nameof(command.VerificationToken));
    }
}
