using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ModularMonolith.Identity.Domain.Common.Entities;

namespace ModularMonolith.Identity.Infrastructure.Common.EntityConfigurations;

internal sealed class RoleEntityTypeConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("role");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.Name)
            .IsRequired()
            .HasMaxLength(128);
        
        builder.Property(b => b.NormalizedName)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(b => b.ConcurrencyStamp)
            .IsRequired()
            .HasMaxLength(64);
        
        builder.HasIndex(b => b.NormalizedName).IsUnique();
    }
}
