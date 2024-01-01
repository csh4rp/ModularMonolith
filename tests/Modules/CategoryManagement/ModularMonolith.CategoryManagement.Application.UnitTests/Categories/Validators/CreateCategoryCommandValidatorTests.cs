using FluentAssertions;
using FluentValidation.TestHelper;
using ModularMonolith.CategoryManagement.Application.Categories.Validators;
using ModularMonolith.CategoryManagement.Contracts.Categories.Commands;
using Xunit;

namespace ModularMonolith.CategoryManagement.Application.UnitTests.Categories.Validators;

public class CreateCategoryCommandValidatorTests
{
    private readonly CreateCategoryCommandValidator _validator = new();

    [Fact]
    public void ShouldReturnValid_WhenCommandIsValid()
    {
        // Arrange
        var command = new CreateCategoryCommand(Guid.NewGuid(), "Category-1");

        // Act
        var validationResult = _validator.TestValidate(command);

        // Assert
        validationResult.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ShouldReturnInvalid_WhenNameIsEmpty()
    {
        // Arrange
        var command = new CreateCategoryCommand(null, string.Empty);

        // Act
        var validationResult = _validator.TestValidate(command);

        // Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Errors.Should().HaveCount(1);
        validationResult.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorCode("NotEmptyValidator");
    }
}
