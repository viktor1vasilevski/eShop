using eShop.Application.Interfaces;
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
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<IAuthService, AdminAuthService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<ISubcategoryService, SubcategoryService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IBasketService, BasketService>();
        services.AddScoped<ICustomerAuthService, CustomerAuthService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<ICommentService, CommentService>();
        services.AddScoped<IUserService, UserService>();

        // AI Product Description Generator
        services.AddHttpClient<IProductDescriptionGenerator, OpenAiProductDescriptionGenerator>();

        return services;
    }
}
