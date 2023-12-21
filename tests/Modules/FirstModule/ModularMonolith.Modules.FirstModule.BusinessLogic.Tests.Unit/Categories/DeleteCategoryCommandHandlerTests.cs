using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ModularMonolith.Modules.FirstModule.BusinessLogic.Categories.CommandHandlers;
using ModularMonolith.Modules.FirstModule.Contracts.Categories.Commands;
using ModularMonolith.Modules.FirstModule.Domain.Entities;
using ModularMonolith.Modules.FirstModule.Infrastructure.DataAccess;
using ModularMonolith.Shared.BusinessLogic.Exceptions;
using Xunit;

namespace ModularMonolith.Modules.FirstModule.BusinessLogic.Tests.Unit.Categories;

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

    private static FirstModuleDbContext CreateDatabase()
    {
        var optionsBuilder = new DbContextOptionsBuilder<FirstModuleDbContext>();

        optionsBuilder.UseInMemoryDatabase(Guid.NewGuid().ToString(), opt =>
        {
            opt.EnableNullChecks();
        });

        return new FirstModuleDbContext(optionsBuilder.Options);
    }
}
