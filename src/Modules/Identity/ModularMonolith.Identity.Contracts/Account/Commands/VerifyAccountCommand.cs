using ModularMonolith.Shared.Contracts;

namespace ModularMonolith.Identity.Contracts.Account.Commands;

public sealed record VerifyAccountCommand(Guid UserId, string VerificationToken) : ICommand;
