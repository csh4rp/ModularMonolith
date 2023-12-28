﻿using Microsoft.EntityFrameworkCore;
using ModularMonolith.CategoryManagement.Application.Categories.Abstract;
using ModularMonolith.CategoryManagement.Contracts.Categories.Commands;
using ModularMonolith.CategoryManagement.Domain.Entities;
using ModularMonolith.Shared.Application.Commands;
using ModularMonolith.Shared.Application.Exceptions;
using ModularMonolith.Shared.Contracts;
using ModularMonolith.Shared.Contracts.Errors;

namespace ModularMonolith.CategoryManagement.Application.Categories.CommandHandlers;

internal sealed class CreateCategoryCommandHandler : ICommandHandler<CreateCategoryCommand, CreatedResponse>
{
    private readonly ICategoryDatabase _categoryDatabase;

    public CreateCategoryCommandHandler(ICategoryDatabase categoryDatabase) => _categoryDatabase = categoryDatabase;

    public async Task<CreatedResponse> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var categoryWithNameExists = await _categoryDatabase.Categories
            .AnyAsync(c => c.Name == request.Name, cancellationToken);

        if (categoryWithNameExists)
        {
            throw new ConflictException(nameof(request.Name), ErrorCodes.NotUnique, "Category name must be unique.");
        }

        if (request.ParentId.HasValue)
        {
            var parentExists = await _categoryDatabase.Categories
                .AnyAsync(c => c.Id == request.ParentId, cancellationToken);

            if (!parentExists)
            {
                throw new ValidationException(
                    PropertyError.InvalidArgument(nameof(CreateCategoryCommand.Name), request.ParentId));
            }
        }

        var category = new Category { ParentId = request.ParentId, Name = request.Name };

        _categoryDatabase.Categories.Add(category);

        await _categoryDatabase.SaveChangesAsync(cancellationToken);

        return new CreatedResponse(category.Id);
    }
}
