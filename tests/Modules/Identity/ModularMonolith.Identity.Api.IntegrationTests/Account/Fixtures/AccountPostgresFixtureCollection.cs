using ModularMonolith.Identity.Api.IntegrationTests.Fixtures;

namespace ModularMonolith.Identity.Api.IntegrationTests.Account.Fixtures;

[CollectionDefinition("Account")]
public class AccountPostgresFixtureCollection : ICollectionFixture<PostgresFixture>,
    ICollectionFixture<IdentityFixture>,
    ICollectionFixture<AccountFixture>;
