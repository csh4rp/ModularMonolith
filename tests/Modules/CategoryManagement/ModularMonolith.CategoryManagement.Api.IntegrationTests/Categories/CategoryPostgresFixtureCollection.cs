using ModularMonolith.CategoryManagement.Api.IntegrationTests.Fixtures;

namespace ModularMonolith.CategoryManagement.Api.IntegrationTests.Categories;

[CollectionDefinition("Categories")]
public class CategoryPostgresFixtureCollection : ICollectionFixture<PostgresFixture>,
    ICollectionFixture<CategoryFixture>;
