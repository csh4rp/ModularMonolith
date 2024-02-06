using ModularMonolith.CategoryManagement.Application.Categories.Deletion;
using ModularMonolith.CategoryManagement.Domain.Categories;
using NSubstitute;

namespace ModularMonolith.CategoryManagement.Application.UnitTests.Categories.Deletion;

public partial class DeleteCategoryCommandHandlerTests
{
    private class Fixture
    {
        private readonly ICategoryRepository _categoryRepository = Substitute.For<ICategoryRepository>();

        private Category? _category;
        
        public DeleteCategoryCommandHandler CreateSut() => new(_categoryRepository);

        public Category SetupCategory()
        {
            _category = Category.From(new CategoryId(), "Category-1", null);
            
            _categoryRepository.FindByIdAsync(_category.Id, Arg.Any<CancellationToken>())
                .Returns(_category);
            
            return _category;
        }

        public Task AssertThatCategoryWasDeleted() => _categoryRepository.Received(1)
            .RemoveAsync(_category!, Arg.Any<CancellationToken>());
    }
}
