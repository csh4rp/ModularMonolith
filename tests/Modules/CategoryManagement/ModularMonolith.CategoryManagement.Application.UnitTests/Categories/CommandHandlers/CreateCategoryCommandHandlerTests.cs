// using FluentAssertions;
// using Microsoft.EntityFrameworkCore;
// using ModularMonolith.Bootstrapper.Infrastructure;
// using ModularMonolith.CategoryManagement.Application.Categories.Abstract;
// using ModularMonolith.CategoryManagement.Application.Categories.CommandHandlers;
// using ModularMonolith.CategoryManagement.Contracts.Categories.Commands;
// using ModularMonolith.CategoryManagement.Domain.Categories;
// using ModularMonolith.CategoryManagement.Infrastructure.Categories.DataAccess.Concrete;
// using ModularMonolith.Shared.Application.Events;
// using ModularMonolith.Shared.Contracts.Errors;
// using ModularMonolith.Shared.TestUtils.Assertions;
// using NSubstitute;
//
// namespace ModularMonolith.CategoryManagement.Application.UnitTests.Categories.CommandHandlers;
//
// public class CreateCategoryCommandHandlerTests
// {
//     [Fact]
//     public async Task 
//         ShouldCreateCategory_WhenCategoryNameIsUnique()
//     {
//         // Arrange
//         var database = CreateDatabase();
//         var command = new CreateCategoryCommand(null, "Category 1");
//
//         var handler = new CreateCategoryCommandHandler(database);
//
//         // Act
//         var result = await handler.Handle(command, default);
//
//         // Assert
//         result.Should().BeSuccessful();
//
//         var item = await database.Categories.FindAsync(result.Value!.Id);
//
//         result.Value.Id.Should().NotBeEmpty();
//         item.Should().NotBeNull();
//         item!.Name.Should().Be(command.Name);
//         item.ParentId.Should().Be(command.ParentId);
//     }
//
//     [Fact]
//     public async Task ShouldReturnConflictError_WhenCategoryNameIsNotUnique()
//     {
//         // Arrange
//         var database = CreateDatabase();
//         database.Categories.Add(new Category { Id = Guid.NewGuid(), Name = "Category 1" });
//         await database.SaveChangesAsync(default);
//
//         var command = new CreateCategoryCommand(null, "Category 1");
//
//         var handler = new CreateCategoryCommandHandler(database, Substitute.For<IEventBus>());
//
//         // Act
//         var result = await handler.Handle(command, default);
//
//         // Assert
//         result.Should().NotBeSuccessful();
//         result.Error.Should().BeConflictError()
//             .And.HaveTarget(nameof(command.Name));
//     }
//
//     [Fact]
//     public async Task ShouldReturnInvalidValueError_WhenParentCategoryDoesNotExist()
//     {
//         // Arrange
//         var database = CreateDatabase();
//
//         var command = new CreateCategoryCommand(Guid.NewGuid(), "Category 1");
//
//         var handler = new CreateCategoryCommandHandler(database, Substitute.For<IEventBus>());
//
//         // Act
//         var result = await handler.Handle(command, default);
//
//         // Assert
//         result.Should().NotBeSuccessful();
//         result.Error.Should().BeMemberError()
//             .And.HaveCode(ErrorCodes.InvalidValue)
//             .And.HaveTarget(nameof(command.ParentId));
//     }
//
//     private static ICategoryDatabase CreateDatabase()
//     {
//         var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
//
//         optionsBuilder.UseInMemoryDatabase(Guid.NewGuid().ToString(), opt =>
//         {
//             opt.EnableNullChecks();
//         });
//
//         return new CategoryDatabase(new ApplicationDbContext(optionsBuilder.Options));
//     }
// }
