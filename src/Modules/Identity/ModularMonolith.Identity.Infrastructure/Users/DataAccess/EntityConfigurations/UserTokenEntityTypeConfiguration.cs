using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ModularMonolith.Identity.Domain.Users.Entities;

namespace ModularMonolith.Identity.Infrastructure.Users.DataAccess.EntityConfigurations;

internal sealed class UserTokenEntityTypeConfiguration : IEntityTypeConfiguration<UserToken>
{
    public void Configure(EntityTypeBuilder<UserToken> builder)
    {
        builder.ToTable("user_token");
        
        builder.HasKey(b => new {b.UserId, b.LoginProvider, b.Name});
        
        builder.Property(b => b.LoginProvider)
            .IsRequired()
            .HasMaxLength(128);
        
        builder.Property(b => b.Name)
            .IsRequired()
            .HasMaxLength(128);
        
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(b => b.UserId);
    }
}
