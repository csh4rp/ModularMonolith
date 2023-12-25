using ModularMonolith.Identity.Contracts.Account.Responses;
using ModularMonolith.Shared.Contracts;

namespace ModularMonolith.Identity.Contracts.Account.Commands;

public sealed record SignInCommand(string Email, string Password) : ICommand<SignInResponse>;
