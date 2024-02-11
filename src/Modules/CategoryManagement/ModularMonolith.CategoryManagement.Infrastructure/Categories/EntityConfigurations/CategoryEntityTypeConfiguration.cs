using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ModularMonolith.CategoryManagement.Domain.Categories;

namespace ModularMonolith.CategoryManagement.Infrastructure.Categories.EntityConfigurations;

internal sealed class CategoryEntityTypeConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("category", "category_management");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.Id)
            .HasConversion(b => b.Value, b => new CategoryId(b));

        builder.Property(b => b.ParentId)
            .HasConversion(b => b == null ? (Guid?)null : b.Value.Value, b => b != null ?
                new CategoryId(b.Value) : null);

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
