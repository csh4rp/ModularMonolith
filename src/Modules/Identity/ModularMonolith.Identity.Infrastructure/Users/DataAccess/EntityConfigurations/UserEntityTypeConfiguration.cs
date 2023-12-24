using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ModularMonolith.Identity.Domain.Users.Entities;

namespace ModularMonolith.Identity.Infrastructure.Users.DataAccess.EntityConfigurations;

internal sealed class UserEntityTypeConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("user");

        builder.HasKey(x => x.Id);

        builder.Property(b => b.Email)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(b => b.NormalizedEmail)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(b => b.UserName)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(b => b.NormalizedUserName)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(b => b.PasswordHash)
            .HasMaxLength(512);

        builder.HasIndex(b => b.NormalizedEmail);
    }
}
