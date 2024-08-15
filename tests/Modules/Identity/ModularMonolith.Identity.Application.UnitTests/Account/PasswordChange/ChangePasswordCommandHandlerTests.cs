using FluentAssertions;
using ModularMonolith.Identity.Contracts.Account.PasswordChange;
using ModularMonolith.Shared.Application.Exceptions;
using ModularMonolith.Shared.Contracts.Errors;

namespace ModularMonolith.Identity.Application.UnitTests.Account.PasswordChange;

public class ChangePasswordCommandHandlerTests
{
    private readonly ChangePasswordCommandHandlerTestsFixture _fixture = new();

    [Fact]
    public async Task ShouldChangePassword_WhenCurrentPasswordIsValid()
    {
        // Arrange
        const string currentPassword = "Pa$$word";
        const string newPassword = "Pa$$word123";
        var userId = Guid.Parse("4b923b33-187d-465e-8ccc-0b39ad4d713b");

        _fixture.SetupUser(userId, currentPassword);

        var command = new ChangePasswordCommand(currentPassword, newPassword, newPassword);

        var handler = _fixture.CreateSut();

        // Act
        await handler.Handle(command, default);

        // Assert
        await _fixture.AssertThatPasswordChangedEventWasPublished();
    }

    [Fact]
    public async Task ShouldNotChangePassword_WhenCurrentPasswordIsInvalid()
    {
        // Arrange
        const string currentPassword = "Pa$$word";
        const string newPassword = "Pa$$word123";
        var userId = Guid.Parse("4b923b33-187d-465e-8ccc-0b39ad4d713b");

        _fixture.SetupUser(userId, currentPassword);

        var command = new ChangePasswordCommand("invalid", newPassword, newPassword);

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
                && e.Reference.Equals(nameof(command.CurrentPassword), StringComparison.OrdinalIgnoreCase));

        await _fixture.AssertThatNoEventWasPublished();
    }
}
