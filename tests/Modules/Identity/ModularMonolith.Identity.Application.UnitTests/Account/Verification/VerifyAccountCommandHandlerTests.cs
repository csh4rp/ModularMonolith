using FluentAssertions;
using ModularMonolith.Identity.Contracts.Account.Verification;
using ModularMonolith.Shared.Application.Exceptions;
using ModularMonolith.Shared.Contracts.Errors;

namespace ModularMonolith.Identity.Application.UnitTests.Account.Verification;

public class VerifyAccountCommandHandlerTests
{
    private readonly VerifyAccountCommandHandlerTestsFixture _fixture = new();

    [Fact]
    public async Task ShouldVerifyAccount_WhenIdMatchesUser()
    {
        // Arrange
        var userId = Guid.Parse("4B992E53-CA70-4910-BB44-AEE860F084FD");
        const string token = "123";

        _fixture.SetupUser(userId);
        _fixture.SetupEmailConfirmation(token);

        var command = new VerifyAccountCommand(userId, token);

        var handler = _fixture.CreateSut();

        // Act
        await handler.Handle(command, default);

        // Assert
        await _fixture.AssertThatAccountVerifiedEventWasPublished();
    }

    [Fact]
    public async Task ShouldThrowValidationException_WhenUserWithIdDoesNotExist()
    {
        // Arrange
        var userId = Guid.Parse("4B992E53-CA70-4910-BB44-AEE860F084FD");
        const string token = "123";

        _fixture.SetupUser(userId);
        _fixture.SetupEmailConfirmation(token);

        var command = new VerifyAccountCommand(Guid.NewGuid(), token);

        var handler = _fixture.CreateSut();

        // Act
        var exception = await Assert.ThrowsAsync<ValidationException>(() => handler.Handle(command, default));

        // Assert
        exception.Should().NotBeNull();
        exception.Errors.Should()
            .HaveCount(1)
            .And
            .ContainSingle(e =>
                e.Code == ErrorCodes.InvalidValue
                && e.Reference.Equals(nameof(command.UserId), StringComparison.OrdinalIgnoreCase));

        await _fixture.AssertThatNoEventWasPublished();
    }

    [Fact]
    public async Task ShouldThrowValidationException_WhenTokenIsInvalid()
    {
        // Arrange
        var userId = Guid.Parse("4B992E53-CA70-4910-BB44-AEE860F084FD");
        const string token = "123";

        _fixture.SetupUser(userId);
        _fixture.SetupEmailConfirmation(token);

        var command = new VerifyAccountCommand(userId, "1");

        var handler = _fixture.CreateSut();

        // Act
        var exception = await Assert.ThrowsAsync<ValidationException>(() => handler.Handle(command, default));

        // Assert
        exception.Should().NotBeNull();
        exception.Errors.Should()
            .HaveCount(1)
            .And
            .ContainSingle(e =>
                e.Code == ErrorCodes.InvalidValue
                && e.Reference.Equals(nameof(command.VerificationToken), StringComparison.OrdinalIgnoreCase));

        await _fixture.AssertThatNoEventWasPublished();
    }
}
