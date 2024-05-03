using FluentAssertions;
using ModularMonolith.CategoryManagement.Application.Categories.Shared;
using ModularMonolith.CategoryManagement.Contracts.Categories.Creation;
using ModularMonolith.Shared.Application.Exceptions;

namespace ModularMonolith.CategoryManagement.Application.UnitTests.Categories.Creation;

public class CreateCategoryCommandHandlerTests
{
    private readonly CreateCategoryCommandHandlerTestsFixture _fixture = new();

    [Fact]
    public async Task ShouldCreateCategory_WhenCategoryNameIsUnique()
    {
        // Arrange
        var command = new CreateCategoryCommand(null, "Category 1");

        var handler = _fixture.CreateSut();

        // Act
        var response = await handler.Handle(command, default);

        // Assert
        response.Should().NotBeNull();
        response.Id.Should().NotBeEmpty();

        await _fixture.AssertThatCategoryWasAdded();
    }

    [Fact]
    public async Task ShouldThrowException_WhenCategoryNameIsNotUnique()
    {
        // Arrange
        var existingCategory = _fixture.SetupExistingCategory();

        var command = new CreateCategoryCommand(null, existingCategory.Name);

        var handler = _fixture.CreateSut();

        // Act
        var exception = await Assert.ThrowsAsync<CategoryNameConflictException>(() => handler.Handle(command, default));

        exception.Should().NotBeNull();
    }

    [Fact]
    public async Task ShouldReturnThrowException_WhenParentCategoryDoesNotExist()
    {
        // Arrange
        var command = new CreateCategoryCommand(Guid.NewGuid(), "Category 1");

        var handler = _fixture.CreateSut();

        // Act
        var exception = await Assert.ThrowsAsync<ValidationException>(() => handler.Handle(command, default));

        // Assert
        exception.Should().NotBeNull();
        exception.Errors.Should().HaveCount(1);
        exception.Errors[0].Code.Should().Be(ErrorCodes.InvalidValue);
    }
}
