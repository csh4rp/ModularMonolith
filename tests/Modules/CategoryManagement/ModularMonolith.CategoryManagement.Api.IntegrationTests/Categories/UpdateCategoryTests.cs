using System.Net;
using FluentAssertions;
using ModularMonolith.CategoryManagement.Api.IntegrationTests.Categories.Fixtures;
using ModularMonolith.CategoryManagement.Api.IntegrationTests.Fixtures;
using ModularMonolith.Shared.IntegrationTests.Common;

namespace ModularMonolith.CategoryManagement.Api.IntegrationTests.Categories;

[Collection("Categories")]
public class UpdateCategoryTests : BaseIntegrationTest<UpdateCategoryTests>
{
    private readonly PostgresFixture _postgresFixture;
    private readonly CategoryFixture _categoryFixture;
    private readonly HttpClient _client;

    public UpdateCategoryTests(PostgresFixture postgresFixture,
        CategoryFixture categoryFixture,
        CategoryManagementFixture categoryManagementFixture)
    {
        _postgresFixture = postgresFixture;
        _categoryFixture = categoryFixture;
        _client = categoryManagementFixture.CreateClient(_postgresFixture.ConnectionString);
    }

    [Fact]
    public async Task ShouldReturnNoContent()
    {
        // Arrange
        var category = _categoryFixture.Generate();

        _postgresFixture.CategoryManagementDbContext.Categories.Add(category);
        await _postgresFixture.CategoryManagementDbContext.SaveChangesAsync();
        
        using var request = GetResource("UpdateCategory.Valid.json");
        
        // Act
        using var response = await _client.PutAsync($"categories/{category.Id}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
    
    [Fact]
    [TestMethodName("BadRequest")]
    public async Task ShouldReturnBadRequest_WhenCategoryNameIsEmpty()
    {
        // Arrange
        using var request = GetResource("UpdateCategory.EmptyName.json");
        
        // Act
        using var response = await _client.PutAsync($"categories/{Guid.Empty}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        await VerifyResponse(response);
    }
    
    [Fact]
    [TestMethodName("NotFound")]
    public async Task ShouldReturnNotFound_WhenCategoryDoesNotExist()
    {
        // Arrange
        using var request = GetResource("UpdateCategory.Valid.json");
        
        // Act
        using var response = await _client.PutAsync($"categories/{Guid.Empty}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        await VerifyResponse(response);
    }
    
    [Fact]
    [TestMethodName("Conflict")]
    public async Task ShouldReturnConflict_WhenCategoryWithGivenNameAlreadyExists()
    {
        // Arrange
        var currentCategory = _categoryFixture.Clone()
            .RuleFor(x => x.Id, Guid.Parse("775DA6BC-13FE-46E9-9D00-55374A95C542"))
            .Generate();
        
        var otherCategory = _categoryFixture.Clone()
            .RuleFor(x => x.Name, "Updated-Category-Duplicate")
            .Generate();

        await _postgresFixture.CategoryManagementDbContext.Categories.AddRangeAsync(currentCategory, otherCategory);
        await _postgresFixture.CategoryManagementDbContext.SaveChangesAsync();
        
        using var request = GetResource("UpdateCategory.DuplicateName.json");
        
        // Act
        using var response = await _client.PutAsync($"categories/{currentCategory.Id}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        await VerifyResponse(response);
    }
}
