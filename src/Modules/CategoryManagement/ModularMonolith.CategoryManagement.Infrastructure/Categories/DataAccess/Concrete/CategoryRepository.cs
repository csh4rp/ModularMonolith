using Microsoft.EntityFrameworkCore;
using ModularMonolith.CategoryManagement.Domain.Categories;

namespace ModularMonolith.CategoryManagement.Infrastructure.Categories.DataAccess.Concrete;

internal sealed class CategoryRepository : ICategoryRepository
{
    private readonly DbContext _dbContext;

    public CategoryRepository(DbContext dbContext)
    {
        _dbContext = dbContext;
        _categories = _dbContext.Set<Category>();
    }

    private DbSet<Category> _categories;

    public Task AddAsync(Category category, CancellationToken cancellationToken)
    {
        _categories.Add(category);
        return _dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task UpdateAsync(Category category, CancellationToken cancellationToken)
    {
        if (_dbContext.Set<Category>().Entry(category).State == EntityState.Detached)
        {
            _dbContext.Set<Category>().Attach(category);
        }
        
        return _dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task RemoveAsync(Category category, CancellationToken cancellationToken) => throw new NotImplementedException();

    public Task<Category?> FindByIdAsync(CategoryId id, CancellationToken cancellationToken) => throw new NotImplementedException();

    public Task<Category?> FindByNameAsync(string name, CancellationToken cancellationToken) => throw new NotImplementedException();

    public Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken) => throw new NotImplementedException();

    public Task<bool> ExistsByIdAsync(CategoryId id, CancellationToken cancellationToken) => throw new NotImplementedException();
}
