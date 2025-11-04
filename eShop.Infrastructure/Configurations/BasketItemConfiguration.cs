using eShop.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eShop.Infrastructure.Configurations;

public class BasketItemConfiguration : IEntityTypeConfiguration<BasketItem>
{
    public void Configure(EntityTypeBuilder<BasketItem> builder)
    {
        builder.HasKey(i => i.Id);

        builder.Property(i => i.Quantity)
               .IsRequired();

        builder.HasOne(i => i.Product)
               .WithMany(p => p.BasketItems)
               .HasForeignKey(i => i.ProductId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.Basket)
               .WithMany(b => b.BasketItems)
               .HasForeignKey(i => i.BasketId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.Property(i => i.Created).IsRequired();
        builder.Property(i => i.CreatedBy).HasMaxLength(256);
    }
}
