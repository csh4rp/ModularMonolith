using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ModularMonolith.Identity.Contracts.Account.Commands;
using ModularMonolith.Identity.Contracts.Account.Responses;
using ModularMonolith.Identity.Core.Options;
using ModularMonolith.Identity.Domain.Common.Entities;
using ModularMonolith.Identity.Domain.Common.Events;
using ModularMonolith.Shared.Application.Commands;
using ModularMonolith.Shared.Application.Events;
using ModularMonolith.Shared.Contracts;
using ModularMonolith.Shared.Contracts.Errors;

namespace ModularMonolith.Identity.Application.Account.CommandHandlers;

internal sealed class SignInCommandHandler : ICommandHandler<SignInCommand, SignInResponse>
{
    private readonly UserManager<User> _userManager;
    private readonly IEventBus _eventBus;
    private readonly IOptions<AuthOptions> _options;
    private readonly TimeProvider _timeProvider;

    public SignInCommandHandler(UserManager<User> userManager, IEventBus eventBus, IOptions<AuthOptions> options,
        TimeProvider timeProvider)
    {
        _userManager = userManager;
        _eventBus = eventBus;
        _options = options;
        _timeProvider = timeProvider;
    }

    public async Task<Result<SignInResponse>> Handle(SignInCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            return MemberError.InvalidValue(nameof(request.Password));
        }

        var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!isPasswordValid)
        {
            await _eventBus.PublishAsync(new SignInFailed(user.Id), cancellationToken);

            return MemberError.InvalidValue(nameof(request.Password));
        }

        var options = _options.Value;

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.Key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, user.UserName!) };

        var expirationTime = _timeProvider.GetUtcNow().AddMinutes(options.ExpirationTimeInMinutes);

        var token = new JwtSecurityToken(options.Issuer,
            options.Audience,
            claims,
            expires: expirationTime.UtcDateTime,
            signingCredentials: credentials);

        var tokenValue = new JwtSecurityTokenHandler().WriteToken(token);

        await _eventBus.PublishAsync(new SignInSucceeded(user.Id), cancellationToken);

        return SignInResponse.Succeeded(tokenValue);
    }
}
