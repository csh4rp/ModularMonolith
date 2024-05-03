namespace ModularMonolith.Shared.Domain.Abstractions;

public interface IEntity
{
    IEnumerable<DomainEvent> DequeueEvents();
}
