using ModularMonolith.Shared.Infrastructure.IntegrationTests.Fixtures;

namespace ModularMonolith.Shared.Infrastructure.IntegrationTests.Events;

[CollectionDefinition("Events")]
public class EventsCollectionFixture : ICollectionFixture<PostgresFixture>;
