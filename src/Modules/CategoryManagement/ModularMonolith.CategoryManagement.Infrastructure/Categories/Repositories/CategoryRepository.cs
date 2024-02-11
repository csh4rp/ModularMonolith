using Microsoft.EntityFrameworkCore;
using ModularMonolith.CategoryManagement.Domain.Categories;

namespace ModularMonolith.CategoryManagement.Infrastructure.Categories.Repositories;

internal sealed class CategoryRepository : ICategoryRepository
{
    private readonly DbContext _dbContext;

    public CategoryRepository(DbContext dbContext)
    {
        _dbContext = dbContext;
        _categories = _dbContext.Set<Category>();
    }

    private readonly DbSet<Category> _categories;

    public Task AddAsync(Category category, CancellationToken cancellationToken)
    {
        _categories.Add(category);
        return _dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task UpdateAsync(Category category, CancellationToken cancellationToken)
    {
        if (_categories.Entry(category).State == EntityState.Detached)
        {
            _categories.Attach(category);
        }

        return _dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task RemoveAsync(Category category, CancellationToken cancellationToken)
    {
        _categories.Remove(category);
        return _dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<Category?> FindByIdAsync(CategoryId id, CancellationToken cancellationToken) =>
        _categories.FindAsync([id], cancellationToken: cancellationToken).AsTask();

    public Task<Category?> FindByNameAsync(string name, CancellationToken cancellationToken) =>
        _categories.FirstOrDefaultAsync(c => c.Name == name, cancellationToken);

    public Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken) =>
        _categories.AnyAsync(c => c.Name == name, cancellationToken);

    public Task<bool> ExistsByIdAsync(CategoryId id, CancellationToken cancellationToken) =>
        _categories.AnyAsync(c => c.Id == id, cancellationToken);
}
