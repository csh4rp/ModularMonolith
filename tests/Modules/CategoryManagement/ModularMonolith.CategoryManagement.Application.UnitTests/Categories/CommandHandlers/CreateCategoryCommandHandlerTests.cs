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
        result.Should().BeSuccessful();
        
        var item = await dbContext.Categories.FindAsync(result.Value!.Id);
        
        result.Value.Id.Should().NotBeEmpty();
        item.Should().NotBeNull();
        item!.Name.Should().Be(command.Name);
        item.ParentId.Should().Be(command.ParentId);
    }

    [Fact]
    public async Task ShouldReturnConflictError_WhenCategoryNameIsNotUnique()
    {
        // Arrange
        await using var context = CreateDbContext();
        context.Categories.Add(new Category { Id = Guid.NewGuid(), Name = "Category 1" });
        await context.SaveChangesAsync();

        var command = new CreateCategoryCommand(null, "Category 1");

        var handler = new CreateCategoryCommandHandler(context);

        // Act
        var result = await handler.Handle(command, default);

        // Assert
        result.Should().NotBeSuccessful();
        result.Error.Should().BeConflictError()
            .And.HaveTarget(nameof(command.Name)); 
    }
    
    [Fact]
    public async Task ShouldReturnInvalidValueError_WhenParentCategoryDoesNotExist()
    {
        // Arrange
        await using var context = CreateDbContext();

        var command = new CreateCategoryCommand(Guid.NewGuid(), "Category 1");

        var handler = new CreateCategoryCommandHandler(context);

        // Act
        var result =  await handler.Handle(command, default);

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
