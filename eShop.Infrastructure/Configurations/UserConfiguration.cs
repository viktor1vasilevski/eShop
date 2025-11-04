using eShop.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eShop.Infrastructure.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);

        builder.OwnsOne(u => u.FullName, fullName =>
        {
            fullName.Property(fn => fn.FirstName)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("FirstName");

            fullName.Property(fn => fn.LastName)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("LastName");
        });

        builder.OwnsOne(u => u.Username, uName =>
        {
            uName.Property(u => u.Value)
                 .HasColumnName("Username")
                 .HasMaxLength(50)
                 .IsRequired();

            uName.HasIndex(u => u.Value).IsUnique();
        });

        builder.OwnsOne(u => u.Email, e =>
        {
            e.Property(em => em.Value)
             .HasColumnName("Email")
             .HasMaxLength(150)
             .IsRequired();

            e.HasIndex(em => em.Value).IsUnique();
        });

        builder.Property(u => u.PasswordHash)
               .IsRequired();

        builder.Property(u => u.SaltKey)
               .IsRequired();

        builder.Property(u => u.Role)
               .IsRequired();

        builder.Property(u => u.IsDeleted)
               .IsRequired();

        builder.HasOne(u => u.Basket)
               .WithOne(b => b.User)
               .HasForeignKey<Basket>(b => b.UserId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.Orders)
               .WithOne(o => o.User)
               .HasForeignKey(o => o.UserId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.Comments)
               .WithOne(c => c.User)
               .HasForeignKey(c => c.UserId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.Property(u => u.Created).IsRequired();
        builder.Property(u => u.CreatedBy).HasMaxLength(256);
    }
}
