using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ModularMonolith.CategoryManagement.Application.Categories.CommandHandlers;
using ModularMonolith.CategoryManagement.Contracts.Categories.Commands;
using ModularMonolith.CategoryManagement.Domain.Entities;
using ModularMonolith.CategoryManagement.Infrastructure.Common.DataAccess;
using ModularMonolith.Shared.TestUtils.Assertions;
using ModularMonolith.Shared.Contracts.Errors;

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
        var result = await handler.Handle(command, default);

        // Assert
        result.Should().BeSuccessful();

        var updatedCategory = await context.Categories.FindAsync(category.Id);

        updatedCategory.Should().NotBeNull();
        updatedCategory!.Name.Should().Be(command.Name);
        updatedCategory.ParentId.Should().Be(command.ParentId);
    }

    [Fact]
    public async Task ShouldReturnNotFoundError_WhenCategoryDoesNotExist()
    {
        // Arrange
        await using var context = CreateDbContext();

        var command = new UpdateCategoryCommand(Guid.NewGuid(), null, "Category 1");

        var handler = new UpdateCategoryCommandHandler(context);

        // Act
        var result = await handler.Handle(command, default);

        // Assert
        result.Should().NotBeSuccessful();
        result.Error.Should().BeOfType<EntityNotFoundError>();
    }

    [Fact]
    public async Task ShouldReturnConflictError_WhenCategoryWithNameAlreadyExists()
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
        var result = await handler.Handle(command, default);

        // Assert
        result.Should().NotBeSuccessful();
        result.Error.Should().BeConflictError()
            .And.HaveTarget(nameof(command.Name));
    }

    [Fact]
    public async Task ShouldReturnInvalidValueError_WhenParentDoesNotExist()
    {
        // Arrange
        await using var context = CreateDbContext();

        var category = new Category { Id = Guid.NewGuid(), Name = "Category 1" };

        context.Categories.Add(category);
        await context.SaveChangesAsync();

        var command = new UpdateCategoryCommand(category.Id, Guid.NewGuid(), "Category 1-1");

        var handler = new UpdateCategoryCommandHandler(context);

        // Act
        var result = await handler.Handle(command, default);

        // Assert
        result.Should().NotBeSuccessful();
        result.Error.Should().BeMemberError()
            .And.HaveCode(ErrorCodes.InvalidValue)
            .And.HaveTarget(nameof(command.ParentId));
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
