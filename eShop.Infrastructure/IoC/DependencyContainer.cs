using eShop.Application.Interfaces.Shared;
using eShop.Application.Services.Admin;
using eShop.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace eShop.Infrastructure.IoC;

public static class DependencyContainer
{
    public static IServiceCollection AddAdminIoCServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AdminAuthService>();

        //services.AddScoped<ICategoryAdminService, CategoryAdminService>();
        //services.AddScoped<IProductAdminService, ProductAdminService>();
        //services.AddScoped<IUserAdminService, UserAdminService>();
        //services.AddScoped<IOrderAdminService, OrderAdminService>();

        //services.AddScoped<IUnitOfWork, UnitOfWork>();
        //services.AddHttpClient<IOpenAIProductDescriptionGenerator, OpenAIProductDescriptionGenerator>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();

        return services;
    }

    public static IServiceCollection AddCustomerIoCService(this IServiceCollection services)
    {
        //services.AddSingleton<IEmailQueue, InMemoryEmailQueue>();
        //services.AddScoped<IEmailService, MailKitEmailService>();
        //services.AddHostedService<EmailBackgroundService>();

        //services.AddScoped<ICustomerAuthService, CustomerAuthService>();

        //services.AddScoped<ICategoryCustomerService, CategoryCustomerService>();
        //services.AddScoped<IProductCustomerService, ProductCustomerService>();
        //services.AddScoped<IBasketCustomerService, BasketCustomerService>();
        //services.AddScoped<IOrderCustomerService, OrderCustomerService>();
        //services.AddScoped<ICommentCustomerService, CommentCustomerService>();
        //services.AddScoped<IOrderCustomerService, OrderCustomerService>();

        //services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();

        return services;
    }
}
