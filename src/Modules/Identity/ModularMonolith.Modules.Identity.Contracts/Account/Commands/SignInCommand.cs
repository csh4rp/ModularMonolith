using ModularMonolith.Modules.Identity.Contracts.Account.Responses;
using ModularMonolith.Shared.Contracts;

namespace ModularMonolith.Modules.Identity.Contracts.Account.Commands;

public sealed record SignInCommand(string Email, string Password) : ICommand<SignInResponse>, ICommand;
