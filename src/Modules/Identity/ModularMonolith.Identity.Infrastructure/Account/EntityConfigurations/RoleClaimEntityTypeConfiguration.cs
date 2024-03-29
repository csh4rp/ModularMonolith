﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ModularMonolith.Identity.Domain.Roles;

namespace ModularMonolith.Identity.Infrastructure.Account.EntityConfigurations;

internal sealed class RoleClaimEntityTypeConfiguration : IEntityTypeConfiguration<RoleClaim>
{
    public void Configure(EntityTypeBuilder<RoleClaim> builder)
    {
        builder.ToTable("role_claim", "identity");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.ClaimType)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(b => b.ClaimValue)
            .IsRequired()
            .HasMaxLength(128);

        builder.HasOne<Role>()
            .WithMany()
            .HasForeignKey(b => b.RoleId);
    }
}
