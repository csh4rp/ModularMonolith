using System.Net;
using FluentAssertions;
using ModularMonolith.Identity.Api.IntegrationTests.Account.Fixtures;
using ModularMonolith.Identity.Api.IntegrationTests.Fixtures;
using ModularMonolith.Shared.IntegrationTests.Common;
using ModularMonolith.Shared.TestUtils.Abstractions;

namespace ModularMonolith.Identity.Api.IntegrationTests.Account;

[Collection("Account")]
public class ChangePasswordTests : BaseIntegrationTest<ChangePasswordTests>
{
    private static readonly Guid UserId = Guid.Parse("FDCD0B4A-828E-40ED-9DDB-AB987B2E70F7");

    private readonly IdentityFixture _identityFixture;
    private readonly AccountFixture _accountFixture;

    public ChangePasswordTests(IdentityFixture identityFixture, AccountFixture accountFixture)
    {
        _identityFixture = identityFixture;
        _accountFixture = accountFixture;
    }

    [Fact]
    public async Task ShouldReturnNoContent_WhenPasswordIsValid()
    {
        // Arrange
        var user = _accountFixture.AActiveUser();
        user.Id = UserId;
        _identityFixture.Database.Users.Add(user);
        await _identityFixture.Database.SaveChangesAsync(default);

        using var client = _identityFixture.CreateClientWithAuthToken(UserId);
        using var request = GetResource("ChangePassword.Valid.json");

        // Act
        using var response = await client.PostAsync("/api/identity/account/change-password", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    [TestFileName("BadRequest_CurrentPasswordIsInvalid")]
    public async Task ShouldReturnNoContent_WhenCurrentPasswordIsInvalid()
    {
        // Arrange
        var user = _accountFixture.AActiveUser();
        user.Id = UserId;
        _identityFixture.Database.Users.Add(user);
        await _identityFixture.Database.SaveChangesAsync(default);

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
        _identityFixture.Database.Users.Add(user);
        await _identityFixture.Database.SaveChangesAsync(default);

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

    public override async Task DisposeAsync() => await _identityFixture.ResetAsync();
}
