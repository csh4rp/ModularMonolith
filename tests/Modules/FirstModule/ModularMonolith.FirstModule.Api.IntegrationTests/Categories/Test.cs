using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using ModularMonolith.FirstModule.Api.IntegrationTests.Fixtures;
using ModularMonolith.FirstModule.Domain.Entities;

namespace ModularMonolith.FirstModule.Api.IntegrationTests.Categories;

[Collection("Postgres")]
[UsesVerify]
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
        _fixture.FirstModuleDbContext.Categories.Add(new Category { Name = "Category-1" });
        await _fixture.FirstModuleDbContext.SaveChangesAsync();
        
        using var response = await _httpClient.GetAsync("categories");

        var x = response.EnsureSuccessStatusCode();

        await Verify(x.Content);
    }
}

