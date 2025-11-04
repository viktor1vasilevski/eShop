using eShop.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eShop.Infrastructure.Configurations;

public class CommentConfiguration : IEntityTypeConfiguration<Comment>
{
    public void Configure(EntityTypeBuilder<Comment> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.CommentText)
               .HasMaxLength(2500)
               .IsRequired();

        builder.Property(c => c.Rating)
               .IsRequired();

        builder.Property(c => c.ProductId)
               .IsRequired();

        builder.Property(c => c.UserId)
               .IsRequired();

        builder.HasOne(c => c.Product)
               .WithMany(p => p.Comments)
               .HasForeignKey(c => c.ProductId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.User)
               .WithMany(u => u.Comments)
               .HasForeignKey(c => c.UserId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.Property(c => c.Created).IsRequired();
        builder.Property(c => c.CreatedBy).HasMaxLength(256);
    }
}
