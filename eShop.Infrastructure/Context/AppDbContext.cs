using eShop.Domain.Models;
using eShop.Domain.Models.Base;
using eShop.Infrastructure.Configurations;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace eShop.Infrastructure.Context;

public class AppDbContext(DbContextOptions<AppDbContext> options, IHttpContextAccessor _httpContextAccessor) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var username = _httpContextAccessor?.HttpContext?.User?.Identity?.Name ?? "System";

        var addedEntries = ChangeTracker
            .Entries()
            .Where(e => e.Entity is AuditableBaseEntity && e.State == EntityState.Added);

        foreach (var entry in addedEntries)
        {
            var entity = (AuditableBaseEntity)entry.Entity;
            entity.Created = DateTime.Now;
            entity.CreatedBy = username;
        }

        var modifiedEntries = ChangeTracker
            .Entries()
            .Where(e => e.Entity is AuditableBaseEntity && e.State == EntityState.Modified);

        foreach (var entry in modifiedEntries)
        {
            var entity = (AuditableBaseEntity)entry.Entity;

            entry.Property(nameof(entity.Created)).IsModified = false;
            entry.Property(nameof(entity.CreatedBy)).IsModified = false;

            entity.LastModified = DateTime.Now;
            entity.LastModifiedBy = username;
        }

        return await base.SaveChangesAsync(cancellationToken);
    }


    public override int SaveChanges() => SaveChangesAsync().GetAwaiter().GetResult();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(UserConfiguration).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
