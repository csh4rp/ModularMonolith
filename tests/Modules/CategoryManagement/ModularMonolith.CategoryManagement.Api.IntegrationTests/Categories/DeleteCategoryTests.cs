using System.Net;
using FluentAssertions;
using ModularMonolith.CategoryManagement.Api.IntegrationTests.Categories.Fixtures;
using ModularMonolith.CategoryManagement.Api.IntegrationTests.Fixtures;
using ModularMonolith.Shared.IntegrationTests.Common;

namespace ModularMonolith.CategoryManagement.Api.IntegrationTests.Categories;

[Collection("Categories")]
public class DeleteCategoryTests : BaseIntegrationTest<DeleteCategoryTests>
{
    private readonly PostgresFixture _postgresFixture;
    private readonly CategoryFixture _categoryFixture;
    private readonly HttpClient _client;

    public DeleteCategoryTests(PostgresFixture postgresFixture, CategoryFixture categoryFixture, CategoryManagementFixture categoryManagementFixture)
    {
        _postgresFixture = postgresFixture;
        _categoryFixture = categoryFixture;
        _client = categoryManagementFixture.CreateClient(_postgresFixture.ConnectionString);
    }

    [Fact]
    public async Task ShouldReturnNoContent()
    {
        // Arrange
        var category = _categoryFixture.Clone()
            .RuleFor(x => x.Id, f => f.Random.Guid())
            .RuleFor(x => x.Name, f => f.Random.String(10))
            .Generate();

        _postgresFixture.CategoryManagementDbContext.Categories.Add(category);
        await _postgresFixture.CategoryManagementDbContext.SaveChangesAsync();
        
        // Act
        using var response = await _client.DeleteAsync($"categories/{category.Id}");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
    
    [Fact]
    [HasFileName("NotFound")]
    public async Task ShouldReturnNotFound_WhenCategoryDoesNotExist()
    {
        // Arrange & Act
        using var response = await _client.DeleteAsync($"categories/{Guid.Empty}");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
