using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ModularMonolith.CategoryManagement.Application.Categories.CommandHandlers;
using ModularMonolith.CategoryManagement.Contracts.Categories.Commands;
using ModularMonolith.CategoryManagement.Domain.Entities;
using ModularMonolith.CategoryManagement.Infrastructure.Common.DataAccess;
using ModularMonolith.Shared.Application.Exceptions;
using ModularMonolith.Shared.Contracts.Errors;
using Xunit;

namespace ModularMonolith.CategoryManagement.Application.UnitTests.Categories.CommandHandlers;

public class CreateCategoryCommandHandlerTests
{
    [Fact]
    public async Task ShouldCreateCategory()
    {
        // Arrange
        await using var dbContext = CreateDatabase();
        var cmd = new CreateCategoryCommand(Guid.NewGuid(), "Category 1");

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

        var cmd = new CreateCategoryCommand(Guid.NewGuid(), "Category 1");

        var handler = new CreateCategoryCommandHandler(dbContext);

        // Act
        var act = () => handler.Handle(cmd, default);

        // Assert
        var expectedErrors = new[] { PropertyError.NotUnique(nameof(cmd.Name), cmd.Name) };

        var exceptionAssertion = await act.Should().ThrowAsync<ValidationException>();
        exceptionAssertion.And.Errors.Should().BeEquivalentTo(expectedErrors);
    }

    private static CategoryManagementDbContext CreateDatabase()
    {
        var optionsBuilder = new DbContextOptionsBuilder<CategoryManagementDbContext>();

        optionsBuilder.UseInMemoryDatabase(Guid.NewGuid().ToString(), _ =>
        {
        });

        return new CategoryManagementDbContext(optionsBuilder.Options);
    }
}
