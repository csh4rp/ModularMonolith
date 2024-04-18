using Microsoft.EntityFrameworkCore;
using ModularMonolith.Shared.Domain.Abstractions;

namespace ModularMonolith.Shared.DataAccess.SqlServer.Repositories;

public abstract class CrudRepository<TAggregate, TId> where TAggregate : AggregateRoot<TId> where TId : IEquatable<TId>
{
    protected DbContext Context { get; set; }

    public CrudRepository(DbContext context) => Context = context;

    public Task AddAsync(TAggregate aggregate, CancellationToken cancellationToken)
    {
        Context.Set<TAggregate>().Add(aggregate);
        return Context.SaveChangesAsync(cancellationToken);
    }

    public Task AddAsync(IEnumerable<TAggregate> aggregates, CancellationToken cancellationToken)
    {
        Context.Set<TAggregate>().AddRange(aggregates);
        return Context.SaveChangesAsync(cancellationToken);
    }

    public Task UpdateAsync(TAggregate aggregate, CancellationToken cancellationToken)
    {
        if (Context.Entry(aggregate).State == EntityState.Detached)
        {
            Context.Attach(aggregate);
        }

        return Context.SaveChangesAsync(cancellationToken);
    }

    public Task UpdateeAsync(IEnumerable<TAggregate> aggregates, CancellationToken cancellationToken)
    {
        foreach (var aggregate in aggregates)
        {
            if (Context.Entry(aggregate).State == EntityState.Detached)
            {
                Context.Attach(aggregate);
            }
        }

        return Context.SaveChangesAsync(cancellationToken);
    }

    public Task RemoveAsync(TAggregate aggregate, CancellationToken cancellationToken)
    {
        Context.Remove(aggregate);
        return Context.SaveChangesAsync(cancellationToken);
    }

    public Task RemoveAsync(IEnumerable<TAggregate> aggregates, CancellationToken cancellationToken)
    {
        Context.RemoveRange(aggregates);
        return Context.SaveChangesAsync(cancellationToken);
    }

    public Task<TAggregate?> FindByIdAsync(TId id, CancellationToken cancellationToken) =>
        Context.Set<TAggregate>().FindAsync([id], cancellationToken: cancellationToken).AsTask();

    public Task<List<TAggregate>> FindAllByIdsAsync(IEnumerable<TId> ids, CancellationToken cancellationToken) =>
        Context.Set<TAggregate>().Where(a => ids.Contains(a.Id)).ToListAsync(cancellationToken);

    public Task<bool> ExistsByIdAsync(TId id, CancellationToken cancellationToken) =>
        Context.Set<TAggregate>().AnyAsync(c => c.Id.Equals(id), cancellationToken);
}
