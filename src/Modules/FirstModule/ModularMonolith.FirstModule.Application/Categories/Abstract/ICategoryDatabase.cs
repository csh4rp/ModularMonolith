using Microsoft.EntityFrameworkCore;
using ModularMonolith.FirstModule.Domain.Entities;

namespace ModularMonolith.FirstModule.Application.Categories.Abstract;

public interface ICategoryDatabase
{
    DbSet<Category> Categories { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
