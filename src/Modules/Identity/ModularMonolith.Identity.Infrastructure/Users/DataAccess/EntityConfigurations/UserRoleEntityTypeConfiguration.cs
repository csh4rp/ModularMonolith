using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ModularMonolith.Identity.Domain.Users.Entities;

namespace ModularMonolith.Identity.Infrastructure.Users.DataAccess.EntityConfigurations;

internal sealed class UserRoleEntityTypeConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.ToTable("user_role");
        
        builder.HasKey(b => new {b.UserId, b.RoleId});
        
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(b => b.UserId);
        
        builder.HasOne<Role>()
            .WithMany()
            .HasForeignKey(b => b.RoleId);
    }
}
