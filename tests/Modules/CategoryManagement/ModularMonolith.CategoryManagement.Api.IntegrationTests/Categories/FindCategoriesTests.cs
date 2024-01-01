using System.Net;
using FluentAssertions;
using ModularMonolith.CategoryManagement.Api.IntegrationTests.Categories.Fixtures;
using ModularMonolith.CategoryManagement.Api.IntegrationTests.Fixtures;
using ModularMonolith.Shared.IntegrationTests.Common;

namespace ModularMonolith.CategoryManagement.Api.IntegrationTests.Categories;

[Collection("Categories")]
public class FindCategoriesTests : BaseIntegrationTest<FindCategoriesTests>
{
    private readonly CategoryManagementFixture _categoryManagementFixture;
    private readonly CategoryFixture _categoryFixture;
    private readonly HttpClient _client;

    public FindCategoriesTests(CategoryManagementFixture categoryManagementFixture, CategoryFixture categoryFixture)
    {
        _categoryManagementFixture = categoryManagementFixture;
        _categoryFixture = categoryFixture;
        _client = categoryManagementFixture.CreateClientWithAuthToken();
    }

    [Fact]
    [TestFileName("Ok_FiltersAreNotSet")]
    public async Task ShouldReturnOk_WhenFiltersAreNotSet()
    {
        // Arrange
        var index = 0;
        var categories = _categoryFixture
            .Clone()
            .RuleFor(x => x.Name, _ => $"Category-{index++}")
            .GenerateForever()
            .Take(20)
            .ToList();

        _categoryManagementFixture.CategoryManagementDbContext.AddRange(categories);
        await _categoryManagementFixture.CategoryManagementDbContext.SaveChangesAsync();

        // Act
        using var response = await _client.GetAsync("api/category-management/categories");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        await VerifyResponse(response);
    }

    [Theory]
    [MemberData(nameof(FilterValues))]
    [TestFileName("Ok_FiltersAreSet")]
    public async Task ShouldReturnOk_WhenFiltersAreSet(int skip, int take, string orderBy, string? search)
    {
        // Arrange
        var index = 0;
        var categories = _categoryFixture
            .Clone()
            .RuleFor(x => x.Name, f => $"Category-{index++}")
            .GenerateForever()
            .Take(20)
            .ToList();

        _categoryManagementFixture.CategoryManagementDbContext.AddRange(categories);
        await _categoryManagementFixture.CategoryManagementDbContext.SaveChangesAsync();

        // Act
        using var response =
            await _client.GetAsync(
                $"api/category-management/categories?skip={skip}&take={take}&orderBy={orderBy}&search={search}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        // response.Headers.Should().ContainSingle(f => f.Key == "X-Total-Length" && f.Value.Any(v => v == "20"));
        await VerifyResponse(response, [skip, take, orderBy, search]);
    }

    public static IEnumerable<object?[]> FilterValues()
    {
        yield return [0, 20, "name:asc", null];
        yield return [0, 20, "name:desc", null];
        yield return [10, 10, "name:asc", null];
        yield return [0, 10, "name:asc", "Category-1"];
    }

    public override async Task DisposeAsync() => await _categoryManagementFixture.ResetAsync();
}
