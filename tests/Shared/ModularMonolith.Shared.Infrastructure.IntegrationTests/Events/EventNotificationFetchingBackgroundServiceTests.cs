using ModularMonolith.Shared.Infrastructure.IntegrationTests.Fixtures;

namespace ModularMonolith.Shared.Infrastructure.IntegrationTests.Events;

public class EventNotificationFetchingBackgroundServiceTests
{
    private readonly PostgresFixture _postgresFixture;
    
    public EventNotificationFetchingBackgroundServiceTests(PostgresFixture postgresFixture)
    {
        _postgresFixture = postgresFixture;
    }
}
