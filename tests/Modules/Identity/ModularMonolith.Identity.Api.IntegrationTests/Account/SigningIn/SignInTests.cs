using System.Net;
using FluentAssertions;
using ModularMonolith.Identity.Api.IntegrationTests.Account.Fixtures;
using ModularMonolith.Identity.Api.IntegrationTests.Shared;
using ModularMonolith.Identity.Domain.Users;
using ModularMonolith.Shared.IntegrationTests.Common;
using ModularMonolith.Shared.TestUtils.Abstractions;

namespace ModularMonolith.Identity.Api.IntegrationTests.Account.SigningIn;

[Collection("Account")]
public class SignInTests : BaseIntegrationTest<SignInTests>
{
    private readonly IdentityFixture _identityFixture;
    private readonly AccountFixture _accountFixture;
    private readonly HttpClient _client;

    public SignInTests(IdentityFixture identityFixture, AccountFixture accountFixture)
    {
        _identityFixture = identityFixture;
        _accountFixture = accountFixture;
        _client = identityFixture.CreateClient();
    }

    [Fact]
    [TestFileName("Ok_CredentialsAreValid")]
    public async Task ShouldReturnOk_WhenCredentialsAreValid()
    {
        // Arrange
        var user = _accountFixture.AActiveUser();
        _identityFixture.DbContext.Set<User>().Add(user);
        await _identityFixture.DbContext.SaveChangesAsync();

        using var request = GetResource("SignIn.Valid.json");

        // Act
        using var response = await _client.PostAsync("/api/identity/account/sign-in", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        await VerifyResponse(response);
    }

    [Fact]
    [TestFileName("BadRequest_PasswordIsIncorrect")]
    public async Task ShouldReturnBadRequest_WhenPasswordIsIncorrect()
    {
        // Arrange
        var user = _accountFixture.AActiveUser();
        _identityFixture.DbContext.Set<User>().Add(user);
        await _identityFixture.DbContext.SaveChangesAsync();

        using var request = GetResource("SignIn.InvalidPassword.json");

        // Act
        using var response = await _client.PostAsync("/api/identity/account/sign-in", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        await VerifyResponse(response);
    }

    public override async Task DisposeAsync() => await _identityFixture.ResetAsync();
}
