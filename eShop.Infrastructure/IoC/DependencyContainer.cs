using eShop.Application.Interfaces;
using eShop.Application.Interfaces.Admin;
using eShop.Application.Interfaces.Customer;
using eShop.Application.Services;
using eShop.Application.Services.Admin;
using eShop.Application.Services.Customer;
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

        services.AddScoped<ICategoryAdminService, CategoryAdminService>();
        services.AddScoped<IProductAdminService, ProductAdminService>();

        services.AddScoped<ICategoryCustomerService, CategoryCustomerService>();
        services.AddScoped<IProductCustomerService, ProductCustomerService>();
        services.AddScoped<IBasketCustomerService, BasketCustomerService>();

        services.AddScoped<IBasketService, BasketService>();


        services.AddScoped<ICustomerAuthService, CustomerAuthService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<ICommentService, CommentService>();
        services.AddScoped<IUserService, UserService>();

        // Infrastructure.Services
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddHttpClient<IOpenAIProductDescriptionGenerator, OpenAIProductDescriptionGenerator>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();

        return services;
    }
}
