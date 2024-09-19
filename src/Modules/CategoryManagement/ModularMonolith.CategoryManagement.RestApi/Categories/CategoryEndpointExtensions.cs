using MediatR;
using Microsoft.AspNetCore.Mvc;
using ModularMonolith.CategoryManagement.Contracts.Categories.Creation;
using ModularMonolith.CategoryManagement.Contracts.Categories.Deletion;
using ModularMonolith.CategoryManagement.Contracts.Categories.Modification;
using ModularMonolith.CategoryManagement.Contracts.Categories.Querying;
using ModularMonolith.CategoryManagement.Contracts.Categories.Shared;
using ModularMonolith.Shared.RestApi.CustomResults;
using ModularMonolith.Shared.RestApi.Filters;
using ModularMonolith.Shared.RestApi.Models.Errors;
using ModularMonolith.Shared.Contracts;

namespace ModularMonolith.CategoryManagement.RestApi.Categories;

internal static class CategoryEndpointExtensions
{
    private const string Prefix = "api/category-management/categories";

    public static WebApplication UseCategoryEndpoints(this WebApplication app)
    {
        var group = app.MapGroup(Prefix)
            .RequireAuthorization()
            .WithApiVersionSet()
            .HasApiVersion(1, 0)
            .WithTags("Categories")
            .WithGroupName("category-management-v1");

        group.MapPost(string.Empty, async (
                [FromServices] IMediator mediator,
                [FromBody] CreateCategoryCommand command,
                CancellationToken cancellationToken) =>
            {
                var result = await mediator.Send(command, cancellationToken);
                return ApiResult.From(result);
            })
            .AddValidation<CreateCategoryCommand>()
            .Produces<ValidationErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<CreatedResponse>();

        group.MapPut("{id:guid}", async (
                [FromServices] IMediator mediator,
                [FromBody] UpdateCategoryCommand command,
                [FromRoute] Guid id,
                CancellationToken cancellationToken) =>
            {
                command.Id = id;
                await mediator.Send(command, cancellationToken);
                return Results.NoContent();
            })
            .AddValidation<UpdateCategoryCommand>()
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ValidationErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        group.MapDelete("{id:guid}", async (
                [FromServices] IMediator mediator,
                [FromRoute] Guid id,
                CancellationToken cancellationToken) =>
            {
                var command = new DeleteCategoryCommand(id);
                await mediator.Send(command, cancellationToken);
                return Results.NoContent();
            })
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        group.MapGet("{id:guid}", async (
                [FromServices] IMediator mediator,
                [FromRoute] Guid id,
                CancellationToken cancellationToken) =>
            {
                var query = new GetCategoryDetailsByIdQuery(id);
                var response = await mediator.Send(query, cancellationToken);

                return Results.Ok(response);
            })
            .Produces<CategoryDetailsResponse>()
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        group.MapGet(string.Empty, async (
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
