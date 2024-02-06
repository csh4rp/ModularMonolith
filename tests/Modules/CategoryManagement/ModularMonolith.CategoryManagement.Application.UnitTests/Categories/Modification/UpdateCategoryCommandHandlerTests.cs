using FluentAssertions;
using ModularMonolith.CategoryManagement.Application.Categories.Shared;
using ModularMonolith.CategoryManagement.Contracts.Categories.Modification;
using ModularMonolith.Shared.Application.Exceptions;
using ModularMonolith.Shared.Contracts.Errors;

namespace ModularMonolith.CategoryManagement.Application.UnitTests.Categories.Modification;

public partial class UpdateCategoryCommandHandlerTests
{
    private readonly Fixture _fixture = new();
    
    [Fact]
    public async Task ShouldUpdateCategory_WhenNameIsUnique()
    {
        // Arrange
        var category = _fixture.SetupCategory();

        var command = new UpdateCategoryCommand(category.Id.Value, null, "Category 1-1");

        var handler = _fixture.CreateSut();

        // Act
        await handler.Handle(command, default);

        // Assert
        await _fixture.AssertThatCategoryWasUpdated();
    }

    [Fact]
    public async Task ShouldThrowException_WhenCategoryDoesNotExist()
    {
        // Arrange
        var command = new UpdateCategoryCommand(Guid.NewGuid(), null, "Category 1");

        var handler = _fixture.CreateSut();

        // Act
        var exception = await Assert.ThrowsAsync<EntityNotFoundException>(() => handler.Handle(command, default));

        // Assert
        exception.Should().NotBeNull();
    }

    [Fact]
    public async Task ShouldThrowException_WhenCategoryWithNameAlreadyExists()
    {
        // Arrange
        var category = _fixture.SetupCategory();
        var otherCategory = _fixture.SetupOtherCategory();
        
        var command = new UpdateCategoryCommand(category.Id.Value, null, otherCategory.Name);

        var handler = _fixture.CreateSut();

        // Act
        var exception = await Assert.ThrowsAsync<CategoryNameConflictException>(() => handler.Handle(command, default));

        // Assert
        exception.Should().NotBeNull();
    }

    [Fact]
    public async Task ShouldThrowException_WhenParentDoesNotExist()
    {
        // Arrange
        var category = _fixture.SetupCategory();

        var command = new UpdateCategoryCommand(category.Id.Value, Guid.NewGuid(), "Category 1-1");

        var handler = _fixture.CreateSut();

        // Act
        var exception = await Assert.ThrowsAsync<ValidationException>(() => handler.Handle(command, default));

        // Assert
        exception.Should().NotBeNull();
        exception.Errors.Should().HaveCount(1);
        exception.Errors[0].Code.Should().Be(ErrorCodes.InvalidValue);
        exception.Errors[0].Reference.Should().Be(nameof(command.ParentId));
    }
}
