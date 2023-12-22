using ModularMonolith.Modules.Identity.Contracts.Account.Commands;
using ModularMonolith.Shared.BusinessLogic.Commands;

namespace ModularMonolith.Modules.Identity.BusinessLogic.Account.CommandHandlers;

internal sealed class SignInCommandHandler : ICommandHandler<SignInCommand>
{
    public Task Handle(SignInCommand request, CancellationToken cancellationToken) => throw new NotImplementedException();
}
