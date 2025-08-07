using eShop.Domain.Entities;
using eShop.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace eShop.Infrastructure.Context;

public static class AppDbContextSeed
{

    public static void SeedTestUser(AppDbContext context)
    {
        context.Database.Migrate();

        if (context.Users.Any(x => x.Role == Role.Customer && x.Username.ToLower() == "viktorvasilevski"))
            return;

        var adminUser = User.CreateNew(
            firstName: "Viktor",
            lastName: "Vasilevski",
            username: "viktorvasilevski",
            email: "viktor@example.com",
            password: "Viktor@123",
            role: Role.Customer);

        context.Users.Add(adminUser);
        context.SaveChanges();
    }


    public static void SeedAdminUser(AppDbContext context, string password)
    {
        context.Database.Migrate();

        if (context.Users.Any(x => x.Role == Role.Admin))
            return;

        var adminUser = User.CreateNew(
            firstName: "Admin",
            lastName: "Admin",
            username: "admin",
            email: "admin@example.com",
            password: password,
            role: Role.Admin);

        context.Users.Add(adminUser);
        context.SaveChanges();
    }

    public static void SeedUncategorizedCategory(AppDbContext context)
    {
        context.Database.Migrate();

        if (context.Categories.Any(x => x.Name == "UNCATEGORIZED"))
            return;

        var uncategorizedCategory = Category.CreateNew("UNCATEGORIZED");

        context.Categories.Add(uncategorizedCategory);
        context.SaveChanges();
    }

    public static void SeedUncategorizedSubcategory(AppDbContext context)
    {
        context.Database.Migrate();

        if (context.Subcategories.Any(x => x.Name == "UNCATEGORIZED"))
            return;

        var category = context.Categories.Where(x => x.Name == "UNCATEGORIZED").First();

        var uncategorizedSubcategory = Subcategory.CreateNew(category.Id, "UNCATEGORIZED");

        context.Subcategories.Add(uncategorizedSubcategory);
        context.SaveChanges();
    }
}
