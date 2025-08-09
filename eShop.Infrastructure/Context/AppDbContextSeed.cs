using eShop.Application.Constants;
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

        var userData = new UserData(
            firstName: "Viktor",
            lastName: "Vasilevski",
            username: "viktorvasilevski",
            email: "viktor@example.com",
            password: "Viktor@123",
            role: Role.Customer);

        var adminUser = User.CreateNew(userData);

        context.Users.Add(adminUser);
        context.SaveChanges();
    }


    public static void SeedAdminUser(AppDbContext context, string password)
    {
        context.Database.Migrate();

        if (context.Users.Any(x => x.Role == Role.Admin))
            return;

        var adminData = new UserData(
            firstName: "Admin",
            lastName: "Admin",
            username: "admin",
            email: "admin@example.com",
            password: password,
            role: Role.Admin);

        var adminUser = User.CreateNew(adminData);

        context.Users.Add(adminUser);
        context.SaveChanges();
    }

    public static void SeedUncategorizedCategory(AppDbContext context)
    {
        context.Database.Migrate();

        if (context.Categories.Any(x => x.Name == SystemConstants.UNCATEGORIZED_CATEGORY_NAME))
            return;

        var uncategorizedCategory = Category.CreateNew(SystemConstants.UNCATEGORIZED_CATEGORY_NAME);

        context.Categories.Add(uncategorizedCategory);
        context.SaveChanges();
    }

    public static void SeedUncategorizedSubcategory(AppDbContext context)
    {
        context.Database.Migrate();

        if (context.Subcategories.Any(x => x.Name == SystemConstants.UNCATEGORIZED_SUBCATEGORY_NAME))
            return;

        var category = context.Categories.Where(x => x.Name == SystemConstants.UNCATEGORIZED_CATEGORY_NAME).First();

        var uncategorizedSubcategory = Subcategory.CreateNew(category.Id, SystemConstants.UNCATEGORIZED_SUBCATEGORY_NAME);

        context.Subcategories.Add(uncategorizedSubcategory);
        context.SaveChanges();
    }
}
