using FluentAssertions;
using ModularMonolith.Identity.Contracts.Account.SigningIn;

namespace ModularMonolith.Identity.Application.UnitTests.Account.SigningIn;

public class SignInCommandHandlerTests
{
    private readonly SignInCommandHandlerTestsFixture _fixture = new();

    [Fact]
    public async Task ShouldSignIn_WhenUserNameAndPasswordIsCorrect()
    {
        // Arrange
        const string validEmail = "mail@mail.com";
        const string validPassword = "Pa$$word";

        _fixture.SetupUser(validEmail, validPassword);

        var command = new SignInCommand(validEmail, validPassword);

        var handler = _fixture.CreateSut();

        // Act
        var response = await handler.Handle(command, default);

        // Assert
        response.Should().NotBeNull();
        response.Token.Should().NotBeNullOrEmpty();

        await _fixture.AssertThatSignInSucceededEventWasPublished();
    }

    [Fact]
    public async Task ShouldNotSignIn_WhenUserNameIsInvalid()
    {
        // Arrange
        const string validEmail = "mail@mail.com";
        const string validPassword = "Pa$$word";

        _fixture.SetupUser(validEmail, validPassword);

        var command = new SignInCommand("invalid@mail.com", validPassword);

        var handler = _fixture.CreateSut();

        // Act
        var response = await handler.Handle(command, default);

        // Assert
        response.Should().NotBeNull();
        response.Token.Should().BeNullOrEmpty();

        await _fixture.AssertThatNoEventWasPublished();
    }

    [Fact]
    public async Task ShouldNotSignIn_WhenPasswordIsInvalid()
    {
        // Arrange
        const string validEmail = "mail@mail.com";
        const string validPassword = "Pa$$word";

        _fixture.SetupUser(validEmail, validPassword);

        var command = new SignInCommand(validEmail, "invalid");

        var handler = _fixture.CreateSut();

        // Act
        var response = await handler.Handle(command, default);

        // Assert
        response.Should().NotBeNull();
        response.Token.Should().BeNullOrEmpty();

        await _fixture.AssertThatSignInFailedEventWasPublished();
    }
}
