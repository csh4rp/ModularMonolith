using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ModularMonolith.Identity.Application.Account.Exceptions;
using ModularMonolith.Identity.Contracts.Account.Commands;
using ModularMonolith.Identity.Domain.Common.Entities;
using ModularMonolith.Identity.Domain.Common.Events;
using ModularMonolith.Shared.Application.Commands;
using ModularMonolith.Shared.Application.Events;
using ModularMonolith.Shared.Contracts;
using ModularMonolith.Shared.Contracts.Errors;

namespace ModularMonolith.Identity.Application.Account.CommandHandlers;

internal sealed class RegisterCommandHandler : ICommandHandler<RegisterCommand>
{
    private readonly UserManager<User> _userManager;
    private readonly IEventBus _eventBus;

    public RegisterCommandHandler(UserManager<User> userManager, IEventBus eventBus)
    {
        _userManager = userManager;
        _eventBus = eventBus;
    }

    public async Task<Result> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var normalizedEmail = _userManager.NormalizeEmail(request.Email);

        var emailExists = await _userManager.Users
            .AnyAsync(u => u.NormalizedEmail == normalizedEmail, cancellationToken);
        
        if (emailExists)
        {
            return MemberError.Conflict(nameof(request.Email));
        }

        var user = new User { UserName = request.Email, Email = request.Email };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            throw new UserRegistrationException(result.Errors);
        }

        await _eventBus.PublishAsync(new UserRegistered(user.Id, user.Email), cancellationToken);
        
        return Result.Successful;
    }
}
