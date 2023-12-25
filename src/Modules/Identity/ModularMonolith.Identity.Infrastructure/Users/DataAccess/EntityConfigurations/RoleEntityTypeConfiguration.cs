using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ModularMonolith.Identity.Domain.Users.Entities;

namespace ModularMonolith.Identity.Infrastructure.Users.DataAccess.EntityConfigurations;

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

        builder.HasIndex(b => b.NormalizedName).IsUnique();
    }
}
