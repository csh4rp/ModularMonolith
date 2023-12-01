﻿using Microsoft.EntityFrameworkCore;
using ModularMonolith.Modules.FirstModule.Domain.Entities;

namespace ModularMonolith.Modules.FirstModule.BusinessLogic.Abstract;

public interface ICategoryDatabase
{
    DbSet<Category> Categories { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
