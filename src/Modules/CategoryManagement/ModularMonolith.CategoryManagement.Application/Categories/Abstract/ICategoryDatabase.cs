﻿using Microsoft.EntityFrameworkCore;
using ModularMonolith.CategoryManagement.Domain.Entities;

namespace ModularMonolith.CategoryManagement.Application.Categories.Abstract;

public interface ICategoryDatabase
{
    DbSet<Category> Categories { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}