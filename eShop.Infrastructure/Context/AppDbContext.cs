using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace eShop.Infrastructure.Context;

public class AppDbContext(DbContextOptions<AppDbContext> options, IHttpContextAccessor _httpContextAccessor) : DbContext(options)
{


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        //modelBuilder.ApplyConfigurationsFromAssembly(typeof(UserConfiguration).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
