using System.Net;
using FluentAssertions;
using ModularMonolith.CategoryManagement.Api.IntegrationTests.Categories.Shared;
using ModularMonolith.CategoryManagement.Api.IntegrationTests.Shared;
using ModularMonolith.CategoryManagement.Domain.Categories;
using ModularMonolith.Shared.IntegrationTests.Common;
using ModularMonolith.Shared.TestUtils.Abstractions;

namespace ModularMonolith.CategoryManagement.Api.IntegrationTests.Categories.Deletion;

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
        _categoryManagementFixture.DbContext.Set<Category>().Add(category);
        await _categoryManagementFixture.DbContext.SaveChangesAsync();

        // Act
        using var response = await _client.DeleteAsync($"api/category-management/categories/{category.Id.Value}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    [TestFileName("NotFound_CategoryDoesNotExist")]
    public async Task ShouldReturnNotFound_WhenCategoryDoesNotExist()
    {
        // Arrange & Act
        using var response = await _client.DeleteAsync($"api/category-management/categories/{Guid.Parse("00000000-0000-0000-0000-000000000001")}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    public override async Task DisposeAsync() => await _categoryManagementFixture.ResetAsync();
}
