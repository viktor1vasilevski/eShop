using Domain.Interfaces;
using eShop.Infrastructure.Data.Context;
using Infrastructure.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Infrastructure.IoC;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddCors(policy => policy.AddPolicy("MyPolicy", builder =>
{
    builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
}));

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped(typeof(IUnitOfWork<>), typeof(SqlUnitOfWork<>));
builder.Services.AddIoCService();

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseCors("MyPolicy");

app.UseAuthorization();

app.MapControllers();

app.Run();
