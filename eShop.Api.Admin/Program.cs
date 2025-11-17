using eShop.Api.Admin.Extensions;
using eShop.Api.Admin.Middlewares;
using eShop.Application.Interfaces.Shared;
using eShop.Application.Validations.Admin.Category;
using eShop.Domain.Interfaces.EntityFramework;
using eShop.Infrastructure.Context;
using eShop.Infrastructure.IoC;
using eShop.Infrastructure.Repositories.EntityFramework;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddOpenApi();
builder.Services.AddHttpContextAccessor();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerWithAuth();

builder.Services.AddCors(policy => policy.AddPolicy("MyPolicy", builder =>
{
    builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
}));

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddJwtAuthentication(builder.Configuration);

builder.Services.AddScoped<IEfUnitOfWork, EfUnitOfWork>();
builder.Services.AddScoped(typeof(IEfRepository<>), typeof(EfRepository<>));
builder.Services.AddAdminIoCServices();

builder.Services.AddValidatorsFromAssemblyContaining<CreateCategoryAdminRequestValidator>(ServiceLifetime.Transient);
builder.Services.AddFluentValidationAutoValidation();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var _passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordService>();

    try
    {
        var password = builder.Configuration["SeedAdmin:Password"] ?? "Admin@123";
        AppDbContextSeed.SeedAdminUser(dbContext, password, _passwordHasher);
        AppDbContextSeed.SeedTestUser(dbContext, _passwordHasher);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error during data seeding");
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "eShop.Api.Admin");
    });
}

app.UseHttpsRedirection();

app.UseCors("MyPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
