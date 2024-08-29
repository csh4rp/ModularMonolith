using System.Net;
using FluentAssertions;
using ModularMonolith.Identity.Api.IntegrationTests.Account.Shared;
using ModularMonolith.Identity.Api.IntegrationTests.Shared;
using ModularMonolith.Identity.Domain.Users;
using ModularMonolith.Tests.Utils.Abstractions;
using ModularMonolith.Tests.Utils.Kafka;

namespace ModularMonolith.Identity.Api.IntegrationTests.Account.PasswordReset;

[Collection("Account")]
public class InitializePasswordResetTests : BaseIntegrationTest<InitializePasswordResetTests>
{
    private readonly IdentityFixture _identityFixture;
    private readonly AccountFixture _accountFixture;
    private readonly KafkaMessagingFixture<PasswordResetInitializedEvent> _passwordResetInitializedMessagingFixture;

    public InitializePasswordResetTests(IdentityFixture identityFixture, AccountFixture accountFixture)
    {
        _identityFixture = identityFixture;
        _accountFixture = accountFixture;
        _passwordResetInitializedMessagingFixture =
            new KafkaMessagingFixture<PasswordResetInitializedEvent>(_identityFixture.GetMessagingConnectionString());
    }

    public override Task InitializeAsync() => _passwordResetInitializedMessagingFixture.StartAsync();

    [Fact]
    public async Task ShouldReturnNoContent_WhenRequestIsValid()
    {
        // Arrange
        var user = _accountFixture.AActiveUser();

        await _identityFixture.AddUsersAsync(user);

        using var request = GetResource("InitializePasswordReset.Valid.json");

        // Act
        using var client = _identityFixture.CreateClient();
        using var response = await client.PostAsync("/api/identity/account/initialize-password-reset", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var passwordChangeInitialized = await _passwordResetInitializedMessagingFixture.VerifyMessageReceivedAsync();
        await VerifyMessage(passwordChangeInitialized);
    }

    public override async Task DisposeAsync() => await _identityFixture.ResetAsync();
}
