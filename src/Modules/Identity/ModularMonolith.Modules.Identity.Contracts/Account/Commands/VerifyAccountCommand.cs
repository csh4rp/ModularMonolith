using ModularMonolith.Shared.Contracts;

namespace ModularMonolith.Modules.Identity.Contracts.Account.Commands;

public sealed record VerifyAccountCommand(Guid UserId, string VerificationToken) : ICommand;
