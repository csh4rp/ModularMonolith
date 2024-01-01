using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ModularMonolith.CategoryManagement.Application.Categories.CommandHandlers;
using ModularMonolith.CategoryManagement.Contracts.Categories.Commands;
using ModularMonolith.CategoryManagement.Domain.Entities;
using ModularMonolith.CategoryManagement.Infrastructure.Common.DataAccess;
using ModularMonolith.Shared.Contracts.Errors;
using ModularMonolith.Shared.TestUtils.Assertions;
using Xunit;

namespace ModularMonolith.CategoryManagement.Application.UnitTests.Categories.CommandHandlers;

public class DeleteCategoryCommandHandlerTests
{
    [Fact]
    public async Task ShouldDeleteCategory_WhenCategoryExists()
    {
        // Arrange
        await using var context = CreateDbContext();

        var existingCategory = new Category { Id = Guid.NewGuid(), Name = "Category 1" };

        context.Categories.Add(existingCategory);
        await context.SaveChangesAsync();

        var command = new DeleteCategoryCommand(existingCategory.Id);

        var handler = new DeleteCategoryCommandHandler(context);

        // Act
        var result = await handler.Handle(command, default);

        // Assert
        result.Should().BeSuccessful();

        var category = await context.Categories.FindAsync(existingCategory.Id);

        category.Should().BeNull();
    }

    [Fact]
    public async Task ShouldReturnNotFoundError_WhenCategoryDoesNotExist()
    {
        // Arrange
        await using var context = CreateDbContext();

        var command = new DeleteCategoryCommand(Guid.NewGuid());

        var handler = new DeleteCategoryCommandHandler(context);

        // Act
        var result = await handler.Handle(command, default);

        // Assert
        result.Should().NotBeSuccessful();
        result.Error.Should().BeOfType<EntityNotFoundError>();
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
