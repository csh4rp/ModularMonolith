using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using ModularMonolith.Identity.Api.IntegrationTests.Account.Shared;
using ModularMonolith.Identity.Api.IntegrationTests.Shared;
using ModularMonolith.Identity.Domain.Users;
using ModularMonolith.Shared.IntegrationTests.Common;
using ModularMonolith.Shared.TestUtils.Abstractions;

namespace ModularMonolith.Identity.Api.IntegrationTests.Account.PasswordReset;

[Collection("Account")]
public class ResetPasswordTests : BaseIntegrationTest<ResetPasswordTests>
{
    private readonly IdentityFixture _identityFixture;
    private readonly AccountFixture _accountFixture;
    private readonly AsyncServiceScope _serviceScope;
    private readonly HttpClient _httpClient;

    public ResetPasswordTests(IdentityFixture identityFixture, AccountFixture accountFixture)
    {
        _identityFixture = identityFixture;
        _accountFixture = accountFixture;
        _serviceScope = _identityFixture.CreateServiceScope();
        _httpClient = _identityFixture.CreateClient();
    }

    [Fact]
    public async Task ShouldReturnNoContent_WhenTokenIsValid()
    {
        // Arrange
        var user = _accountFixture.AActiveUser();

        await _identityFixture.AddUsersAsync(user);


        var manager = _serviceScope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var token = await manager.GeneratePasswordResetTokenAsync(user);

        var payload = new
        {
            UserId = user.Id,
            ResetPasswordToken = token,
            NewPassword = "123!@#Pa$$word!@#123",
            NewPasswordConfirmed = "123!@#Pa$$word!@#123"
        };

        using var request = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        // Act
        using var response = await _httpClient.PostAsync("/api/identity/account/reset-password", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    [TestFileName("BadRequest_UserIdIsInvalid")]
    public async Task ShouldReturnBadRequest_WhenUserIdIsInvalid()
    {
        // Arrange
        var payload = new
        {
            UserId = Guid.NewGuid(),
            ResetPasswordToken = "123",
            NewPassword = "123!@#Pa$$word!@#123",
            NewPasswordConfirmed = "123!@#Pa$$word!@#123"
        };

        using var request = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        // Act
        using var response = await _httpClient.PostAsync("/api/identity/account/reset-password", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        await VerifyResponse(response);
    }

    [Fact]
    [TestFileName("BadRequest_TokenIsInvalid")]
    public async Task ShouldReturnBadRequest_WhenTokenIsInvalid()
    {
        // Arrange
        var user = _accountFixture.AActiveUser();

        await _identityFixture.AddUsersAsync(user);

        var payload = new
        {
            UserId = user.Id,
            ResetPasswordToken = "123",
            NewPassword = "123!@#Pa$$word!@#123",
            NewPasswordConfirmed = "123!@#Pa$$word!@#123"
        };

        using var request = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        // Act
        using var response = await _httpClient.PostAsync("/api/identity/account/reset-password", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        await VerifyResponse(response);
    }

    public override async Task DisposeAsync() => await _identityFixture.ResetAsync();
}
