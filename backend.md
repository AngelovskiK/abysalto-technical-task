# Backend — Basket Service

## Architecture

The Basket Service follows a **Clean Architecture** approach with a soft DDD flavour. The domain is kept pure and free of infrastructure concerns; the application layer orchestrates use cases via CQRS; persistence and the API are outer-layer implementation details.

```
BasketService/
├── src/
│   ├── BasketService.Api/           ← ASP.NET Core entry point, controllers, DI wiring
│   ├── BasketService.Application/   ← Commands, queries, handlers (CQRS), interfaces
│   ├── BasketService.Domain/        ← Entities, value objects, domain events, rules
│   └── BasketService.Persistence/   ← EF Core DbContext, repositories, Redis client
└── tests/
    ├── BasketService.Tests.Application/  ← Unit tests for handlers and validators
    └── BasketService.Tests.Domain/       ← Unit tests for domain logic
```

### Dependency flow (innermost ← outermost)

```
Domain ← Application ← Persistence
                      ← Api
```

The Domain and Application layers have **zero infrastructure dependencies**.

---

## Technology Choices

| Concern | Package / Technology | Rationale |
|---|---|---|
| Framework | .NET 10 ASP.NET Core | Latest LTS; first-class minimal API + controller support |
| API documentation | `Microsoft.AspNetCore.OpenApi` + `Scalar.AspNetCore` | Built-in OpenAPI spec generation; Scalar replaces Swagger UI with a modern, actively maintained UI |
| CQRS / Mediator | `Mediator.SourceGenerator` + `Mediator.Abstractions` | Source-generator–based mediator; zero reflection at runtime, faster than MediatR, same pipeline concept |
| Validation | `FluentValidation` | Fluent, testable validation rules; integrates cleanly with Mediator pipeline behaviours |
| ORM | `Npgsql.EntityFrameworkCore.PostgreSQL` | EF Core provider for PostgreSQL; code-first migrations; parameterised queries by default (no SQL injection) |
| Cache / Redis | `StackExchange.Redis` | Official Redis .NET client; used for basket read/write, idempotency keys, rate limiting counters |
| Health checks | `Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore` | Readiness probes check DB connectivity; liveness probe is built-in |
| Observability | `OpenTelemetry.Extensions.Hosting` `OpenTelemetry.Instrumentation.AspNetCore` `OpenTelemetry.Instrumentation.Http` `OpenTelemetry.Exporter.OpenTelemetryProtocol` | Vendor-neutral OTLP traces, metrics, and logs — forwarded to Aspire Dashboard locally, Azure Application Insights in higher environments |
| Testing | `xunit`, `FluentAssertions`, `NSubstitute` | xUnit as test runner; FluentAssertions for readable assertions; NSubstitute for mocking interfaces |

---

## Local Development

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (for infrastructure services)
- EF Core CLI tools: `dotnet tool install --global dotnet-ef`

### Start infrastructure

From the repo root:

```bash
docker compose up -d
```

This starts PostgreSQL, Redis, Azurite, and the Aspire Dashboard (OpenTelemetry UI at http://localhost:18888).

### Run the API

```bash
cd backend/BasketService/src/BasketService.Api
dotnet run
```

API: https://localhost:7054  
Scalar docs: https://localhost:7054/scalar

### Connection strings (Development)

Set in `BasketService.Api/appsettings.Development.json` or via environment variables:

| Key | Local value |
|---|---|
| `ConnectionStrings__BasketDb` | `Host=localhost;Port=5432;Database=basketdb;Username=retail;Password=retail_dev_pw` |
| `ConnectionStrings__Redis` | `localhost:6379` |
| `OTEL_EXPORTER_OTLP_ENDPOINT` | `http://localhost:4317` |
| `OTEL_EXPORTER_OTLP_PROTOCOL` | `grpc` |
| `OTEL_SERVICE_NAME` | `basket-api` |

### Database migrations

```bash
# Create a new migration (run from repo root)
dotnet ef migrations add <MigrationName> \
  --project backend/BasketService/src/BasketService.Persistence \
  --startup-project backend/BasketService/src/BasketService.Api

# Apply migrations
dotnet ef database update \
  --project backend/BasketService/src/BasketService.Persistence \
  --startup-project backend/BasketService/src/BasketService.Api
```

### Run tests

```bash
cd backend/BasketService
dotnet test
```
