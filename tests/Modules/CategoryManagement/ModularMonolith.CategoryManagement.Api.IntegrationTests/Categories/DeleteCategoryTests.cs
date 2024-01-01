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

    public DeleteCategoryTests(PostgresFixture postgresFixture,
        CategoryFixture categoryFixture,
        CategoryManagementFixture categoryManagementFixture)
    {
        _postgresFixture = postgresFixture;
        _categoryFixture = categoryFixture;
        _client = categoryManagementFixture.CreateClient(_postgresFixture.ConnectionString);
    }

    [Fact]
    public async Task ShouldReturnNoContent_WhenCategoryExists()
    {
        // Arrange
        var category = _categoryFixture.Generate();
        _postgresFixture.CategoryManagementDbContext.Categories.Add(category);
        await _postgresFixture.CategoryManagementDbContext.SaveChangesAsync();
        
        // Act
        using var response = await _client.DeleteAsync($"api/category-management/categories/{category.Id}");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
    
    [Fact]
    [TestFileName("NotFound_CategoryDoesNotExist")]
    public async Task ShouldReturnNotFound_WhenCategoryDoesNotExist()
    {
        // Arrange & Act
        using var response = await _client.DeleteAsync($"api/category-management/categories/{Guid.Empty}");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    public override async Task DisposeAsync()
    {
        await base.DisposeAsync();
        await _postgresFixture.ResetAsync();
    }
}
