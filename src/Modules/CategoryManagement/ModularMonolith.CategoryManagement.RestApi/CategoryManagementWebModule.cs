using Microsoft.OpenApi.Models;
using ModularMonolith.CategoryManagement.RestApi.Categories;
using ModularMonolith.CategoryManagement.Infrastructure;
using ModularMonolith.Shared.RestApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace ModularMonolith.CategoryManagement.RestApi;

public sealed class CategoryManagementWebModule : CategoryManagementModule, IWebAppModule
{
    public WebApplication RegisterEndpoints(WebApplication app)
    {
        app.UseCategoryEndpoints();
        return app;
    }

    public void SwaggerGenAction(SwaggerGenOptions options) =>
        options.SwaggerDoc("category-management-v1", new OpenApiInfo { Version = "v1.0" });

    public void SwaggerUIAction(SwaggerUIOptions options) =>
        options.SwaggerEndpoint("/swagger/category-management-v1/swagger.json", "Category Management v1.0");
}
