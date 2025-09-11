using eShop.Application.Constants;
using eShop.Application.Interfaces;
using eShop.Domain.Entities;
using eShop.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace eShop.Infrastructure.Context;

public static class AppDbContextSeed
{

    public static void SeedTestUser(AppDbContext context, IPasswordHasher _passwordHasher)
    {
        context.Database.Migrate();

        if (context.Users.Any(x => x.Role == Role.Customer && x.Username.ToLower() == "testuser"))
            return;

        var passwordHash = _passwordHasher.HashPassword("Test@123", out string salt);

        var userData = new UserData(
            firstName: "Test",
            lastName: "Test",
            username: "testuser",
            email: "test@example.com",
            passwordHash: passwordHash,
            salt: salt, 
            role: Role.Customer);

        var user = User.CreateNew(userData);

        context.Users.Add(user);
        context.SaveChanges();
    }


    public static void SeedAdminUser(AppDbContext context, string password, IPasswordHasher _passwordHasher)
    {
        context.Database.Migrate();

        if (context.Users.Any(x => x.Role == Role.Admin))
            return;

        var passwordHash = _passwordHasher.HashPassword(password, out string salt);

        var adminData = new UserData(
            firstName: "Admin",
            lastName: "Admin",
            username: "admin",
            email: "admin@example.com",
            passwordHash: passwordHash,
            salt: salt,
            role: Role.Admin);

        var adminUser = User.CreateNew(adminData);

        context.Users.Add(adminUser);
        context.SaveChanges();
    }

    public static void SeedUncategorizedCategory(AppDbContext context)
    {
        context.Database.Migrate();

        if (context.Categories.Any(x => x.Name == SystemConstants.UncategorizedCategoryName))
            return;

        var uncategorizedCategory = Category.Create(SystemConstants.UncategorizedCategoryName, null);

        context.Categories.Add(uncategorizedCategory);
        context.SaveChanges();
    }
}
