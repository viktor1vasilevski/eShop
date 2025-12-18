using eShop.Application.Interfaces.Admin;
using eShop.Application.Interfaces.Customer;
using eShop.Application.Interfaces.Shared;
using eShop.Application.Services.Admin;
using eShop.Application.Services.Customer;
using eShop.Domain.Interfaces.Dapper;
using eShop.Infrastructure.BackgroundServices;
using eShop.Infrastructure.Repositories.Dapper;
using eShop.Infrastructure.Services;
using eShop.Infrastructure.Services.Admin;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Data;

namespace eShop.Infrastructure.IoC;

public static class DependencyContainer
{
    public static IServiceCollection AddAdminIoCServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthAdminService, AuthAdminService>();
        services.AddScoped<ICategoryAdminService, CategoryAdminService>();
        services.AddScoped<IProductAdminService, ProductAdminService>();

        services.AddHttpClient<IOpenAIProductDescriptionGenerator, OpenAIProductDescriptionGenerator>(client =>
        {
            client.BaseAddress = new Uri("https://api.openai.com/");
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        services.AddScoped<IPasswordService, PasswordHasher>();

        return services;
    }

    public static IServiceCollection AddCustomerIoCService(this IServiceCollection services, IConfiguration configuration)
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

        services.AddScoped<IDbConnection>(sp =>
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Missing connection string 'DefaultConnection'.");

            var connection = new SqlConnection(connectionString);
            connection.Open();
            return connection;
        });

        services.AddScoped(typeof(IDapperRepository<>), typeof(DapperRepository<>));
        services.AddScoped<IDapperUnitOfWork, DapperUnitOfWork>();

        services.AddScoped<IPasswordService, PasswordHasher>();

        return services;
    }
}
