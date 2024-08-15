using ModularMonolith.Shared.DataAccess.EntityFramework.Postgres.IntegrationTests.Fixtures;

namespace ModularMonolith.Shared.DataAccess.EntityFramework.Postgres.IntegrationTests.AuditLogs;

[CollectionDefinition("AuditLogs")]
public class AuditLogCollectionFixture : ICollectionFixture<PostgresFixture>;
