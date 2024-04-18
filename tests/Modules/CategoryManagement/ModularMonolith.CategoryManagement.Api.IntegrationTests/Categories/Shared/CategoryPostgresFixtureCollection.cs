
using ModularMonolith.CategoryManagement.Api.IntegrationTests.Shared;

namespace ModularMonolith.CategoryManagement.Api.IntegrationTests.Categories.Shared;

[CollectionDefinition("Categories")]
public class CategoryPostgresFixtureCollection : ICollectionFixture<CategoryFixture>,
    ICollectionFixture<CategoryManagementFixture>;
