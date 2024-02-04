using ModularMonolith.Shared.Application.Exceptions;

namespace ModularMonolith.CategoryManagement.Application.Categories.Shared;

public sealed class CategoryNameConflictException : ConflictException
{
    public CategoryNameConflictException(string message, string reference) : base(message, reference)
    {
    }

    public override string Code => "CATEGORY_WITH_NAME_ALREADY_EXISTS";
}
