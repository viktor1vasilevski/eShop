# eShop

**Learning Project** — This project is built for learning purposes. Any developer is welcome to clone it, explore the code, experiment, and build on top of it.

A full-featured e-commerce backend built with **.NET 10** and **Clean Architecture**. The solution exposes two separate REST APIs — one for admin management and one for customer-facing operations — orchestrated with **.NET Aspire**.

---

## Table of Contents

- [Architecture Overview](#architecture-overview)
- [Project Structure](#project-structure)
- [Tech Stack](#tech-stack)
- [Domain Model](#domain-model)
- [API Reference](#api-reference)
  - [Admin API](#admin-api)
  - [Customer API](#customer-api)
- [Authentication](#authentication)
- [Request & Response Format](#request--response-format)
- [Getting Started](#getting-started)
  - [Prerequisites](#prerequisites)
  - [Configuration](#configuration)
  - [Running the Application](#running-the-application)
- [Running Tests](#running-tests)
- [Key Features](#key-features)

---

## Architecture Overview

The solution follows **Clean Architecture** with a strict dependency rule — inner layers never depend on outer layers:

```
Domain  ←  Application  ←  Infrastructure  ←  API
```

| Layer | Responsibility |
|---|---|
| **Domain** | Entities, value objects, domain exceptions, repository interfaces |
| **Application** | Use cases, service interfaces, DTOs, validators, Result pattern |
| **Infrastructure** | EF Core, Dapper, repositories, email service, OpenAI integration |
| **API** | Controllers, middleware, request routing, HTTP response mapping |

Additional patterns in use:
- **Result pattern** — services return `Result<T>` instead of throwing exceptions for flow control
- **Repository + Unit of Work** — abstracts data access (both EF Core and Dapper)
- **Value objects** — domain validation is encapsulated in value object constructors
- **Background service** — email sending is offloaded to an in-memory queue processed by a hosted service

---

## Project Structure

```
eShop/
├── eShop.Api.Admin/            # Admin-facing REST API
│   ├── Controllers/
│   ├── Extensions/             # JWT & Swagger configuration
│   ├── Middlewares/            # Global exception handler
│   ├── Responses/              # ApiResponse<T> envelope
│   └── Program.cs
├── eShop.Api.Customer/         # Customer-facing REST API
│   ├── Controllers/
│   ├── Extensions/
│   ├── Middlewares/
│   ├── Responses/
│   └── Program.cs
├── eShop.AppHost/              # .NET Aspire orchestration host
├── eShop.Application/          # Use cases & business logic
│   ├── Constants/
│   ├── Enums/
│   ├── Exceptions/
│   ├── Extensions/
│   ├── Helpers/
│   ├── Interfaces/
│   ├── Requests/
│   ├── Responses/
│   ├── Services/
│   └── Validations/
├── eShop.Domain/               # Core domain
│   ├── Enums/
│   ├── Exceptions/
│   ├── Helpers/
│   ├── Interfaces/
│   ├── Models/
│   ├── Primitives/
│   └── ValueObjects/
├── eShop.Infrastructure/       # Data access & external services
│   ├── BackgroundServices/
│   ├── Configurations/         # EF Core entity configs
│   ├── Context/
│   ├── IoC/
│   ├── Migrations/
│   ├── Repositories/
│   └── Services/
├── eShop.Application.Tests/
├── eShop.Domain.Tests/
└── eShop.Infrastructure.Tests/
```

---

## Tech Stack

| Concern | Technology |
|---|---|
| Framework | .NET 10, ASP.NET Core |
| Orchestration | .NET Aspire 9.5 |
| ORM | Entity Framework Core 9 (SQL Server) |
| Micro-ORM | Dapper 2.1 |
| Auth | JWT Bearer (Microsoft.AspNetCore.Authentication.JwtBearer) |
| Validation | FluentValidation 12 |
| Email | MailKit / MimeKit |
| AI | OpenAI GPT-4o-mini (product description generation) |
| API Docs | Swashbuckle (Swagger UI) |
| Testing | xUnit |
| Database | SQL Server |

---

## Domain Model

### Entities

| Entity | Key Properties |
|---|---|
| **User** | FullName, Username, Email, PasswordHash, SaltKey, Role (Admin/Customer), IsDeleted |
| **Product** | ProductName, ProductDescription, UnitPrice, UnitQuantity, Image, CategoryId, IsDeleted |
| **Category** | CategoryName, Image, ParentCategoryId, Children (hierarchical tree) |
| **Basket** | UserId, BasketItems |
| **BasketItem** | ProductId, UnitQuantity |
| **Order** | UserId, TotalAmount, OrderStatus (Pending/Paid/Cancelled), OrderItems |
| **OrderItem** | ProductId, UnitQuantity, UnitPrice |
| **Comment** | ProductId, UserId, CommentText, Rating |

### Value Objects

`ProductName`, `ProductDescription`, `UnitPrice`, `UnitQuantity`, `Image`, `CategoryName`, `Email`, `Username`, `FullName`, `CommentText`

Each value object validates its own invariants on construction and throws a `DomainValidationException` on invalid input.

### Base Classes

- `BaseEntity` — `Id (Guid)`
- `AuditableBaseEntity` — extends `BaseEntity` with `Created`, `CreatedBy`, `LastModified`, `LastModifiedBy` (auto-populated on save)

### Soft Deletes

`Product`, `Category`, and `User` are never hard-deleted — they carry an `IsDeleted` flag.

---

## API Reference

All endpoints return JSON. The response envelope is:

```json
// Success
{ "data": { ... }, "message": "...", "totalCount": 25 }

// Error
{ "message": "Reason for failure" }

// No content (204)
// empty body
```

`totalCount` and `message` are omitted when null.

---

### Admin API

Base URL: `http://localhost:{port}/api`
Authentication: JWT Bearer — include `Authorization: Bearer <token>` on all protected routes.

#### Auth

| Method | Route | Auth | Description |
|---|---|---|---|
| `POST` | `/auth/admin/login` | None | Authenticate and receive a JWT token |

**Login request body:**
```json
{ "username": "admin", "password": "Admin@123" }
```

---

#### Products

| Method | Route | Description |
|---|---|---|
| `GET` | `/product` | List products (pagination + sorting via query params) |
| `GET` | `/product/{id}` | Get product details |
| `POST` | `/product` | Create a new product |
| `PUT` | `/product/{id}` | Update a product |
| `DELETE` | `/product/{id}` | Soft-delete a product |
| `GET` | `/product/{id}/edit` | Get product data for edit form |
| `GET` | `/product/generate` | Generate an AI product description (GPT-4o-mini) |

---

#### Categories

| Method | Route | Description |
|---|---|---|
| `GET` | `/category` | List categories |
| `GET` | `/category/{id}` | Get category details |
| `POST` | `/category` | Create a category |
| `PUT` | `/category/{id}` | Update a category |
| `DELETE` | `/category/{id}` | Soft-delete a category and its descendants |
| `GET` | `/category/{id}/edit` | Get category data for edit form |
| `GET` | `/category/tree` | Get full hierarchical category tree |

---

#### Dashboard

| Method | Route | Description |
|---|---|---|
| `GET` | `/dashboard/orders-today` | Total orders placed today |
| `GET` | `/dashboard/revenue-today` | Total revenue for today |
| `GET` | `/dashboard/total-customers` | Total registered customers |

---

### Customer API

Base URL: `http://localhost:{port}/api`

#### Auth

| Method | Route | Auth | Description |
|---|---|---|---|
| `POST` | `/auth/login` | None | Login and receive a JWT token |
| `POST` | `/auth/register` | None | Register a new customer account |

---

#### Products

| Method | Route | Auth | Description |
|---|---|---|---|
| `GET` | `/product` | None | Browse products with filters and pagination |
| `GET` | `/product/{id}` | None | Get product details (includes user-specific data if authenticated) |

---

#### Categories

| Method | Route | Auth | Description |
|---|---|---|---|
| `GET` | `/category/category-tree-for-menu` | None | Get category tree for navigation menu |

---

#### Basket

| Method | Route | Auth | Description |
|---|---|---|---|
| `GET` | `/basket` | Customer | Get current user's basket |
| `POST` | `/basket/merge` | Customer | Add or update items in the basket |
| `DELETE` | `/basket/items` | Customer | Clear all items from the basket |
| `DELETE` | `/basket/items/{productId}` | Customer | Remove a specific item from the basket |

---

#### Orders

| Method | Route | Auth | Description |
|---|---|---|---|
| `POST` | `/order` | Customer | Place an order (sends confirmation email) |
| `GET` | `/order` | Customer | Get order history for the current user |

---

#### Comments

| Method | Route | Auth | Description |
|---|---|---|---|
| `GET` | `/comment` | None | Get comments for a product |
| `POST` | `/comment` | Customer | Post a comment with a rating on a product |

---

## Authentication

JWT Bearer tokens are used across both APIs.

- Tokens are generated on login and must be sent in the `Authorization: Bearer <token>` header
- Role-based authorization: `Admin` role for the Admin API, `Customer` role for protected Customer endpoints
- Token lifetime validation is enabled; issuer/audience validation is disabled (configurable)

**Password requirements:** minimum 4 characters, at least one uppercase letter, one lowercase letter, one digit, and one special character.

**Password hashing:** PBKDF2-SHA256 with 100,000 iterations and a 16-byte random salt per user.

---

## Request & Response Format

### Validation errors

FluentValidation runs automatically on all requests. Invalid input returns `400 Bad Request`:

```json
{ "message": "Validation error description" }
```

### Global exception handling

Unhandled exceptions are caught by the global exception handler middleware:

| Exception | HTTP Status | Message |
|---|---|---|
| `DomainValidationException` | 400 | Domain rule violation message |
| `ExternalDependencyException` | 503 | "Service temporarily unavailable." |
| Any other | 500 | "An unexpected error occurred." |

---

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- SQL Server (local or remote)
- (Optional) [.NET Aspire workload](https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/setup-tooling) for orchestrated startup

### Configuration

Both API projects require an `appsettings.json` (or user secrets / environment variables) with the following keys:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=eShopDb;Trusted_Connection=True;"
  },
  "Jwt": {
    "SecretKey": "your-secret-key-at-least-32-characters"
  },
  "SeedAdmin": {
    "Password": "Admin@123"
  },
  "OpenAI": {
    "ApiKey": "your-openai-api-key"
  },
  "EmailSettings": {
    "Host": "smtp.example.com",
    "Port": 587,
    "Username": "your-email@example.com",
    "Password": "your-email-password"
  }
}
```

### Running the Application

**Option 1 — With .NET Aspire (recommended):**

```bash
dotnet run --project eShop.AppHost
```

This starts both APIs and the Aspire dashboard in one command.

**Option 2 — Run APIs individually:**

```bash
# Apply migrations (run once)
dotnet ef database update --project eShop.Infrastructure --startup-project eShop.Api.Admin

# Admin API
dotnet run --project eShop.Api.Admin

# Customer API
dotnet run --project eShop.Api.Customer
```

Swagger UI is available at `/swagger` in the Development environment.

A default admin user is seeded on first startup using the `SeedAdmin:Password` configuration value.

---

## Running Tests

```bash
dotnet test
```

Test projects:

| Project | Covers |
|---|---|
| `eShop.Domain.Tests` | Domain entities and value objects |
| `eShop.Application.Tests` | Application service logic |
| `eShop.Infrastructure.Tests` | Infrastructure integration tests |

---

## Key Features

- **Hierarchical categories** — categories support a parent/child tree structure with cascading soft-deletes
- **AI product descriptions** — admins can generate product descriptions via GPT-4o-mini
- **Shopping basket** — supports guest-to-authenticated basket merging
- **Order placement** — places an order, reduces basket, and sends an HTML confirmation email asynchronously
- **Email queue** — emails are queued in-memory and dispatched by a background hosted service, keeping order placement fast
- **Dual data access** — EF Core for writes and complex queries, Dapper for lightweight read queries
- **Auditing** — all auditable entities automatically track creation and modification timestamps and user
- **Clean separation of concerns** — Admin and Customer APIs share the same Application and Infrastructure layers but have completely separate controllers, DTOs, and service interfaces
