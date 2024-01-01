using System.Net;
using FluentAssertions;
using ModularMonolith.CategoryManagement.Api.IntegrationTests.Categories.Fixtures;
using ModularMonolith.CategoryManagement.Api.IntegrationTests.Fixtures;
using ModularMonolith.Shared.IntegrationTests.Common;

namespace ModularMonolith.CategoryManagement.Api.IntegrationTests.Categories;

[Collection("Categories")]
public class GetCategoryTests : BaseIntegrationTest<GetCategoryTests>
{
    private readonly PostgresFixture _postgresFixture;
    private readonly CategoryFixture _categoryFixture;
    private readonly HttpClient _client;

    public GetCategoryTests(PostgresFixture postgresFixture,
        CategoryFixture categoryFixture,
        CategoryManagementFixture categoryManagementFixture)
    {
        _postgresFixture = postgresFixture;
        _categoryFixture = categoryFixture;
        _client = categoryManagementFixture.CreateClient(_postgresFixture.ConnectionString);
    }

    [Fact]
    [TestMethodName("Ok_CategoryExists")]
    public async Task ShouldReturnOk_WhenCategoryExists()
    {
        // Arrange
        var category = _categoryFixture.Clone()
            .RuleFor(x => x.Name, "Category-Name-1")
            .Generate();
        
        _postgresFixture.CategoryManagementDbContext.Categories.Add(category);
        await _postgresFixture.CategoryManagementDbContext.SaveChangesAsync();
        
        // Act
        using var response = await _client.GetAsync($"api/category-management/categories/{category.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        await VerifyResponse(response);
    }
    
    [Fact]
    [TestMethodName("NotFound_CategoryDoesNotExist")]
    public async Task ShouldReturnNotFound_WhenCategoryDoesNotExist()
    {
        // Arrange & Act
        using var response = await _client.GetAsync($"api/category-management/categories/{Guid.Empty}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        await VerifyResponse(response);
    }
}
