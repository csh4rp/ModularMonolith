namespace ModularMonolith.Shared.Core;

public interface IDateTimeProvider
{
    DateTimeOffset GetUtcNow();
}
