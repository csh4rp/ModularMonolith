using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using ModularMonolith.Identity.Contracts.Account.Commands;
using ModularMonolith.Shared.Application.Exceptions;
using ModularMonolith.Shared.Contracts.Errors;

namespace ModularMonolith.Identity.Application.UnitTests.Account.CommandHandlers;

public partial class ResetPasswordCommandHandlerTests
{
    private readonly Fixture _fixture = new ();
    
    [Fact]
    public async Task ShouldResetPassword_WhenPasswordResetTokenIsValid()
    {
        // Arrange
        var userId = Guid.Parse("EAC1106F-A02D-4243-9BEC-A7CEE36D45C1");
        const string token = "123";
        const string password = "Pa$$word123!@#";
        
        _fixture.SetupUser(userId);
        _fixture.SetupPasswordReset(token, new IdentityErrorDescriber().InvalidToken());
        
        var command = new ResetPasswordCommand(userId, token, password, password);

        var handler = _fixture.CreateSut();

        // Act
        await handler.Handle(command, default);

        // Assert
        await _fixture.AssertThatPasswordResetEventWasPublished();
    }

    [Fact]
    public async Task ShouldThrowValidationException_WhenPasswordResetTokenIsInvalid()
    {
        // Arrange
        var userId = Guid.Parse("EAC1106F-A02D-4243-9BEC-A7CEE36D45C1");
        const string token = "123";
        const string password = "Pa$$word123!@#";

        _fixture.SetupUser(userId);

        _fixture.SetupPasswordReset(token, new IdentityErrorDescriber().InvalidToken());

        var command = new ResetPasswordCommand(userId, "invalid", password, password);

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
                && e.Reference.Equals(nameof(command.ResetPasswordToken), StringComparison.OrdinalIgnoreCase));
        
        await _fixture.AssertThatNoEventWasPublished();
    }
}
