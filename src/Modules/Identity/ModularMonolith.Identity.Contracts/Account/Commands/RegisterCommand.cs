using ModularMonolith.Shared.Contracts;

namespace ModularMonolith.Identity.Contracts.Account.Commands;

public sealed record RegisterCommand(string Email, string Password, string PasswordConfirmed) : ICommand;
