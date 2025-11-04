using eShop.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eShop.Infrastructure.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(x => x.Id);

        builder.OwnsOne(x => x.Name, pn =>
        {
            pn.Property(p => p.Value)
              .HasColumnName("Name")
              .HasMaxLength(200)
              .IsRequired();
        });

        builder.OwnsOne(x => x.Description, pd =>
        {
            pd.Property(p => p.Value)
              .HasColumnName("Description")
              .HasMaxLength(2500)
              .IsRequired();
        });

        builder.OwnsOne(x => x.UnitPrice, up =>
        {
            up.Property(v => v.Value)
              .HasColumnName("UnitPrice")
              .HasColumnType("decimal(18,2)")
              .IsRequired();
        });

        builder.OwnsOne(x => x.UnitQuantity, uq =>
        {
            uq.Property(v => v.Value)
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
