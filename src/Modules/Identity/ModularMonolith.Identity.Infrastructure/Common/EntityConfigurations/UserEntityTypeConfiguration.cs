using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ModularMonolith.Identity.Domain.Common.Entities;

namespace ModularMonolith.Identity.Infrastructure.Common.EntityConfigurations;

internal sealed class UserEntityTypeConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("user", "identity");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.UserName)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(b => b.NormalizedUserName)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(b => b.Email)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(b => b.NormalizedEmail)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(b => b.ConcurrencyStamp)
            .IsRequired()
            .HasMaxLength(64)
            .IsConcurrencyToken();

        builder.Property(b => b.SecurityStamp)
            .IsRequired()
            .HasMaxLength(64)
            .IsConcurrencyToken();


        builder.Property(b => b.PhoneNumber)
            .HasMaxLength(64);

        builder.Property(b => b.PasswordHash)
            .HasMaxLength(256);

        builder.HasIndex(b => b.NormalizedEmail).IsUnique();

        builder.HasIndex(b => b.NormalizedUserName).IsUnique();
    }
}
