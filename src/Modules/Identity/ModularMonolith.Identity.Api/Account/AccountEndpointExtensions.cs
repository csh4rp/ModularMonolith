using System.Diagnostics;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ModularMonolith.Identity.Contracts.Account.ChangePassword;
using ModularMonolith.Identity.Contracts.Account.PasswordReset;
using ModularMonolith.Identity.Contracts.Account.Registration;
using ModularMonolith.Identity.Contracts.Account.SigningIn;
using ModularMonolith.Identity.Contracts.Account.Verification;
using ModularMonolith.Shared.Api.Filters;
using ModularMonolith.Shared.Api.Models.Errors;
using ModularMonolith.Shared.Contracts.Errors;

namespace ModularMonolith.Identity.Api.Account;

internal static class AccountEndpointExtensions
{
    private const string Prefix = "api/identity/account";

    public static WebApplication UseAccountEndpoints(this WebApplication app)
    {
        var group = app.MapGroup(Prefix)
            .WithApiVersionSet()
            .HasApiVersion(1, 0)
            .WithTags("Account")
            .WithGroupName("identity-v1");

        group.MapPost("sign-in", async (HttpContext httpContext,
                [FromServices] IMediator mediator,
                [FromBody] SignInCommand command,
                CancellationToken cancellationToken) =>
            {
                var response = await mediator.Send(command, cancellationToken);

                if (response.IsSuccessful)
                {
                    return Results.Ok(response);
                }

                var errorResponse = new ValidationErrorResponse(httpContext.Request.Path,
                    Activity.Current!.TraceId.ToString(),
                    new[] { MemberError.InvalidValue(nameof(command.Email)) });

                return Results.BadRequest(errorResponse);
            })
            .AddValidation<SignInCommand>()
            .Produces<SignInResponse>();

        group.MapPost("change-password", async ([FromServices] IMediator mediator,
                [FromBody] ChangePasswordCommand command,
                CancellationToken cancellationToken) =>
            {
                await mediator.Send(command, cancellationToken);
                return Results.NoContent();
            })
            .AddValidation<ChangePasswordCommand>()
            .Produces(StatusCodes.Status204NoContent)
            .RequireAuthorization();

        group.MapPost("initialize-password-reset", async ([FromServices] IMediator mediator,
                [FromBody] InitializePasswordResetCommand command,
                CancellationToken cancellationToken) =>
            {
                await mediator.Send(command, cancellationToken);
                return Results.NoContent();
            })
            .AddValidation<InitializePasswordResetCommand>()
            .Produces(StatusCodes.Status204NoContent);

        group.MapPost("reset-password", async ([FromServices] IMediator mediator,
                [FromBody] ResetPasswordCommand command,
                CancellationToken cancellationToken) =>
            {
                await mediator.Send(command, cancellationToken);
                return Results.NoContent();
            })
            .AddValidation<ResetPasswordCommand>()
            .Produces(StatusCodes.Status204NoContent);

        group.MapPost("register", async ([FromServices] IMediator mediator,
                [FromBody] RegisterCommand command,
                CancellationToken cancellationToken) =>
            {
                await mediator.Send(command, cancellationToken);
                return Results.NoContent();
            })
            .AddValidation<RegisterCommand>()
            .Produces(StatusCodes.Status204NoContent);

        group.MapPost("verify", async ([FromServices] IMediator mediator,
                [FromBody] VerifyAccountCommand command,
                CancellationToken cancellationToken) =>
            {
                await mediator.Send(command, cancellationToken);
                return Results.NoContent();
            })
            .AddValidation<VerifyAccountCommand>()
            .Produces(StatusCodes.Status204NoContent);

        return app;
    }
}
