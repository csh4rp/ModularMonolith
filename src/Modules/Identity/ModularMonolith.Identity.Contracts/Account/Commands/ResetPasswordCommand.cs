using ModularMonolith.Shared.Contracts;

namespace ModularMonolith.Identity.Contracts.Account.Commands;

public sealed record ResetPasswordCommand(Guid UserId, string ResetPasswordToken, string NewPassword, string NewPasswordConfirmed)
    : ICommand;
