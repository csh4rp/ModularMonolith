using System.Net;
using FluentAssertions;
using ModularMonolith.CategoryManagement.Api.IntegrationTests.Categories.Shared;
using ModularMonolith.CategoryManagement.Api.IntegrationTests.Shared;
using ModularMonolith.Shared.IntegrationTests.Common;
using ModularMonolith.Shared.TestUtils.Abstractions;

namespace ModularMonolith.CategoryManagement.Api.IntegrationTests.Categories.Searching;

[Collection("Categories")]
public class GetCategoryTests : BaseIntegrationTest<GetCategoryTests>
{
    private readonly CategoryManagementFixture _categoryManagementFixture;
    private readonly CategoryFixture _categoryFixture;
    private readonly HttpClient _client;

    public GetCategoryTests(CategoryManagementFixture categoryManagementFixture, CategoryFixture categoryFixture)
    {
        _categoryManagementFixture = categoryManagementFixture;
        _categoryFixture = categoryFixture;
        _client = categoryManagementFixture.CreateClientWithAuthToken();
    }

    [Fact]
    [TestFileName("Ok_CategoryExists")]
    public async Task ShouldReturnOk_WhenCategoryExists()
    {
        // Arrange
        var category = _categoryFixture.Clone()
            .RuleFor(x => x.Name, "Category-Name-1")
            .Generate();

        await _categoryManagementFixture.AddCategoriesAsync(category);

        // Act
        using var response = await _client.GetAsync($"api/category-management/categories/{category.Id.Value}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        await VerifyResponse(response);
    }

    [Fact]
    [TestFileName("NotFound_CategoryDoesNotExist")]
    public async Task ShouldReturnNotFound_WhenCategoryDoesNotExist()
    {
        // Arrange & Act
        using var response =
            await _client.GetAsync($"api/category-management/categories/00000000-0000-0000-0000-000000000001");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        await VerifyResponse(response);
    }
}
