using eShop.AdminAPI.Extension;
using eShop.AdminAPI.Middlewares;
using eShop.Application.Validations.Category;
using eShop.Infrastructure.Context;
using eShop.Infrastructure.IoC;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
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

builder.Services.AddIoCService();

// Fluent Validations
builder.Services.AddValidatorsFromAssemblyContaining<CreateUpdateCategoryRequestValidator>(ServiceLifetime.Transient);
builder.Services.AddFluentValidationAutoValidation();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    try
    {
        var password = builder.Configuration["SeedAdmin:Password"];
        AppDbContextSeed.SeedAdminUser(dbContext, password);

        AppDbContextSeed.SeedUncategorizedCategory(dbContext);
        AppDbContextSeed.SeedUncategorizedSubcategory(dbContext);
        AppDbContextSeed.SeedTestUser(dbContext);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error during data seeding");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "eShop.Admin API");
        c.DocumentTitle = "eShop.Admin API Docs";
    });
}

app.UseHttpsRedirection();

app.UseExceptionHandler();

app.UseCors("MyPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
