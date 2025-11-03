using eShop.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eShop.Infrastructure.Configurations;

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.HasKey(i => i.Id);

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

        builder.Property(i => i.OrderId)
               .IsRequired();

        builder.Property(i => i.ProductId)
               .IsRequired();

        builder.HasOne(i => i.Order)
               .WithMany(o => o.OrderItems)
               .HasForeignKey(i => i.OrderId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(i => i.Product)
               .WithMany(p => p.OrderItems)
               .HasForeignKey(i => i.ProductId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.Property(i => i.Created).IsRequired();
        builder.Property(i => i.CreatedBy).HasMaxLength(256);
    }
}
