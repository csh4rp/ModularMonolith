using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ModularMonolith.FirstModule.Application.Categories.CommandHandlers;
using ModularMonolith.FirstModule.Contracts.Categories.Commands;
using ModularMonolith.FirstModule.Domain.Entities;
using ModularMonolith.FirstModule.Infrastructure.Categories.DataAccess;
using ModularMonolith.FirstModule.Infrastructure.Common;
using ModularMonolith.FirstModule.Infrastructure.Common.DataAccess;
using ModularMonolith.Shared.Application.Exceptions;
using ModularMonolith.Shared.Contracts.Errors;
using Xunit;

namespace ModularMonolith.FirstModule.Application.UnitTests.Categories;

public class UpdateCategoryCommandHandlerTests
{
    [Fact]
    public async Task ShouldUpdateCategory()
    {
        // Arrange
        await using var cnx = CreateDatabase();

        var category = new Category { Id = Guid.NewGuid(), Name = "Category 1" };

        cnx.Categories.Add(category);
        await cnx.SaveChangesAsync();

        var cmd = new UpdateCategoryCommand { Id = category.Id, Name = "Category 1-1", ParentId = Guid.NewGuid() };

        var handler = new UpdateCategoryCommandHandler(cnx);

        // Act
        await handler.Handle(cmd, default);

        // Assert
        var updatedCategory = await cnx.Categories.FindAsync(category.Id);

        updatedCategory.Should().NotBeNull();
        updatedCategory!.Name.Should().Be(cmd.Name);
        updatedCategory.ParentId.Should().Be(cmd.ParentId);
    }

    [Fact]
    public async Task ShouldThrowException_WhenCategoryDoesNotExist()
    {
        // Arrange
        await using var cnx = CreateDatabase();

        var cmd = new UpdateCategoryCommand { Id = Guid.NewGuid(), ParentId = Guid.NewGuid(), Name = "Category 1" };

        var handler = new UpdateCategoryCommandHandler(cnx);

        // Act
        var act = () => handler.Handle(cmd, default);

        // Assert
        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task ShouldThrowException_WhenCategoryWithNameAlreadyExists()
    {
        // Arrange
        await using var cnx = CreateDatabase();

        var category = new Category { Id = Guid.NewGuid(), Name = "Category 1-1" };
        var existingCategoryWithName = new Category { Id = Guid.NewGuid(), Name = "Category 1" };

        cnx.Categories.Add(category);
        cnx.Categories.Add(existingCategoryWithName);
        await cnx.SaveChangesAsync();

        var cmd = new UpdateCategoryCommand { Id = category.Id, Name = "Category 1", ParentId = Guid.NewGuid() };

        var handler = new UpdateCategoryCommandHandler(cnx);

        // Act
        var act = () => handler.Handle(cmd, default);

        // Assert
        var expectedErrors = new[] { PropertyError.NotUnique(nameof(cmd.Name), cmd.Name) };

        var exceptionAssertion = await act.Should().ThrowAsync<ValidationException>();
        exceptionAssertion.And.Errors.Should().BeEquivalentTo(expectedErrors);
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
