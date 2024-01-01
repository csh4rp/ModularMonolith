using System.Net;
using FluentAssertions;
using ModularMonolith.Identity.Api.IntegrationTests.Fixtures;
using ModularMonolith.Shared.IntegrationTests.Common;

namespace ModularMonolith.Identity.Api.IntegrationTests.Account;

[Collection("Account")]
public class RegisterTests : BaseIntegrationTest<RegisterTests>
{
    private readonly PostgresFixture _postgresFixture;
    private readonly HttpClient _client;
    
    public RegisterTests(PostgresFixture postgresFixture,
        IdentityFixture identityFixture)
    {
        _postgresFixture = postgresFixture;
        _client = identityFixture.CreateClient(_postgresFixture.ConnectionString);
    }
    
    [Fact]
    public async Task ShouldReturnNoContent_WhenUserWithEmailDoesNotExist()
    {
        // Arrange
        using var request = GetResource("Register.Valid.json");

        // Act
        using var response = await _client.PostAsync("/api/identity/account/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    public override async Task DisposeAsync()
    {
        _client.Dispose();
        await _postgresFixture.ResetAsync();
    }
}
