using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ModularMonolith.CategoryManagement.Domain.Entities;

namespace ModularMonolith.CategoryManagement.Infrastructure.Categories.DataAccess.EntityConfigurations;

internal sealed class CategoryEntityTypeConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("category");

        builder.HasKey(b => b.Id);

        builder.HasMany<Category>()
            .WithOne()
            .HasForeignKey(b => b.ParentId);

        builder.Property(b => b.Name)
            .IsRequired()
            .HasMaxLength(128);

        builder.HasIndex(b => b.Name)
            .IsUnique();
    }
}
