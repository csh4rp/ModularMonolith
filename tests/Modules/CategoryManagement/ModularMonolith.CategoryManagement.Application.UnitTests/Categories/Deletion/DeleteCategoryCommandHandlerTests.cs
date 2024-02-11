using FluentAssertions;
using ModularMonolith.CategoryManagement.Contracts.Categories.Deletion;
using ModularMonolith.Shared.Application.Exceptions;

namespace ModularMonolith.CategoryManagement.Application.UnitTests.Categories.Deletion;

public class DeleteCategoryCommandHandlerTests
{
    private readonly DeleteCategoryCommandHandlerTestsFixture _fixture = new();

    [Fact]
    public async Task ShouldDeleteCategory_WhenCategoryExists()
    {
        // Arrange
        var existingCategory = _fixture.SetupCategory();

        var command = new DeleteCategoryCommand(existingCategory.Id.Value);

        var handler = _fixture.CreateSut();

        // Act
        await handler.Handle(command, default);

        // Assert
        await _fixture.AssertThatCategoryWasDeleted();
    }

    [Fact]
    public async Task ShouldThrowException_WhenCategoryDoesNotExist()
    {
        // Arrange
        var command = new DeleteCategoryCommand(Guid.NewGuid());

        var handler = _fixture.CreateSut();

        // Act
        var exception = await Assert.ThrowsAsync<EntityNotFoundException>(() =>handler.Handle(command, default));

        // Assert
        exception.Should().NotBeNull();
    }
}
