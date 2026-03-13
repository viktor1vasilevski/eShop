# eShop

A Clean Architecture .NET solution with two APIs (Admin and Customer) and .NET Aspire orchestration.

## Project Structure

```
src/
├── eShop.Api.Admin/        # Admin-facing API
├── eShop.Api.Customer/     # Customer-facing API
├── eShop.AppHost/          # .NET Aspire orchestration
├── eShop.Application/      # Use cases, services, DTOs, validators
├── eShop.Domain/           # Entities, value objects, domain logic
└── eShop.Infrastructure/   # EF Core, external services, repositories

tests/
├── eShop.Application.Tests/
├── eShop.Domain.Tests/
└── eShop.Infrastructure.Tests/
```

## Commands

- **Build:** `dotnet build`
- **Test:** `dotnet test`

## Architecture & Conventions

- Clean Architecture: Domain → Application → Infrastructure → API
- Domain layer has no external dependencies
- Use Result pattern for error handling (avoid exceptions for flow control)
- FluentValidation for all input validation
- Repository pattern in Infrastructure
- Async/await throughout — always propagate CancellationToken
- Use IReadOnlyList<T> for read-only collections in return types

## Coding Standards

- Follow SOLID principles
- XML doc comments on public interfaces
- No business logic in API controllers — delegate to Application layer
- Global exception handler in API projects