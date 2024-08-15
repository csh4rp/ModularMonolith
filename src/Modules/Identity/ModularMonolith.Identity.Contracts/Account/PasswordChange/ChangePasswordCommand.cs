using ModularMonolith.Shared.Contracts;

namespace ModularMonolith.Identity.Contracts.Account.PasswordChange;

public sealed record ChangePasswordCommand(string CurrentPassword, string NewPassword, string NewPasswordConfirmed)
    : ICommand;
