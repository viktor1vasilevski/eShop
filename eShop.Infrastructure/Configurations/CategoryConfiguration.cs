using eShop.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eShop.Infrastructure.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
               .HasMaxLength(200)
               .IsRequired();

        builder.HasIndex(x => x.Name).IsUnique();

        builder.OwnsOne(x => x.Image, img =>
        {
            img.Property(i => i.Bytes)
               .HasColumnName("Image")
               .HasColumnType("varbinary(max)")
               .IsRequired(false);

            img.Property(i => i.Type)
               .HasColumnName("ImageType")
               .HasMaxLength(32)
               .IsRequired(false);
        });

        builder.Navigation(x => x.Image);

        // Recursive relationship
        builder.HasOne(c => c.ParentCategory)
               .WithMany(c => c.Children)
               .HasForeignKey(c => c.ParentCategoryId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
