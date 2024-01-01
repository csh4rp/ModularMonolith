using System.Net;
using FluentAssertions;
using ModularMonolith.Identity.Api.IntegrationTests.Account.Fixtures;
using ModularMonolith.Identity.Api.IntegrationTests.Fixtures;
using ModularMonolith.Shared.IntegrationTests.Common;

namespace ModularMonolith.Identity.Api.IntegrationTests.Account;

[Collection("Account")]
public class SignInTests : BaseIntegrationTest<SignInTests>
{
    private readonly PostgresFixture _postgresFixture;
    private readonly AccountFixture _accountFixture;
    private readonly HttpClient _client;

    public SignInTests(PostgresFixture postgresFixture,
        AccountFixture accountFixture,
        IdentityFixture identityFixture)
    {
        _postgresFixture = postgresFixture;
        _accountFixture = accountFixture;
        _client = identityFixture.CreateClient(_postgresFixture.ConnectionString);
    }
    
    [Fact]
    [TestMethodName("Ok_CredentialsAreValid")]
    public async Task ShouldReturnOk_WhenCredentialsAreValid()
    {
        // Arrange
        var user = _accountFixture.AActiveUser();
        _postgresFixture.IdentityDbContext.Users.Add(user);
        await _postgresFixture.IdentityDbContext.SaveChangesAsync();
        
        using var request = GetResource("SignIn.Valid.json");

        // Act
        using var response = await _client.PostAsync("/api/identity/account/sign-in", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        await VerifyResponse(response);
    }
    
    [Fact]
    [TestMethodName("BadRequest_PasswordIsIncorrect")]
    public async Task ShouldReturnBadRequest_WhenPasswordIsIncorrect()
    {
        // Arrange
        var user = _accountFixture.AActiveUser();
        _postgresFixture.IdentityDbContext.Users.Add(user);
        await _postgresFixture.IdentityDbContext.SaveChangesAsync();
        
        using var request = GetResource("SignIn.InvalidPassword.json");

        // Act
        using var response = await _client.PostAsync("/api/identity/account/sign-in", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        await VerifyResponse(response);
    }
    
    public override async Task DisposeAsync()
    {
        _client.Dispose();
        await _postgresFixture.ResetAsync();
    }
}
