using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ModularMonolith.CategoryManagement.Application.Categories.CommandHandlers;
using ModularMonolith.CategoryManagement.Contracts.Categories.Commands;
using ModularMonolith.CategoryManagement.Domain.Entities;
using ModularMonolith.CategoryManagement.Infrastructure.Common.DataAccess;
using ModularMonolith.Shared.Application.Exceptions;
using ModularMonolith.Shared.Contracts;
using ModularMonolith.Shared.Contracts.Errors;
using Xunit;

namespace ModularMonolith.CategoryManagement.Application.UnitTests.Categories.CommandHandlers;

public class UpdateCategoryCommandHandlerTests
{
    [Fact]
    public async Task ShouldUpdateCategory_WhenNameIsUnique()
    {
        // Arrange
        await using var context = CreateDbContext();

        var category = new Category { Id = Guid.NewGuid(), Name = "Category 1" };

        context.Categories.Add(category);
        await context.SaveChangesAsync();

        var command = new UpdateCategoryCommand(category.Id, null, "Category 1-1");

        var handler = new UpdateCategoryCommandHandler(context);

        // Act
        await handler.Handle(command, default);

        // Assert
        var updatedCategory = await context.Categories.FindAsync(category.Id);

        updatedCategory.Should().NotBeNull();
        updatedCategory!.Name.Should().Be(command.Name);
        updatedCategory.ParentId.Should().Be(command.ParentId);
    }

    [Fact]
    public async Task ShouldThrowException_WhenCategoryDoesNotExist()
    {
        // Arrange
        await using var context = CreateDbContext();

        var command = new UpdateCategoryCommand(Guid.NewGuid(), null, "Category 1");

        var handler = new UpdateCategoryCommandHandler(context);

        // Act
        var act = () => handler.Handle(command, default);

        // Assert
        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task ShouldThrowException_WhenCategoryWithNameAlreadyExists()
    {
        // Arrange
        await using var context = CreateDbContext();

        var category = new Category { Id = Guid.NewGuid(), Name = "Category 1" };
        var existingCategoryWithName = new Category { Id = Guid.NewGuid(), Name = "Category 1-1" };

        context.Categories.Add(category);
        context.Categories.Add(existingCategoryWithName);
        await context.SaveChangesAsync();

        var command = new UpdateCategoryCommand(category.Id, null, "Category 1-1");

        var handler = new UpdateCategoryCommandHandler(context);

        // Act
        var act = () => handler.Handle(command, default);

        // Assert
        var exceptionAssertion = await act.Should().ThrowAsync<ConflictException>();
        exceptionAssertion.And.PropertyName.Should().Be(nameof(UpdateCategoryCommand.Name));
        exceptionAssertion.And.ErrorCode.Should().Be(ErrorCodes.NotUnique);
    }
    
    [Fact]
    public async Task ShouldThrowException_WhenParentDoesNotExist()
    {
        // Arrange
        await using var context = CreateDbContext();

        var category = new Category { Id = Guid.NewGuid(), Name = "Category 1" };

        context.Categories.Add(category);
        await context.SaveChangesAsync();

        var command = new UpdateCategoryCommand(category.Id, Guid.NewGuid() ,"Category 1-1");

        var handler = new UpdateCategoryCommandHandler(context);

        // Act
        var act = () => handler.Handle(command, default);

        // Assert
        var expectedErrors = new[] { PropertyError.InvalidArgument(nameof(command.ParentId), command.ParentId) };

        var exceptionAssertion = await act.Should().ThrowAsync<ValidationException>();
        exceptionAssertion.And.Errors.Should().BeEquivalentTo(expectedErrors);
    }

    private static CategoryManagementDbContext CreateDbContext()
    {
        var optionsBuilder = new DbContextOptionsBuilder<CategoryManagementDbContext>();

        optionsBuilder.UseInMemoryDatabase(Guid.NewGuid().ToString(), opt =>
        {
            opt.EnableNullChecks();
        });

        return new CategoryManagementDbContext(optionsBuilder.Options);
    }
}
