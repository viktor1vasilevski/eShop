using eShop.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eShop.Infrastructure.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(x => x.Id);

        builder.OwnsOne(x => x.Name, n =>
        {
            n.Property(p => p.Value)
             .HasColumnName("Name")
             .HasMaxLength(50)
             .IsRequired();
        });

        builder.OwnsOne(x => x.Description, desc =>
        {
            desc.Property(d => d.Value)
                .HasColumnName("Description")
                .HasMaxLength(2500)
                .IsRequired();
        });

        builder.OwnsOne(x => x.UnitPrice, price =>
        {
            price.Property(p => p.Value)
                 .HasColumnName("UnitPrice")
                 .HasColumnType("decimal(18,2)")
                 .IsRequired();
        });

        builder.OwnsOne(x => x.UnitQuantity, qty =>
        {
            qty.Property(q => q.Value)
               .HasColumnName("UnitQuantity")
               .IsRequired();
        });

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

        builder.HasOne(p => p.Category)
               .WithMany(c => c.Products)
               .HasForeignKey(p => p.CategoryId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(p => p.BasketItems)
               .WithOne(i => i.Product)
               .HasForeignKey(i => i.ProductId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.OrderItems)
               .WithOne(x => x.Product)
               .HasForeignKey(x => x.ProductId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.Property(b => b.Created).IsRequired();
        builder.Property(b => b.CreatedBy).HasMaxLength(256);
    }
}
