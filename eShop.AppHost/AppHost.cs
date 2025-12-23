var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.eShop_Api_Admin>("eshop-api-admin");

builder.AddProject<Projects.eShop_Api_Customer>("eshop-api-customer");

builder.Build().Run();
