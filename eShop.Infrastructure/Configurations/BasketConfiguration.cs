using eShop.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eShop.Infrastructure.Configurations;

public class BasketConfiguration : IEntityTypeConfiguration<Basket>
{
    public void Configure(EntityTypeBuilder<Basket> builder)
    {
        builder.HasKey(b => b.Id);

        builder.HasOne(b => b.User)
               .WithOne(u => u.Basket)
               .HasForeignKey<Basket>(b => b.UserId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(b => b.BasketItems)
               .WithOne(i => i.Basket)
               .HasForeignKey(i => i.BasketId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.Property(b => b.Created).IsRequired();
        builder.Property(b => b.CreatedBy).HasMaxLength(256);
    }
}
