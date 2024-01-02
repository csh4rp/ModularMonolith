using Microsoft.EntityFrameworkCore;
using ModularMonolith.Shared.Infrastructure.DataAccess;

namespace ModularMonolith.Shared.Infrastructure.IntegrationTests.Events;

public class EventLogTestDbContext : BaseDbContext
{
    public EventLogTestDbContext(DbContextOptions<EventLogTestDbContext> options) : base(options)
    {
    }
}
