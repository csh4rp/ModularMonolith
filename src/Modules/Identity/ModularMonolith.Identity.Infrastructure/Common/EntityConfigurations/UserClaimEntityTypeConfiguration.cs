using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ModularMonolith.Identity.Domain.Common.Entities;

namespace ModularMonolith.Identity.Infrastructure.Common.EntityConfigurations;

internal sealed class UserClaimEntityTypeConfiguration : IEntityTypeConfiguration<UserClaim>
{
    public void Configure(EntityTypeBuilder<UserClaim> builder)
    {
        builder.ToTable("user_claim", "identity");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.ClaimType)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(b => b.ClaimValue)
            .IsRequired()
            .HasMaxLength(128);

        builder.HasOne(b => b.User)
            .WithMany()
            .HasForeignKey(b => b.UserId);
    }
}
