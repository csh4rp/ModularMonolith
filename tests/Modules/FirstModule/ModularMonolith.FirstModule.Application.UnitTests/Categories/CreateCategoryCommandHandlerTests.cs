using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ModularMonolith.FirstModule.Application.Categories.CommandHandlers;
using ModularMonolith.FirstModule.Contracts.Categories.Commands;
using ModularMonolith.FirstModule.Domain.Entities;
using ModularMonolith.FirstModule.Infrastructure.Categories.DataAccess;
using ModularMonolith.Shared.Application.Exceptions;
using ModularMonolith.Shared.Contracts.Errors;
using Xunit;

namespace ModularMonolith.FirstModule.Application.UnitTests.Categories;

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

        result.Should().NotBeNull();

        result.Id.Should().NotBeEmpty();
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
        var act = () => handler.Handle(cmd, default);

        // Assert
        var expectedErrors = new[] { PropertyError.NotUnique(nameof(cmd.Name), cmd.Name) };

        var exceptionAssertion = await act.Should().ThrowAsync<ValidationException>();
        exceptionAssertion.And.Errors.Should().BeEquivalentTo(expectedErrors);
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
