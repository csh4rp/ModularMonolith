using ModularMonolith.FirstModule.Api.IntegrationTests.Fixtures;

namespace ModularMonolith.FirstModule.Api.IntegrationTests.Categories;

[CollectionDefinition("Categories")]
public class CategoryPostgresFixtureCollection : ICollectionFixture<PostgresFixture>,
    ICollectionFixture<CategoryFixture>;
