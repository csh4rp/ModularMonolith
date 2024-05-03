using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ModularMonolith.Identity.Domain.Roles;
using ModularMonolith.Shared.DataAccess.EntityFramework.AuditLogs;

namespace ModularMonolith.Identity.Infrastructure.Account.EntityConfigurations;

internal sealed class RoleEntityTypeConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("role", "identity");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.Name)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(b => b.NormalizedName)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(b => b.ConcurrencyStamp)
            .IsRequired()
            .HasMaxLength(64)
            .AuditIgnore();

        builder.HasIndex(b => b.NormalizedName).IsUnique();
    }
}
