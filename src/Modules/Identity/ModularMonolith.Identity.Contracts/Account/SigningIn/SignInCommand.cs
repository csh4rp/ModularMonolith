using ModularMonolith.Shared.Contracts;

namespace ModularMonolith.Identity.Contracts.Account.SigningIn;

public sealed record SignInCommand(string Email, string Password) : ICommand<SignInResponse>;
