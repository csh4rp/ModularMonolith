using MediatR;
using Microsoft.AspNetCore.Mvc;
using ModularMonolith.Identity.Contracts.Account.Commands;
using ModularMonolith.Identity.Contracts.Account.Responses;
using ModularMonolith.Shared.Api.Filters;

namespace ModularMonolith.Identity.Api.Account;

internal static class AccountEndpointExtensions
{
    public static WebApplication UseAccountEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("account");

        group.MapPost("sign-in", async ([FromServices] IMediator mediator,
            [FromBody] SignInCommand command,
            CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(command, cancellationToken);

            if (string.IsNullOrEmpty(result.Token))
            {
                return Results.Problem();
            }

            return Results.Ok(result);
        }).AddValidation<SignInCommand>()
        .Produces<SignInResponse>();
        
        group.MapPost("change-password", async ([FromServices] IMediator mediator,
                [FromBody] ChangePasswordCommand command,
                CancellationToken cancellationToken) =>
            {
                await mediator.Send(command, cancellationToken);

                return Results.NoContent();
            }).AddValidation<ChangePasswordCommand>()
            .Produces(StatusCodes.Status204NoContent)
            .RequireAuthorization();

        group.MapPost("initialize-password-reset", async ([FromServices] IMediator mediator,
                [FromBody] InitializePasswordResetCommand command,
                CancellationToken cancellationToken) =>
            {
                await mediator.Send(command, cancellationToken);

                return Results.NoContent();
            }).AddValidation<InitializePasswordResetCommand>()
            .Produces(StatusCodes.Status204NoContent);
        
        group.MapPost("reset-password", async ([FromServices] IMediator mediator,
                [FromBody] ResetPasswordCommand command,
                CancellationToken cancellationToken) =>
            {
                await mediator.Send(command, cancellationToken);

                return Results.NoContent();
            }).AddValidation<ResetPasswordCommand>()
            .Produces(StatusCodes.Status204NoContent);
        
        group.MapPost("register", async ([FromServices] IMediator mediator,
                [FromBody] RegisterCommand command,
                CancellationToken cancellationToken) =>
            {
                await mediator.Send(command, cancellationToken);

                return Results.NoContent();
            }).AddValidation<RegisterCommand>()
            .Produces(StatusCodes.Status204NoContent);

        group.MapPost("verify-account", async ([FromServices] IMediator mediator,
                [FromBody] VerifyAccountCommand command,
                CancellationToken cancellationToken) =>
            {
                await mediator.Send(command, cancellationToken);

                return Results.NoContent();
            }).AddValidation<VerifyAccountCommand>()
            .Produces(StatusCodes.Status204NoContent);
        
        return app;
    }
}
