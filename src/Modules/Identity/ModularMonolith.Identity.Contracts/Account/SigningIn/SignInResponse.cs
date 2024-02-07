namespace ModularMonolith.Identity.Contracts.Account.SigningIn;

public sealed record SignInResponse
{
    public string? Token { get; private set; }
    
    public bool IsSuccessful { get; private set; }

    public static SignInResponse Succeeded(string token) => new()
    {
        Token = token,
        IsSuccessful = true
    };

    public static SignInResponse InvalidCredentials() => new();
}
