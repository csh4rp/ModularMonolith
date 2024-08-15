using ModularMonolith.Shared.DataAccess.EntityFramework.SqlServer.IntegrationTests.Fixtures;

namespace ModularMonolith.Shared.DataAccess.EntityFramework.SqlServer.IntegrationTests.AuditLogs;

[CollectionDefinition("AuditLogs")]
public class AuditLogCollectionFixture : ICollectionFixture<SqlServerFixture>;
