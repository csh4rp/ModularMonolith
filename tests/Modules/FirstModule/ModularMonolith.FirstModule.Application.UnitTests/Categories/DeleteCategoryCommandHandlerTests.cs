using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ModularMonolith.FirstModule.Application.Categories.CommandHandlers;
using ModularMonolith.FirstModule.Contracts.Categories.Commands;
using ModularMonolith.FirstModule.Domain.Entities;
using ModularMonolith.FirstModule.Infrastructure.Categories.DataAccess;
using ModularMonolith.Shared.Application.Exceptions;
using Xunit;

namespace ModularMonolith.FirstModule.Application.UnitTests.Categories;

public class DeleteCategoryCommandHandlerTests
{
    [Fact]
    public async Task ShouldDeleteCategory()
    {
        // Arrange
        await using var cnx = CreateDatabase();

        var existingCategory = new Category { Id = Guid.NewGuid(), Name = "Category 1" };

        cnx.Categories.Add(existingCategory);
        await cnx.SaveChangesAsync();

        var cmd = new DeleteCategoryCommand(existingCategory.Id);

        var handler = new DeleteCategoryCommandHandler(cnx);

        // Act
        await handler.Handle(cmd, default);

        // Assert
        var category = await cnx.Categories.FindAsync(existingCategory.Id);

        category.Should().BeNull();
    }

    [Fact]
    public async Task ShouldThrowException_WhenCategoryDoesNotExist()
    {
        // Arrange
        await using var cnx = CreateDatabase();

        var cmd = new DeleteCategoryCommand(Guid.NewGuid());

        var handler = new DeleteCategoryCommandHandler(cnx);

        // Act
        var act = () => handler.Handle(cmd, default);

        // Assert
        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    private static CategoryDbContext CreateDatabase()
    {
        var optionsBuilder = new DbContextOptionsBuilder<CategoryDbContext>();

        optionsBuilder.UseInMemoryDatabase(Guid.NewGuid().ToString(), opt =>
        {
            opt.EnableNullChecks();
        });

        return new CategoryDbContext(optionsBuilder.Options);
    }
}
