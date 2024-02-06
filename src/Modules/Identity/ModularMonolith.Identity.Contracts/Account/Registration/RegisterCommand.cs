using ModularMonolith.Shared.Contracts;

namespace ModularMonolith.Identity.Contracts.Account.Registration;

public sealed record RegisterCommand(string Email, string Password, string PasswordConfirmed) : ICommand;
