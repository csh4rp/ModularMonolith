using System.Net;
using FluentAssertions;
using ModularMonolith.CategoryManagement.Api.IntegrationTests.Categories.Fixtures;
using ModularMonolith.CategoryManagement.Api.IntegrationTests.Fixtures;
using ModularMonolith.Shared.IntegrationTests.Common;

namespace ModularMonolith.CategoryManagement.Api.IntegrationTests.Categories;

[Collection("Categories")]
public class CreateCategoryTests : BaseIntegrationTest<CreateCategoryTests>
{
    private readonly CategoryManagementFixture _categoryManagementFixture;
    private readonly CategoryFixture _categoryFixture;
    private readonly HttpClient _client;

    public CreateCategoryTests(CategoryManagementFixture categoryManagementFixture, CategoryFixture categoryFixture)
    {
        _categoryManagementFixture = categoryManagementFixture;
        _categoryFixture = categoryFixture;
        _client = categoryManagementFixture.CreateClientWithAuthToken();
    }

    [Fact]
    [TestFileName("Created_CategoryNameIsNotUsed")]
    public async Task ShouldReturnCreated_WhenCategoryNameIsNotUsed()
    {
        // Arrange
        using var request = GetResource("CreateCategory.Valid.json");

        // Act
        using var response = await _client.PostAsync("api/category-management/categories", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        await VerifyResponse(response);
    }

    [Fact]
    [TestFileName("Conflict_CategoryNameIsAlreadyUsed")]
    public async Task ShouldReturnConflict_WhenCategoryNameIsAlreadyUsed()
    {
        // Arrange
        var category = _categoryFixture.Clone()
            .RuleFor(x => x.Name, "Created-Category-Duplicate")
            .Generate();

        _categoryManagementFixture.CategoryManagementDbContext.Categories.Add(category);
        await _categoryManagementFixture.CategoryManagementDbContext.SaveChangesAsync();

        using var request = GetResource("CreateCategory.DuplicateName.json");

        // Act
        using var response = await _client.PostAsync("api/category-management/categories", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        await VerifyResponse(response);
    }

    [Fact]
    [TestFileName("BadRequest_CategoryNameIsEmpty")]
    public async Task ShouldReturnBadRequest_WhenCategoryNameIsEmpty()
    {
        // Arrange
        using var request = GetResource("CreateCategory.EmptyName.json");

        // Act
        using var response = await _client.PostAsync("api/category-management/categories", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        await VerifyResponse(response);
    }

    public override async Task DisposeAsync() => await _categoryManagementFixture.ResetAsync();
}
