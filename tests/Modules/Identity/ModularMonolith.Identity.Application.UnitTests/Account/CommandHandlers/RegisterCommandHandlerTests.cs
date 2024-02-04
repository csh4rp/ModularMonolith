using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using ModularMonolith.Identity.Application.Account;
using ModularMonolith.Identity.Contracts.Account.Commands;
using ModularMonolith.Shared.Application.Exceptions;

namespace ModularMonolith.Identity.Application.UnitTests.Account.CommandHandlers;

public partial class RegisterCommandHandlerTests
{
    private readonly Fixture _fixture = new();

    [Fact]
    public async Task ShouldRegisterUser_WhenEmailIsNotUsed()
    {
        // Arrange
        _fixture.SetupUserCreationToSucceed();

        var command = new RegisterCommand("mail@mail.com", "Pa$$word123", "Pa$$word123");

        var handler = _fixture.CreateSut();

        // Act
        await handler.Handle(command, default);

        // Assert
        await _fixture.AssertThatUserRegisteredEventWasPublished();
    }

    [Fact]
    public async Task ShouldThrowValidationException_WhenEmailIsAlreadyUsed()
    {
        // Arrange
        const string email = "mail@mail.com";

        _fixture.SetupUser(email);

        var command = new RegisterCommand(email, "Pa$$word123", "Pa$$word123");

        var handler = _fixture.CreateSut();

        // Act
        var exception = await Assert.ThrowsAsync<ValidationException>(() => handler.Handle(command, default));

        // Assert
        exception.Should().NotBeNull();
        exception.Errors.Should()
            .HaveCount(1)
            .And
            .ContainSingle(e =>
                e.Code == AccountErrorCodes.EmailConflict
                && e.Reference.Equals(nameof(command.Email), StringComparison.OrdinalIgnoreCase));

        await _fixture.AssertThatNoEventWasPublished();
    }

    [Fact]
    public async Task ShouldThrowValidationException_WhenPasswordDoesNotMatchPolicy()
    {
        // Arrange
        var error = new IdentityErrorDescriber().PasswordTooShort(20);

        // Arrange
        _fixture.SetupUserCreationToFailWithError(error);

        var command = new RegisterCommand("mail@mail.com", "Pa$$word123", "Pa$$word123");

        var handler = _fixture.CreateSut();

        // Act
        var exception = await Assert.ThrowsAsync<ValidationException>(() => handler.Handle(command, default));

        // Assert
        exception.Should().NotBeNull();
        exception.Errors.Should()
            .HaveCount(1)
            .And
            .ContainSingle(e =>
                e.Code == AccountErrorCodes.PasswordNotMatchingPolicy
                && e.Reference.Equals(nameof(command.Password), StringComparison.OrdinalIgnoreCase));

        await _fixture.AssertThatNoEventWasPublished();
    }
}
