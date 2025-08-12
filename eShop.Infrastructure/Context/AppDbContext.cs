using eShop.Domain.Entities;
using eShop.Domain.Entities.Base;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace eShop.Infrastructure.Context;

public class AppDbContext(DbContextOptions<AppDbContext> options, IHttpContextAccessor _httpContextAccessor) : DbContext(options)
{
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Subcategory> Subcategories => Set<Subcategory>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Basket> Baskets => Set<Basket>();
    public DbSet<BasketItem> BasketItems => Set<BasketItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Comment> Comments => Set<Comment>();

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker
            .Entries()
            .Where(e => e.Entity is AuditableBaseEntity &&
                       (e.State == EntityState.Added || e.State == EntityState.Modified));

        var username = _httpContextAccessor?.HttpContext?.User?.Identity?.Name ?? "System";

        foreach (var entry in entries)
        {
            var entity = (AuditableBaseEntity)entry.Entity;

            if (entry.State == EntityState.Added)
            {
                entity.Created = DateTime.Now;
                entity.CreatedBy = username;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Property("Created").IsModified = false;
                entry.Property("CreatedBy").IsModified = false;

                entity.LastModified = DateTime.Now;
                entity.LastModifiedBy = username;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges() =>
      SaveChangesAsync().GetAwaiter().GetResult();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>()
            .HasMany(x => x.Subcategories)
            .WithOne(x => x.Category)
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Product>()
            .HasOne(x => x.Subcategory)
            .WithMany(x => x.Products)
            .HasForeignKey(u => u.SubcategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<User>()
            .HasOne(u => u.Basket)
            .WithOne(b => b.User)
            .HasForeignKey<Basket>(b => b.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Basket>()
            .HasMany(b => b.Items)
            .WithOne(i => i.Basket)
            .HasForeignKey(i => i.BasketId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<BasketItem>()
            .HasOne(i => i.Product)
            .WithMany(p => p.BasketItems)
            .HasForeignKey(i => i.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Order>()
            .HasOne(o => o.User)
            .WithMany(u => u.Orders)
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<OrderItem>()
            .HasOne(oi => oi.Order)
            .WithMany(o => o.Items)
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<OrderItem>()
            .HasOne(oi => oi.Product)
            .WithMany(p => p.OrderItems)
            .HasForeignKey(oi => oi.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Comment>()
            .HasOne(c => c.Product)
            .WithMany(o => o.Comments)
            .HasForeignKey(c => c.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Comment>()
            .HasOne(c => c.User)
            .WithMany(u => u.Comments)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        base.OnModelCreating(modelBuilder);
    }
}
