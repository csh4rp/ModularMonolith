namespace ModularMonolith.Modules.Identity.Contracts.Account.Responses;

public sealed record SignInResponse
{
    public string? Token { get; private set; }

    public static SignInResponse Succeeded(string token) => new() { Token = token };

    public static SignInResponse InvalidCredentials() => new();
}
