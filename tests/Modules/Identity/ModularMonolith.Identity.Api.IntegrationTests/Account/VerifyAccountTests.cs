using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using ModularMonolith.Identity.Api.IntegrationTests.Account.Fixtures;
using ModularMonolith.Identity.Api.IntegrationTests.Fixtures;
using ModularMonolith.Identity.Domain.Common.Entities;
using ModularMonolith.Shared.IntegrationTests.Common;

namespace ModularMonolith.Identity.Api.IntegrationTests.Account;

[Collection("Account")]
public class VerifyAccountTests : BaseIntegrationTest<VerifyAccountTests>
{
    private readonly PostgresFixture _postgresFixture;
    private readonly AccountFixture _accountFixture;
    private readonly AsyncServiceScope _serviceScope;
    private readonly HttpClient _client;
    
    public VerifyAccountTests(PostgresFixture postgresFixture,
        AccountFixture accountFixture,
        IdentityFixture identityFixture)
    {
        _postgresFixture = postgresFixture;
        _accountFixture = accountFixture;
        _serviceScope = identityFixture.CreateServiceScope(_postgresFixture.ConnectionString);
        _client = identityFixture.CreateClient(_postgresFixture.ConnectionString);
    }

    [Fact]
    public async Task ShouldReturnNoContent_WhenUserIdAndTokenAreValid()
    {
        // Arrange
        var user = _accountFixture.AUnverifiedUser();
        _postgresFixture.IdentityDbContext.Users.Add(user);
        await _postgresFixture.IdentityDbContext.SaveChangesAsync();

        var manager = _serviceScope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var token = await manager.GenerateEmailConfirmationTokenAsync(user);

        var payload = new { UserId = user.Id, VerificationToken = token };
        
        using var request = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        // Act
        using var response = await _client.PostAsync("/api/identity/account/verify", request);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
    
    [Fact]
    [TestMethodName("BadRequest_UserIdIsInvalid")]
    public async Task ShouldReturnBadRequest_WhenUserIdIsInvalid()
    {
        // Arrange
        var user = _accountFixture.AUnverifiedUser();
        _postgresFixture.IdentityDbContext.Users.Add(user);
        await _postgresFixture.IdentityDbContext.SaveChangesAsync();

        var manager = _serviceScope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var token = await manager.GenerateEmailConfirmationTokenAsync(user);

        var payload = new { UserId = Guid.NewGuid(), VerificationToken = token };
        
        using var request = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        // Act
        using var response = await _client.PostAsync("/api/identity/account/verify", request);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        await VerifyResponse(response);
    }

    [Fact]
    [TestMethodName("BadRequest_VerificationTokenIsInvalid")]
    public async Task ShouldReturnBadRequest_WhenVerificationTokenIsInvalid()
    {
        // Arrange
        var user = _accountFixture.AUnverifiedUser();
        _postgresFixture.IdentityDbContext.Users.Add(user);
        await _postgresFixture.IdentityDbContext.SaveChangesAsync();
        
        var payload = new { UserId = user.Id, VerificationToken = "123" };
        
        using var request = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        // Act
        using var response = await _client.PostAsync("/api/identity/account/verify", request);
        
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
