using ModularMonolith.Shared.Contracts;

namespace ModularMonolith.Identity.Contracts.Account.Verification;

public sealed record VerifyAccountCommand(Guid UserId, string VerificationToken) : ICommand;
