using FluentValidation;
using Microsoft.AspNetCore.Http.Extensions;
using ModularMonolith.Shared.Api.Models.Errors;

namespace ModularMonolith.Shared.Api.Filters;

internal sealed class ValidationEndpointFilter<TModel>(IValidator<TModel> validator) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var index = GetArgumentIndex(context.Arguments);

        if (index == -1)
        {
            throw new InvalidOperationException($"Could not find argument of type: {typeof(TModel)}");
        }

        var argument = context.GetArgument<TModel>(index);

        var validationResult = await validator.ValidateAsync(argument, context.HttpContext.RequestAborted);

        if (validationResult.IsValid)
        {
            return await next(context);
        }

        var errors = validationResult.Errors.Select(e => new PropertyError
        {
            PropertyName = e.PropertyName,
            Message = e.ErrorMessage, 
            ErrorCode = e.ErrorCode,
            Parameter = e.AttemptedValue
        }).ToArray();

        return Results.BadRequest(new ValidationErrorResponse(context.HttpContext.Request.GetDisplayUrl(), errors));
    }

    private static int GetArgumentIndex(IList<object?> arguments)
    {
        var index = 0;
        while (index < arguments.Count)
        {
            if (arguments[index] is TModel)
            {
                return index;
            }

            index++;
        }

        return -1;
    }
}
