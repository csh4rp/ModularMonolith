﻿using ModularMonolith.Shared.Contracts;

namespace ModularMonolith.Modules.FirstModule.Contracts.Categories.Commands;

public class CreateCategoryCommand : ICommand<Guid>
{
    public required Guid? ParentId { get; init; }
    
    public required string Name { get; init; }
}