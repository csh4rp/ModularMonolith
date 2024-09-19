using System.Net;
using FluentAssertions;
using ModularMonolith.Identity.Api.IntegrationTests.Account.Shared;
using ModularMonolith.Identity.Api.IntegrationTests.Shared;
using ModularMonolith.Identity.Domain.Users;
using ModularMonolith.Tests.Utils.Abstractions;
using ModularMonolith.Tests.Utils.Kafka;

namespace ModularMonolith.Identity.Api.IntegrationTests.Account.PasswordChange;

[Collection("Account")]
public class ChangePasswordTests : BaseIntegrationTest<ChangePasswordTests>
{
    private static readonly Guid UserId = Guid.Parse("FDCD0B4A-828E-40ED-9DDB-AB987B2E70F7");

    private readonly IdentityFixture _identityFixture;
    private readonly AccountFixture _accountFixture;
    private readonly KafkaMessagingFixture<PasswordChangedEvent> _passwordChangedMessagingFixture;

    public ChangePasswordTests(IdentityFixture identityFixture, AccountFixture accountFixture)
    {
        _identityFixture = identityFixture;
        _accountFixture = accountFixture;
        _passwordChangedMessagingFixture = new KafkaMessagingFixture<PasswordChangedEvent>(_identityFixture.GetMessagingConnectionString());
    }

    public override Task InitializeAsync() => _passwordChangedMessagingFixture.StartAsync();

    [Fact]
    public async Task ShouldReturnNoContent_WhenPasswordIsValid()
    {
        // Arrange
        var user = _accountFixture.AActiveUser();
        user.Id = UserId;
        await _identityFixture.AddUsersAsync(user);

        using var client = _identityFixture.CreateClientWithAuthToken(UserId);
        using var request = GetResource("ChangePassword.Valid.json");

        // Act
        using var response = await client.PostAsync("/api/identity/account/change-password", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var passwordChangedEvent = await _passwordChangedMessagingFixture.VerifyMessageReceivedAsync();
        await VerifyMessage(passwordChangedEvent);
    }

    [Fact]
    [TestFileName("BadRequest_CurrentPasswordIsInvalid")]
    public async Task ShouldReturnNoContent_WhenCurrentPasswordIsInvalid()
    {
        // Arrange
        var user = _accountFixture.AActiveUser();
        user.Id = UserId;

        await _identityFixture.AddUsersAsync(user);

        using var client = _identityFixture.CreateClientWithAuthToken(UserId);
        using var request = GetResource("ChangePassword.InvalidCurrentPassword.json");

        // Act
        using var response = await client.PostAsync("/api/identity/account/change-password", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        await VerifyResponse(response);
    }

    [Fact]
    [TestFileName("BadRequest_NewPasswordDoesNotMatchPolicy")]
    public async Task ShouldReturnNoContent_WhenNewPasswordDoesNotMatchPolicy()
    {
        // Arrange
        var user = _accountFixture.AActiveUser();
        user.Id = UserId;
        await _identityFixture.AddUsersAsync(user);

        using var client = _identityFixture.CreateClientWithAuthToken(UserId);
        using var request = GetResource("ChangePassword.NewPasswordNotMatchingPolicy.json");

        // Act
        using var response = await client.PostAsync("/api/identity/account/change-password", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        await VerifyResponse(response);
    }

    [Fact]
    public async Task ShouldReturnUnauthorized_WhenRequestIsMissingToken()
    {
        // Arrange
        using var client = _identityFixture.CreateClient();
        using var request = GetResource("ChangePassword.Valid.json");

        // Act
        using var response = await client.PostAsync("/api/identity/account/change-password", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    public override async Task DisposeAsync()
    {
        await _identityFixture.ResetAsync();
        await _passwordChangedMessagingFixture.DisposeAsync();
    }
}
