using ModularMonolith.Shared.Contracts;

namespace ModularMonolith.Modules.Identity.Contracts.Account.Commands;

public sealed record ResetPasswordCommand(string Email) : ICommand;

