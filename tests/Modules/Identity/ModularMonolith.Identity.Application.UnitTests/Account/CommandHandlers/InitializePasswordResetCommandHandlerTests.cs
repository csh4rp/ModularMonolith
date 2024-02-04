using ModularMonolith.Identity.Contracts.Account.Commands;

namespace ModularMonolith.Identity.Application.UnitTests.Account.CommandHandlers;

public partial class InitializePasswordResetCommandHandlerTests
{
    private readonly Fixture _fixture = new();

    [Fact]
    public async Task ShouldInitializePasswordReset_WhenEmailMatchesUser()
    {
        // Arrange
        const string validEmail = "mail@mail.com";

        _fixture.SetupUser(validEmail);
        
        var command = new InitializePasswordResetCommand(validEmail);

        var handler = _fixture.CreateSut();

        // Act
        await handler.Handle(command, default);

        // Assert
        await _fixture.AssertThatResetPasswordInitializedEventWasPublished();
    }

    [Fact]
    public async Task ShouldNotInitializePasswordReset_WhenEmailDoesNotMatchUser()
    {
        // Arrange
        _fixture.SetupUser("mail@mail.com");
        
        var command = new InitializePasswordResetCommand("invalid@mail.com");

        var handler = _fixture.CreateSut();

        // Act
        await handler.Handle(command, default);

        // Assert
        await _fixture.AssertThatNoEventWasPublished();
    }
}
