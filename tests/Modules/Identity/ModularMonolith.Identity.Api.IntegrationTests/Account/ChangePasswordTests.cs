using System.Net;
using FluentAssertions;
using ModularMonolith.Identity.Api.IntegrationTests.Account.Fixtures;
using ModularMonolith.Identity.Api.IntegrationTests.Fixtures;
using ModularMonolith.Shared.IntegrationTests.Common;

namespace ModularMonolith.Identity.Api.IntegrationTests.Account;

[Collection("Account")]
public class ChangePasswordTests : BaseIntegrationTest<ChangePasswordTests>
{
    private readonly PostgresFixture _postgresFixture;
    private readonly IdentityFixture _identityFixture;
    private readonly AccountFixture _accountFixture;
    private readonly HttpClient _client;
    
    public ChangePasswordTests(PostgresFixture postgresFixture,
        IdentityFixture identityFixture, AccountFixture accountFixture)
    {
        _postgresFixture = postgresFixture;
        _identityFixture = identityFixture;
        _accountFixture = accountFixture;
        _client = identityFixture.CreateClient(_postgresFixture.ConnectionString);
    }    
    
    [Fact]
    public async Task ShouldReturnNoContent_WhenPasswordIsValid()
    {
        // Arrange
        var user = _accountFixture.AActiveUser();
        _postgresFixture.IdentityDbContext.Users.Add(user);
        await _postgresFixture.IdentityDbContext.SaveChangesAsync(); ;
        
        using var request = GetResource("ChangePassword.Valid.json");

        // Act
        using var response = await _client.PostAsync("/api/identity/account/change-password", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
