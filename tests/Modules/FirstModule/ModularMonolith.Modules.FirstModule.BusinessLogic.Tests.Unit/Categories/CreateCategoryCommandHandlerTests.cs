using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ModularMonolith.Modules.FirstModule.BusinessLogic.Categories.CommandHandlers;
using ModularMonolith.Modules.FirstModule.Contracts.Categories.Commands;
using ModularMonolith.Modules.FirstModule.Domain.Entities;
using ModularMonolith.Modules.FirstModule.Infrastructure.DataAccess.Categories;
using ModularMonolith.Shared.BusinessLogic.Exceptions;
using Xunit;

namespace ModularMonolith.Modules.FirstModule.BusinessLogic.Tests.Unit.Categories;

public class CreateCategoryCommandHandlerTests
{
    [Fact]
    public async Task ShouldCreateCategory()
    {
        // Arrange
        await using var dbContext = CreateDatabase();
        var cmd = new CreateCategoryCommand { ParentId = Guid.NewGuid(), Name = "Category 1" };

        var handler = new CreateCategoryCommandHandler(dbContext);
        
        // Act
        var result = await handler.Handle(cmd, default);

        // Assert
        var item = await dbContext.Categories.FindAsync(result);

        result.Should().NotBeEmpty();
        
        item.Should().NotBeNull();
        item!.Name.Should().Be(cmd.Name);
        item.ParentId.Should().Be(cmd.ParentId);
    }
    
    [Fact]
    public async Task ShouldThrowException_WhenCategoryNameIsNotUnique()
    {
        // Arrange
        await using var dbContext = CreateDatabase();
        dbContext.Categories.Add(new Category { Id = Guid.NewGuid(), Name = "Category 1" });
        await dbContext.SaveChangesAsync();
        
        var cmd = new CreateCategoryCommand { ParentId = Guid.NewGuid(), Name = "Category 1" };

        var handler = new CreateCategoryCommandHandler(dbContext);
        
        // Act
        Func<Task> act = () => handler.Handle(cmd, default);

        // Assert
        var exceptionAssertion = await act.Should().ThrowAsync<ValidationException>();
        exceptionAssertion.And.Errors.Should().HaveCount(1);
        exceptionAssertion.And.Errors[0].PropertyName.Should().Be(nameof(cmd.Name));
        exceptionAssertion.And.Errors[0].Parameter.Should().Be(cmd.Name);
    }

    private static CategoryDbContext CreateDatabase()
    {
        var optionsBuilder = new DbContextOptionsBuilder<CategoryDbContext>();

        optionsBuilder.UseInMemoryDatabase(Guid.NewGuid().ToString());

        return new CategoryDbContext(optionsBuilder.Options);
    }
}
