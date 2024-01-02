using ModularMonolith.Shared.Infrastructure.IntegrationTests.Fixtures;

namespace ModularMonolith.Shared.Infrastructure.IntegrationTests.AuditLogs;

[CollectionDefinition("AuditLogs")]
public class AuditLogsCollectionFixture : ICollectionFixture<PostgresFixture>;
