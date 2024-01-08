using System.Net;
using FluentAssertions;
using ModularMonolith.Identity.Api.IntegrationTests.Account.Fixtures;
using ModularMonolith.Identity.Api.IntegrationTests.Fixtures;
using ModularMonolith.Shared.TestUtils.Abstractions;

namespace ModularMonolith.Identity.Api.IntegrationTests.Account;

[Collection("Account")]
public class InitializePasswordResetTests : BaseIntegrationTest<InitializePasswordResetTests>
{
    private readonly IdentityFixture _identityFixture;
    private readonly AccountFixture _accountFixture;
    private readonly HttpClient _client;

    public InitializePasswordResetTests(IdentityFixture identityFixture, AccountFixture accountFixture)
    {
        _identityFixture = identityFixture;
        _accountFixture = accountFixture;
        _client = _identityFixture.CreateClient();
    }

    [Fact]
    public async Task ShouldReturnNoContent_WhenRequestIsValid()
    {
        // Arrange
        var user = _accountFixture.AActiveUser();
        _identityFixture.IdentityDbContext.Users.Add(user);
        await _identityFixture.IdentityDbContext.SaveChangesAsync();

        using var request = GetResource("InitializePasswordReset.Valid.json");

        // Act
        using var response = await _client.PostAsync("/api/identity/account/initialize-password-reset", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    public override async Task DisposeAsync() => await _identityFixture.ResetAsync();
}
