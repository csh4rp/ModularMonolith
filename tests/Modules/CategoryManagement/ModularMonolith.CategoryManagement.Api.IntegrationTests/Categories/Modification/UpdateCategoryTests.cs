using System.Net;
using FluentAssertions;
using ModularMonolith.CategoryManagement.Api.IntegrationTests.Categories.Shared;
using ModularMonolith.CategoryManagement.Api.IntegrationTests.Shared;
using ModularMonolith.CategoryManagement.Domain.Categories;
using ModularMonolith.Shared.IntegrationTests.Common;
using ModularMonolith.Shared.TestUtils.Abstractions;

namespace ModularMonolith.CategoryManagement.Api.IntegrationTests.Categories.Modification;

[Collection("Categories")]
public class UpdateCategoryTests : BaseIntegrationTest<UpdateCategoryTests>
{
    private readonly CategoryManagementFixture _categoryManagementFixture;
    private readonly CategoryFixture _categoryFixture;
    private readonly HttpClient _client;

    public UpdateCategoryTests(CategoryManagementFixture categoryManagementFixture, CategoryFixture categoryFixture)
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

        await _categoryManagementFixture.AddCategoriesAsync(category);

        using var request = GetResource("UpdateCategory.Valid.json");

        // Act
        using var response = await _client.PutAsync($"api/category-management/categories/{category.Id.Value}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    [TestFileName("BadRequest_CategoryNameIsEmpty")]
    public async Task ShouldReturnBadRequest_WhenCategoryNameIsEmpty()
    {
        // Arrange
        using var request = GetResource("UpdateCategory.EmptyName.json");

        // Act
        using var response =
            await _client.PutAsync($"api/category-management/categories/00000000-0000-0000-0000-000000000001", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        await VerifyResponse(response);
    }

    [Fact]
    [TestFileName("NotFound_CategoryDoesNotExist")]
    public async Task ShouldReturnNotFound_WhenCategoryDoesNotExist()
    {
        // Arrange
        using var request = GetResource("UpdateCategory.Valid.json");

        // Act
        using var response =
            await _client.PutAsync($"api/category-management/categories/00000000-0000-0000-0000-000000000001", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        await VerifyResponse(response);
    }

    [Fact]
    [TestFileName("Conflict_WhenCategoryNameIsAlreadyUsed")]
    public async Task ShouldReturnConflict_WhenCategoryNameIsAlreadyUsed()
    {
        // Arrange
        var currentCategory = _categoryFixture.Clone()
            .RuleFor(x => x.Id, CategoryId.From(Guid.Parse("775DA6BC-13FE-46E9-9D00-55374A95C542")))
            .Generate();

        var otherCategory = _categoryFixture.Clone()
            .RuleFor(x => x.Name, "Updated-Category-Duplicate")
            .Generate();

        await _categoryManagementFixture.AddCategoriesAsync(currentCategory, otherCategory);

        using var request = GetResource("UpdateCategory.DuplicateName.json");

        // Act
        using var response =
            await _client.PutAsync($"api/category-management/categories/{currentCategory.Id.Value}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        await VerifyResponse(response);
    }

    public override async Task DisposeAsync() => await _categoryManagementFixture.ResetAsync();
}
