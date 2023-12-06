using MediatR;
using Microsoft.AspNetCore.Mvc;
using ModularMonolith.Modules.FirstModule.Contracts.Categories.Commands;
using ModularMonolith.Modules.FirstModule.Contracts.Categories.Models;
using ModularMonolith.Modules.FirstModule.Contracts.Categories.Queries;
using ModularMonolith.Modules.FirstModule.Contracts.Categories.Responses;
using ModularMonolith.Shared.Api.CustomResults;
using ModularMonolith.Shared.Api.Filters;

namespace ModularMonolith.Modules.FirstModule.Api.Categories;

internal static class CategoryEndpointExtensions
{
    private const string Prefix = "categories";

    public static WebApplication UseCategoryEndpoints(this WebApplication app)
    {
        var group = app.MapGroup(Prefix);

        group.MapPost(string.Empty, async (
                [FromServices] IMediator mediator,
                [FromBody] CreateCategoryCommand command,
                CancellationToken cancellationToken) =>
            {
                var result = await mediator.Send(command, cancellationToken);

                return TypedResults.Created($"{Prefix}/{result}", result);
            })
            .AddValidation<CreateCategoryCommand>()
            .Produces<Guid>();

        app.MapPut("{id:guid}", async (
                [FromServices] IMediator mediator,
                [FromBody] UpdateCategoryCommand command,
                [FromRoute] Guid id,
                CancellationToken cancellationToken) =>
            {
                command.Id = id;

                await mediator.Send(command, cancellationToken);

                return TypedResults.NoContent();
            })
            .AddValidation<UpdateCategoryCommand>()
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        app.MapDelete("{id:guid}", async (
                [FromServices] IMediator mediator,
                [FromRoute] Guid id,
                CancellationToken cancellationToken) =>
            {
                var command = new DeleteCategoryCommand(id);

                await mediator.Send(command, cancellationToken);

                return TypedResults.NoContent();
            })
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        app.MapGet("{id:guid}", async (
                [FromServices] IMediator mediator,
                [FromRoute] Guid id,
                CancellationToken cancellationToken) =>
            {
                var query = new GetCategoryDetailsByIdQuery(id);

                var response = await mediator.Send(query, cancellationToken);

                return response is null ? Results.NotFound() : Results.Ok(response);
            })
            .Produces<CategoryDetailsResponse>()
            .Produces(StatusCodes.Status404NotFound);

        app.MapGet(string.Empty, async (
                [FromServices] IMediator mediator,
                [AsParameters] FindCategoriesQuery query,
                CancellationToken cancellationToken) =>
            {
                var response = await mediator.Send(query, cancellationToken);

                return PaginatedResult.From(response);
            })
            .AddValidation<FindCategoriesQuery>()
            .Produces<CategoryItemModel[]>();

        return app;
    }
}
