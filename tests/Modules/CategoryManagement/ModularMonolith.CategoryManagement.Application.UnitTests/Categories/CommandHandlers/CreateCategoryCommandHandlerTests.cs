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

public class CreateCategoryCommandHandlerTests
{
    [Fact]
    public async Task ShouldCreateCategory_WhenCategoryNameIsUnique()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        var command = new CreateCategoryCommand(null, "Category 1");

        var handler = new CreateCategoryCommandHandler(dbContext);

        // Act
        var result = await handler.Handle(command, default);

        // Assert
        var item = await dbContext.Categories.FindAsync(result.Id);

        result.Should().NotBeNull();

        result.Id.Should().NotBeEmpty();
        item.Should().NotBeNull();
        item!.Name.Should().Be(command.Name);
        item.ParentId.Should().Be(command.ParentId);
    }

    [Fact]
    public async Task ShouldThrowException_WhenCategoryNameIsNotUnique()
    {
        // Arrange
        await using var context = CreateDbContext();
        context.Categories.Add(new Category { Id = Guid.NewGuid(), Name = "Category 1" });
        await context.SaveChangesAsync();

        var command = new CreateCategoryCommand(null, "Category 1");

        var handler = new CreateCategoryCommandHandler(context);

        // Act
        var act = () => handler.Handle(command, default);

        // Assert
        var exceptionAssertion = await act.Should().ThrowAsync<ConflictException>();
        exceptionAssertion.And.PropertyName.Should().Be(nameof(CreateCategoryCommand.Name));
        exceptionAssertion.And.ErrorCode.Should().Be(ErrorCodes.NotUnique);
    }
    
    [Fact]
    public async Task ShouldThrowException_WhenParentCategoryDoesNotExist()
    {
        // Arrange
        await using var context = CreateDbContext();

        var command = new CreateCategoryCommand(Guid.NewGuid(), "Category 1");

        var handler = new CreateCategoryCommandHandler(context);

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

        optionsBuilder.UseInMemoryDatabase(Guid.NewGuid().ToString(), _ =>
        {
        });

        return new CategoryManagementDbContext(optionsBuilder.Options);
    }
}
