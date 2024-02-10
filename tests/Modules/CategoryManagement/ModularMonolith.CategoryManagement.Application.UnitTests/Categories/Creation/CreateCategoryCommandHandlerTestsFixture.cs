using ModularMonolith.CategoryManagement.Application.Categories.Creation;
using ModularMonolith.CategoryManagement.Domain.Categories;
using NSubstitute;

namespace ModularMonolith.CategoryManagement.Application.UnitTests.Categories.Creation;

internal sealed class CreateCategoryCommandHandlerTestsFixture
{
    private readonly ICategoryRepository _categoryRepository = Substitute.For<ICategoryRepository>();

    public CreateCategoryCommandHandler CreateSut() => new(_categoryRepository);

    public Task AssertThatCategoryWasAdded() => 
        _categoryRepository.Received(1).AddAsync(Arg.Any<Category>(), Arg.Any<CancellationToken>());

    public Category SetupExistingCategory()
    {
        var category = Category.From(new CategoryId(), "Category-", null);

        _categoryRepository.ExistsByIdAsync(category.Id, Arg.Any<CancellationToken>())
            .Returns(true);

        _categoryRepository.ExistsByNameAsync(category.Name, Arg.Any<CancellationToken>())
            .Returns(true);
        
        return category;
    }
}
