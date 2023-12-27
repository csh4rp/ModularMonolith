using System.Net;
using FluentAssertions;
using ModularMonolith.CategoryManagement.Api.IntegrationTests.Categories.Fixtures;
using ModularMonolith.CategoryManagement.Api.IntegrationTests.Fixtures;
using ModularMonolith.Shared.IntegrationTests.Common;

namespace ModularMonolith.CategoryManagement.Api.IntegrationTests.Categories;

[Collection("Categories")]
public class CreateCategoryTests : BaseIntegrationTest<CreateCategoryTests>
{
    private readonly PostgresFixture _postgresFixture;
    private readonly CategoryFixture _categoryFixture;
    private readonly HttpClient _client;

    public CreateCategoryTests(PostgresFixture postgresFixture,
        CategoryFixture categoryFixture,
        CategoryManagementFixture categoryManagementFixture)
    {
        _postgresFixture = postgresFixture;
        _categoryFixture = categoryFixture;
        _client = categoryManagementFixture.CreateClient(_postgresFixture.ConnectionString);
    }

    [Fact]
    [HasFileName("Created")]
    public async Task ShouldReturnCreated()
    {
        // Arrange
        using var request = GetResource("CreateCategory.Valid.json");
        
        // Act
        using var response = await _client.PostAsync("categories", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        await VerifyResponse(response);
    }
    
    [Fact]
    [HasFileName("Conflict")]
    public async Task ShouldReturnConflict_WhenCategoryWithGivenNameAlreadyExists()
    {
        // Arrange
        var category = _categoryFixture.Clone()
            .RuleFor(x => x.Name, "Created-Category-Duplicate")
            .Generate();
        
        _postgresFixture.CategoryManagementDbContext.Categories.Add(category);
        await _postgresFixture.CategoryManagementDbContext.SaveChangesAsync();
        
        using var request = GetResource("CreateCategory.DuplicateName.json");
        
        // Act
        using var response = await _client.PostAsync("categories", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        await VerifyResponse(response);
    }
    
    [Fact]
    [HasFileName("BadRequest")]
    public async Task ShouldReturnBadRequest_WhenCategoryNameIsEmpty()
    {
        // Arrange
        using var request = GetResource("CreateCategory.EmptyName.json");
        
        // Act
        using var response = await _client.PostAsync("categories", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        await VerifyResponse(response);
    }
}

