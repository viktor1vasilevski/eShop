using eShop.Application.Interfaces;
using eShop.Application.Interfaces.Category;
using eShop.Application.Services;
using eShop.Domain.Interfaces;
using eShop.Infrastructure.Repositories;
using eShop.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace eShop.Infrastructure.IoC;

public static class DependencyContainer
{
    public static IServiceCollection AddIoCService(this IServiceCollection services)
    {
        // Application.Services
        services.AddScoped<IAuthService, AdminAuthService>();

        services.AddScoped<ICategoryAdminService, CategoryService>();
        services.AddScoped<ICategoryCustomerService, CategoryService>();

        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IBasketService, BasketService>();
        services.AddScoped<ICustomerAuthService, CustomerAuthService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<ICommentService, CommentService>();
        services.AddScoped<IUserService, UserService>();

        // Infrastructure.Services
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddHttpClient<IProductDescriptionGenerator, OpenAiProductDescriptionGenerator>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();

        return services;
    }
}
