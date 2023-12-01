using Microsoft.EntityFrameworkCore;
using ModularMonolith.Modules.FirstModule.BusinessLogic.Abstract;
using ModularMonolith.Modules.FirstModule.Domain.Entities;

namespace ModularMonolith.Modules.FirstModule.Infrastructure.DataAccess;

internal sealed class CategoryDbContext : DbContext, ICategoryDatabase
{
    public DbSet<Category> Categories { get; } = default!;
}
