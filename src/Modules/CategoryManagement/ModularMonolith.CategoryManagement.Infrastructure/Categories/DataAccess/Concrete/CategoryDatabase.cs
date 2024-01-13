﻿using Microsoft.EntityFrameworkCore;
using ModularMonolith.CategoryManagement.Application.Categories.Abstract;
using ModularMonolith.CategoryManagement.Domain.Entities;

namespace ModularMonolith.CategoryManagement.Infrastructure.Categories.DataAccess.Concrete;

internal sealed class CategoryDatabase : ICategoryDatabase
{
    private readonly DbContext _dbContext;

    public CategoryDatabase(DbContext dbContext) => _dbContext = dbContext;

    public DbSet<Category> Categories => _dbContext.Set<Category>();

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken) =>
        _dbContext.SaveChangesAsync(cancellationToken);
}
