using ModularMonolith.Shared.Contracts;

namespace ModularMonolith.Identity.Contracts.Account.Commands;

public sealed record InitializePasswordResetCommand(string Email) : ICommand;

