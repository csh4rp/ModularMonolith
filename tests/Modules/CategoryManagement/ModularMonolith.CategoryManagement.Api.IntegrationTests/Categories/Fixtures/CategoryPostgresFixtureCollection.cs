using ModularMonolith.CategoryManagement.Api.IntegrationTests.Fixtures;

namespace ModularMonolith.CategoryManagement.Api.IntegrationTests.Categories.Fixtures;

[CollectionDefinition("Categories")]
public class CategoryPostgresFixtureCollection : ICollectionFixture<PostgresFixture>,
    ICollectionFixture<CategoryFixture>,
    ICollectionFixture<CategoryManagementFixture>;
