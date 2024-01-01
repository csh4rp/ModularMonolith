using System.Net;
using FluentAssertions;
using ModularMonolith.CategoryManagement.Api.IntegrationTests.Categories.Fixtures;
using ModularMonolith.CategoryManagement.Api.IntegrationTests.Fixtures;
using ModularMonolith.Shared.IntegrationTests.Common;

namespace ModularMonolith.CategoryManagement.Api.IntegrationTests.Categories;

[Collection("Categories")]
public class DeleteCategoryTests : BaseIntegrationTest<DeleteCategoryTests>
{
    private readonly CategoryManagementFixture _categoryManagementFixture;
    private readonly CategoryFixture _categoryFixture;
    private readonly HttpClient _client;

    public DeleteCategoryTests(CategoryManagementFixture categoryManagementFixture, CategoryFixture categoryFixture)
    {
        _categoryManagementFixture = categoryManagementFixture;
        _categoryFixture = categoryFixture;
        _client = categoryManagementFixture.CreateClientWithAuthToken();
    }

    [Fact]
    public async Task ShouldReturnNoContent_WhenCategoryExists()
    {
        // Arrange
        var category = _categoryFixture.Generate();
        _categoryManagementFixture.CategoryManagementDbContext.Categories.Add(category);
        await _categoryManagementFixture.CategoryManagementDbContext.SaveChangesAsync();

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

    public override async Task DisposeAsync() => await _categoryManagementFixture.ResetAsync();
}
