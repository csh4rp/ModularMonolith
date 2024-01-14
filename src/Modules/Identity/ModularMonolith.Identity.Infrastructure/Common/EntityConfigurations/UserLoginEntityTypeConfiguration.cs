using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ModularMonolith.Identity.Domain.Common.Entities;

namespace ModularMonolith.Identity.Infrastructure.Common.EntityConfigurations;

internal sealed class UserLoginEntityTypeConfiguration : IEntityTypeConfiguration<UserLogin>
{
    public void Configure(EntityTypeBuilder<UserLogin> builder)
    {
        builder.ToTable("user_login", "identity");

        builder.HasKey(b => new { b.UserId, b.LoginProvider, b.ProviderKey });

        builder.Property(b => b.LoginProvider)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(b => b.ProviderKey)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(b => b.ProviderDisplayName)
            .HasMaxLength(128);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(b => b.UserId);
    }
}
