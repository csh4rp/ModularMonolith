using System.Net;
using FluentAssertions;
using ModularMonolith.Identity.Api.IntegrationTests.Fixtures;
using ModularMonolith.Shared.IntegrationTests.Common;

namespace ModularMonolith.Identity.Api.IntegrationTests.Account;

[Collection("Account")]
public class RegisterTests : BaseIntegrationTest<RegisterTests>
{
    private readonly IdentityFixture _identityFixture;
    private readonly HttpClient _client;
    
    public RegisterTests(IdentityFixture identityFixture)
    {
        _identityFixture = identityFixture;
        _client = identityFixture.CreateClient();
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
        await _identityFixture.ResetAsync();
    }
}
