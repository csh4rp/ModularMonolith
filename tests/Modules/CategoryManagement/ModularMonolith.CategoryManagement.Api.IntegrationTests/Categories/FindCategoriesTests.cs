using System.Net;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ModularMonolith.CategoryManagement.Api.IntegrationTests.Categories.Fixtures;
using ModularMonolith.CategoryManagement.Api.IntegrationTests.Fixtures;
using ModularMonolith.Shared.IntegrationTests.Common;

namespace ModularMonolith.CategoryManagement.Api.IntegrationTests.Categories;

[Collection("Categories")]
public class FindCategoriesTests : BaseIntegrationTest<FindCategoriesTests>
{
    private readonly PostgresFixture _postgresFixture;
    private readonly CategoryFixture _categoryFixture;
    private readonly HttpClient _client;

    public FindCategoriesTests(PostgresFixture postgresFixture,
        CategoryFixture categoryFixture,
        CategoryManagementFixture categoryManagementFixture)
    {
        _postgresFixture = postgresFixture;
        _categoryFixture = categoryFixture;
        _client = categoryManagementFixture.CreateClient(_postgresFixture.ConnectionString);
    }

    [Fact]
    [TestMethodName("Ok.NoFilters")]
    public async Task ShouldReturnOk()
    {
        // Arrange
        await _postgresFixture.CategoryManagementDbContext.Categories.ExecuteDeleteAsync();

        int index = 0;
        var categories = _categoryFixture
            .Clone()
            .RuleFor(x => x.Name, _ => $"Category-{index++}")
            .GenerateForever()
            .Take(20)
            .ToList();
        
        _postgresFixture.CategoryManagementDbContext.AddRange(categories);
        await _postgresFixture.CategoryManagementDbContext.SaveChangesAsync();
        
        // Act
        using var response = await _client.GetAsync("api/category-management/categories");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        await VerifyResponse(response);
    }
    
    [Theory]
    [MemberData(nameof(FilterValues))]
    [TestMethodName("Ok.WithFilters")]
    public async Task ShouldReturnOk_WhenGettingCategoriesWithFilters(int skip, int take, string orderBy, string? search)
    {
        // Arrange
        await _postgresFixture.CategoryManagementDbContext.Categories.ExecuteDeleteAsync();

        int index = 0;
        var categories = _categoryFixture
            .Clone()
            .RuleFor(x => x.Name, f => $"Category-{index++}")
            .GenerateForever()
            .Take(20)
            .ToList();
        
        _postgresFixture.CategoryManagementDbContext.AddRange(categories);
        await _postgresFixture.CategoryManagementDbContext.SaveChangesAsync();
        
        // Act
        using var response = await _client.GetAsync($"api/category-management/categories?skip={skip}&take={take}&orderBy={orderBy}&search={search}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        // response.Headers.Should().ContainSingle(f => f.Key == "X-Total-Length" && f.Value.Any(v => v == "20"));
        await VerifyResponse(response, parameters: [skip, take, orderBy, search]);
    }

    public static IEnumerable<object?[]> FilterValues()
    {
        yield return [0, 20, "name:asc", null];
        yield return [0, 20, "name:desc", null];
        yield return [10, 10, "name:asc", null];
        yield return [0, 10, "name:asc", "Category-1"];
    }
}
