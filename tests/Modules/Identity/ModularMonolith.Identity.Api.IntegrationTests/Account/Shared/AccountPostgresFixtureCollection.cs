using ModularMonolith.Identity.Api.IntegrationTests.Account.Fixtures;
using ModularMonolith.Identity.Api.IntegrationTests.Shared;

namespace ModularMonolith.Identity.Api.IntegrationTests.Account.Shared;

[CollectionDefinition("Account")]
public class AccountPostgresFixtureCollection : ICollectionFixture<IdentityFixture>, ICollectionFixture<AccountFixture>;
