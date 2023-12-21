namespace ModularMonolith.Shared.Api.Filters;

public static class Extensions
{
    public static RouteHandlerBuilder AddValidation<TModel>(this RouteHandlerBuilder builder)
    {
        builder.AddEndpointFilter<ValidationEndpointFilter<TModel>>();

        return builder;
    }
}
