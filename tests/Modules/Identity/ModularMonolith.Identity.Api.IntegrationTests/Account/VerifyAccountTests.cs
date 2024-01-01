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
    private readonly IdentityFixture _identityFixture;
    private readonly AccountFixture _accountFixture;
    private readonly HttpClient _client;
    private readonly AsyncServiceScope _serviceScope;
    
    public VerifyAccountTests(IdentityFixture identityFixture, AccountFixture accountFixture)
    {
        _identityFixture = identityFixture;
        _accountFixture = accountFixture;
        _client = identityFixture.CreateClient();
        _serviceScope = identityFixture.CreateServiceScope();
    }
    
    [Fact]
    public async Task ShouldReturnNoContent_WhenUserIdAndTokenAreValid()
    {
        // Arrange
        var user = _accountFixture.AUnverifiedUser();
        _identityFixture.IdentityDbContext.Users.Add(user);
        await _identityFixture.IdentityDbContext.SaveChangesAsync();

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
    [TestFileName("BadRequest_UserIdIsInvalid")]
    public async Task ShouldReturnBadRequest_WhenUserIdIsInvalid()
    {
        // Arrange
        var user = _accountFixture.AUnverifiedUser();
        _identityFixture.IdentityDbContext.Users.Add(user);
        await _identityFixture.IdentityDbContext.SaveChangesAsync();

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
    [TestFileName("BadRequest_VerificationTokenIsInvalid")]
    public async Task ShouldReturnBadRequest_WhenVerificationTokenIsInvalid()
    {
        // Arrange
        var user = _accountFixture.AUnverifiedUser();
        _identityFixture.IdentityDbContext.Users.Add(user);
        await _identityFixture.IdentityDbContext.SaveChangesAsync();
        
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
        await _identityFixture.ResetAsync();
    }
}
