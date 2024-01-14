using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ModularMonolith.Identity.Domain.Common.Entities;

namespace ModularMonolith.Identity.Infrastructure.Common.EntityConfigurations;

internal sealed class UserTokenEntityTypeConfiguration : IEntityTypeConfiguration<UserToken>
{
    public void Configure(EntityTypeBuilder<UserToken> builder)
    {
        builder.ToTable("user_token", "identity");

        builder.HasKey(b => new { b.UserId, b.LoginProvider, b.Name });

        builder.Property(b => b.LoginProvider)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(b => b.Name)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(b => b.Value)
            .IsRequired()
            .HasMaxLength(1024);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(b => b.UserId);
    }
}
