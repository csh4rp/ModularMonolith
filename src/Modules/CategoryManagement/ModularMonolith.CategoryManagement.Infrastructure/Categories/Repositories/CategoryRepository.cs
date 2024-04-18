using Microsoft.EntityFrameworkCore;
using ModularMonolith.CategoryManagement.Domain.Categories;
using ModularMonolith.Shared.DataAccess.Postgres.Repositories;

namespace ModularMonolith.CategoryManagement.Infrastructure.Categories.Repositories;

internal sealed class CategoryRepository : CrudRepository<Category, CategoryId>, ICategoryRepository
{
    private readonly DbSet<Category> _categories;

    public CategoryRepository(DbContext dbContext) : base(dbContext) => _categories = Context.Set<Category>();

    public Task<Category?> FindByNameAsync(string name, CancellationToken cancellationToken) =>
        _categories.FirstOrDefaultAsync(c => c.Name == name, cancellationToken);

    public Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken) =>
        _categories.AnyAsync(c => c.Name == name, cancellationToken);
}
