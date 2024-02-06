using ModularMonolith.Shared.Contracts;

namespace ModularMonolith.Identity.Contracts.Account.PasswordReset;

public sealed record InitializePasswordResetCommand(string Email) : ICommand;
