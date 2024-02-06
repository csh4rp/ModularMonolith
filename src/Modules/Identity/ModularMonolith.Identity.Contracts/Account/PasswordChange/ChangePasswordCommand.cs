using ModularMonolith.Shared.Contracts;

namespace ModularMonolith.Identity.Contracts.Account.ChangePassword;

public sealed record ChangePasswordCommand(string CurrentPassword, string NewPassword, string NewPasswordConfirmed)
    : ICommand;
