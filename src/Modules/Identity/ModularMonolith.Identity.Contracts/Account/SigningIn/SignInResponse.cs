namespace ModularMonolith.Identity.Contracts.Account.SigningIn;

public sealed record SignInResponse
{
    public string? Token { get; private set; }

    public static SignInResponse Succeeded(string token) => new() { Token = token };

    public static SignInResponse InvalidCredentials() => new();
}
