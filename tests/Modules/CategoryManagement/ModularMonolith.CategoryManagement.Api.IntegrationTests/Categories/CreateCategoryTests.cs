using System.Net;
using FluentAssertions;
using ModularMonolith.CategoryManagement.Api.IntegrationTests.Fixtures;
using ModularMonolith.CategoryManagement.Domain.Entities;
using ModularMonolith.Shared.IntegrationTests.Common;

namespace ModularMonolith.CategoryManagement.Api.IntegrationTests.Categories;

[Collection("Categories")]
public class CreateCategoryTests : BaseIntegrationTest<CreateCategoryTests, Program>
{
    private readonly PostgresFixture _postgresFixture;

    public CreateCategoryTests(PostgresFixture postgresFixture) : base(postgresFixture)
    {
        _postgresFixture = postgresFixture;
    }

    [Fact]
    [HasFileName("Created")]
    public async Task ShouldReturnCreated()
    {
        // Arrange
        using var request = GetResource("CreateCategory.Valid.json");
        
        // Act
        using var response = await HttpClient.PostAsync("categories", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        await VerifyResponse(response);
    }
    
    [Fact]
    [HasFileName("Conflict")]
    public async Task ShouldReturnConflict_WhenCategoryWithGivenNameAlreadyExists()
    {
        // Arrange
        _postgresFixture.CategoryManagementDbContext.Categories.Add(new Category { Id = Guid.NewGuid(), Name = "Category-2" });
        await _postgresFixture.CategoryManagementDbContext.SaveChangesAsync();
        
        using var request = GetResource("CreateCategory.DuplicateName.json");
        
        // Act
        using var response = await HttpClient.PostAsync("categories", request);

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
        using var response = await HttpClient.PostAsync("categories", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        await VerifyResponse(response);
    }
}

