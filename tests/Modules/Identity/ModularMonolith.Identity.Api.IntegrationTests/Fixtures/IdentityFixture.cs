using System.Net.Http.Headers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ModularMonolith.Shared.TestUtils.Fakes;

namespace ModularMonolith.Identity.Api.IntegrationTests.Fixtures;

public class IdentityFixture : IAsyncLifetime
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
    
    public HttpClient CreateClient(string connectionString, Action<IWebHostBuilder>? action = null)
    {
        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("ConnectionStrings:Database", connectionString);
            builder.UseSetting("Modules:Identity:Enabled", "true");
            builder.UseSetting("Modules:Identity:Auth:Audience", "localhost");
            builder.UseSetting("Modules:Identity:Auth:Issuer", "localhost");
            builder.UseSetting("Modules:Identity:Auth:Key", "12345678123456781234567812345678");
            builder.UseSetting("Modules:Identity:Auth:ExpirationTimeInMinutes", "15");

            action?.Invoke(builder);
            
            builder.ConfigureServices(s =>
            {
                s.Replace(new ServiceDescriptor(typeof(TimeProvider), typeof(FakeTimeProvider), ServiceLifetime.Singleton));
            });
        }).CreateClient();
        
        client.DefaultRequestHeaders.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;v=1.0;q=1.0"));
        client.DefaultRequestHeaders.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;v=2.0;q=0.9"));

        return client;
    }

    public AsyncServiceScope CreateServiceScope(string connectionString)
    {
        return _factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("ConnectionStrings:Database", connectionString);
            builder.UseSetting("Modules:Identity:Enabled", "true");
            builder.UseSetting("Modules:Identity:Auth:Audience", "localhost");
            builder.UseSetting("Modules:Identity:Auth:Issuer", "localhost");
            builder.UseSetting("Modules:Identity:Auth:Key", "12345678123456781234567812345678");
            builder.UseSetting("Modules:Identity:Auth:ExpirationTimeInMinutes", "15");
            
            builder.ConfigureServices(s =>
            {
                s.Replace(new ServiceDescriptor(typeof(TimeProvider), typeof(FakeTimeProvider), ServiceLifetime.Singleton));
            });
        }).Services.CreateAsyncScope();
    }
}
