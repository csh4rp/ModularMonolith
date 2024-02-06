using ModularMonolith.CategoryManagement.Application.Categories.Modification;
using ModularMonolith.CategoryManagement.Domain.Categories;
using NSubstitute;

namespace ModularMonolith.CategoryManagement.Application.UnitTests.Categories.Modification;

public partial class UpdateCategoryCommandHandlerTests
{
    private class Fixture
    {
        private readonly ICategoryRepository _categoryRepository = Substitute.For<ICategoryRepository>();

        private Category? _category;
        
        public UpdateCategoryCommandHandler CreateSut() => new(_categoryRepository);

        public Category SetupCategory()
        {
            _category = Category.From(new CategoryId(), "Category-1", null);

            _categoryRepository.ExistsByIdAsync(_category.Id, Arg.Any<CancellationToken>())
                .Returns(true);

            _categoryRepository.FindByIdAsync(_category.Id, Arg.Any<CancellationToken>())
                .Returns(_category);

            _categoryRepository.FindByNameAsync(_category.Name, Arg.Any<CancellationToken>())
                .Returns(_category);
            
            return _category;
        }
        
        public Category SetupOtherCategory()
        {
            var category = Category.From(new CategoryId(), "Category-2", null);

            _categoryRepository.ExistsByIdAsync(category.Id, Arg.Any<CancellationToken>())
                .Returns(true);

            _categoryRepository.FindByIdAsync(category.Id, Arg.Any<CancellationToken>())
                .Returns(category);

            _categoryRepository.FindByNameAsync(category.Name, Arg.Any<CancellationToken>())
                .Returns(category);
            
            return category;
        }

        public Task AssertThatCategoryWasUpdated()
        {
            return _categoryRepository.Received(1)
                .UpdateAsync(_category!, Arg.Any<CancellationToken>());
        }
    }
}
