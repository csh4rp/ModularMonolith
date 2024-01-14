using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ModularMonolith.Bootstrapper.Infrastructure.DataAccess;
using ModularMonolith.CategoryManagement.Application.Categories.Abstract;
using ModularMonolith.CategoryManagement.Application.Categories.CommandHandlers;
using ModularMonolith.CategoryManagement.Contracts.Categories.Commands;
using ModularMonolith.CategoryManagement.Domain.Entities;
using ModularMonolith.CategoryManagement.Infrastructure.Categories.DataAccess.Concrete;
using ModularMonolith.Shared.Contracts.Errors;
using ModularMonolith.Shared.TestUtils.Assertions;

namespace ModularMonolith.CategoryManagement.Application.UnitTests.Categories.CommandHandlers;

public class DeleteCategoryCommandHandlerTests
{
    [Fact]
    public async Task ShouldDeleteCategory_WhenCategoryExists()
    {
        // Arrange
        var database = CreateDatabase();

        var existingCategory = new Category { Id = Guid.NewGuid(), Name = "Category 1" };

        database.Categories.Add(existingCategory);
        await database.SaveChangesAsync(default);

        var command = new DeleteCategoryCommand(existingCategory.Id);

        var handler = new DeleteCategoryCommandHandler(database);

        // Act
        var result = await handler.Handle(command, default);

        // Assert
        result.Should().BeSuccessful();

        var category = await database.Categories.FindAsync(existingCategory.Id);

        category.Should().BeNull();
    }

    [Fact]
    public async Task ShouldReturnNotFoundError_WhenCategoryDoesNotExist()
    {
        // Arrange
        var database = CreateDatabase();

        var command = new DeleteCategoryCommand(Guid.NewGuid());

        var handler = new DeleteCategoryCommandHandler(database);

        // Act
        var result = await handler.Handle(command, default);

        // Assert
        result.Should().NotBeSuccessful();
        result.Error.Should().BeOfType<EntityNotFoundError>();
    }

    private static ICategoryDatabase CreateDatabase()
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

        optionsBuilder.UseInMemoryDatabase(Guid.NewGuid().ToString(), opt =>
        {
            opt.EnableNullChecks();
        });

        return new CategoryDatabase(new ApplicationDbContext(optionsBuilder.Options));
    }
}
