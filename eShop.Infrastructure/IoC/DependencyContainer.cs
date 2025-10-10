using eShop.Application.Interfaces.Admin;
using eShop.Application.Interfaces.Customer;
using eShop.Application.Interfaces.Shared;
using eShop.Application.Services.Admin;
using eShop.Application.Services.Customer;
using eShop.Domain.Interfaces.Base;
using eShop.Infrastructure.BackgroundServices;
using eShop.Infrastructure.Repositories.Base;
using eShop.Infrastructure.Services;
using eShop.Infrastructure.Services.Admin;
using Microsoft.Extensions.DependencyInjection;

namespace eShop.Infrastructure.IoC;

public static class DependencyContainer
{
    public static IServiceCollection AddAdminIoCServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthAdminService, AuthAdminService>();

        services.AddScoped<ICategoryAdminService, CategoryAdminService>();
        services.AddScoped<IProductAdminService, ProductAdminService>();
        //services.AddScoped<IUserAdminService, UserAdminService>();
        //services.AddScoped<IOrderAdminService, OrderAdminService>();

        services.AddHttpClient<IOpenAIProductDescriptionGenerator, OpenAIProductDescriptionGenerator>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }

    public static IServiceCollection AddCustomerIoCService(this IServiceCollection services)
    {
        services.AddSingleton<IEmailQueue, InMemoryEmailQueue>();
        services.AddScoped<IEmailService, MailKitEmailService>();
        services.AddHostedService<EmailBackgroundService>();

        services.AddScoped<IAuthCustomerService, AuthCustomerService>();

        services.AddScoped<ICategoryCustomerService, CategoryCustomerService>();
        services.AddScoped<IProductCustomerService, ProductCustomerService>();
        services.AddScoped<IBasketCustomerService, BasketCustomerService>();
        services.AddScoped<IOrderCustomerService, OrderCustomerService>();
        services.AddScoped<ICommentCustomerService, CommentCustomerService>();

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();

        return services;
    }
}
