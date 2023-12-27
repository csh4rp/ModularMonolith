using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace ModularMonolith.CategoryManagement.Api.IntegrationTests.Fixtures;

public class CategoryManagementFixture : IAsyncLifetime
{
    private WebApplicationFactory<Program> _factory = default!;
    
    public Task InitializeAsync()
    {
        _factory = new WebApplicationFactory<Program>();
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        await _factory.DisposeAsync();
    }
    
    public HttpClient CreateClient(string connectionString, Action<IWebHostBuilder>? action = null) =>
        _factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("ConnectionStrings:Database", connectionString);
            builder.UseSetting("Modules:CategoryManagement:Enabled", "true");

            action?.Invoke(builder);
        }).CreateClient();
}
