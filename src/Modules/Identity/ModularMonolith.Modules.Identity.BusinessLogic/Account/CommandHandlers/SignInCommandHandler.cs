using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ModularMonolith.Modules.Identity.BusinessLogic.Account.Exceptions;
using ModularMonolith.Modules.Identity.Contracts.Account.Commands;
using ModularMonolith.Modules.Identity.Contracts.Account.Responses;
using ModularMonolith.Modules.Identity.Core.Options;
using ModularMonolith.Modules.Identity.Domain.Users.Entities;
using ModularMonolith.Shared.BusinessLogic.Commands;

namespace ModularMonolith.Modules.Identity.BusinessLogic.Account.CommandHandlers;

internal sealed class SignInCommandHandler : ICommandHandler<SignInCommand, SignInResponse>
{
    private readonly UserManager<User> _userManager;
    private readonly IOptions<AuthOptions> _options;
    private readonly TimeProvider _timeProvider;

    public SignInCommandHandler(UserManager<User> userManager, IOptions<AuthOptions> options, TimeProvider timeProvider)
    {
        _userManager = userManager;
        _options = options;
        _timeProvider = timeProvider;
    }

    public async Task<SignInResponse> Handle(SignInCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email)
                   ?? throw new InvalidCredentialsException();

        var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);

        if (!isPasswordValid)
        {
            throw new InvalidCredentialsException();
        }

        var options = _options.Value;
        
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.Key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier,user.UserName!),
        };

        var expirationTime = _timeProvider.GetUtcNow().AddMinutes(options.ExpirationTimeInMinutes);
        
        var token = new JwtSecurityToken(options.Issuer,
            options.Audience,
            claims,
            expires: expirationTime.UtcDateTime,
            signingCredentials: credentials);
        
        var tokenValue = new JwtSecurityTokenHandler().WriteToken(token);

        return new SignInResponse(tokenValue);
    }
}
