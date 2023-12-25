using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using ModularMonolith.FirstModule.Api.IntegrationTests.Fixtures;

namespace ModularMonolith.FirstModule.Api.IntegrationTests.Categories;

[Collection("Postgres")]
public class Test
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _httpClient;
    private readonly PostgresFixture _fixture;
    
    public Test(PostgresFixture fixture)
    {
        _fixture = fixture;
        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(b =>
        {
            b.ConfigureAppConfiguration((c, s) =>
            {
                s.AddInMemoryCollection(new []
                {
                    new KeyValuePair<string, string?>("ConnectionStrings:Database", _fixture.ConnectionString)
                });
            });
        });

        _httpClient = _factory.CreateClient();
    }

    [Fact]
    public async Task ShouldRun()
    {
        using var response = await _httpClient.GetAsync("categories");

        var x = response.EnsureSuccessStatusCode();
    }
}

