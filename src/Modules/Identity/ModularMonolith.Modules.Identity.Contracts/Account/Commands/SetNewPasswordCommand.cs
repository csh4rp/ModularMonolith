using ModularMonolith.Shared.Contracts;

namespace ModularMonolith.Modules.Identity.Contracts.Account.Commands;

public sealed record SetNewPasswordCommand(string Email, string Token, string Password, string PasswordConfirmed) : ICommand;
