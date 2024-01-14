﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ModularMonolith.Identity.Domain.Common.Entities;

namespace ModularMonolith.Identity.Infrastructure.Common.EntityConfigurations;

internal sealed class UserRoleEntityTypeConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.ToTable("user_role", "identity");

        builder.HasKey(b => new { b.UserId, b.RoleId });

        builder.HasOne(b => b.User)
            .WithMany()
            .HasForeignKey(b => b.UserId);

        builder.HasOne(b => b.Role)
            .WithMany()
            .HasForeignKey(b => b.RoleId);
    }
}
