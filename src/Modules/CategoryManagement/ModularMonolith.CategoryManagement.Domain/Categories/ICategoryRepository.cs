namespace ModularMonolith.CategoryManagement.Domain.Categories;

public interface ICategoryRepository
{
    Task AddAsync(Category category, CancellationToken cancellationToken);

    Task UpdateAsync(Category category, CancellationToken cancellationToken);

    Task RemoveAsync(Category category, CancellationToken cancellationToken);

    Task<Category?> FindByIdAsync(CategoryId id, CancellationToken cancellationToken);

    Task<Category?> FindByNameAsync(string name, CancellationToken cancellationToken);

    Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken);

    Task<bool> ExistsByIdAsync(CategoryId id, CancellationToken cancellationToken);
}
