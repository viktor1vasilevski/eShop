﻿using eShop.Domain.Models;
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

        builder.HasIndex(x => x.Name);

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

        builder.HasOne(c => c.ParentCategory)
               .WithMany(c => c.Children)
               .HasForeignKey(c => c.ParentCategoryId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.Property(b => b.Created).IsRequired();
        builder.Property(b => b.CreatedBy).HasMaxLength(256);
    }
}
