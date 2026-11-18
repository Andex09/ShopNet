# AGENTS.md

Guidance for AI coding agents (and humans) working in this repository.

## Project status

ShopNet is a 2022 technical test (store: products, users, orders) being modernized as a portfolio piece:
"2022 technical test → how I build it in 2026". The original state is preserved in git tag `v2022-original`.
Work proceeds in phases, one branch + PR per phase (`phase-N-<topic>`), and every phase must leave the
solution building, tested and running.

| Phase | Topic | Status |
|---|---|---|
| 0 | Git baseline (`v2022-original`) | Done |
| 1 | .NET 10, central package management, OpenAPI + Scalar, secrets out of config | Done |
| 2 | Clean Architecture, Minimal APIs, ProblemDetails, FluentValidation, OpenTelemetry, health checks, Aspire, test projects | Done |
| 3 | ASP.NET Core Identity, hardened JWT (iss/aud), admin seed, rate limiting | Pending |
| 4 | Orders with multiple lines, realistic catalog seed | Pending |
| 5 | Full test coverage, CI | Pending |
| 6 | AI: semantic search, similar products, recommendations (Azure OpenAI embeddings + Azure SQL `VECTOR`) | Pending |
| 7 | Terraform (azurerm): Container Apps, Azure SQL, Static Web Apps, Key Vault, AI Foundry | Pending |
| 8 | GitHub Actions CI/CD with OIDC | Pending |
| 9 | Blazor UI polish | Pending |
| 10 | README, ADRs, "2022 vs 2026" | Pending |

## Conventions

- **Code in English** (identifiers, comments, commits, API messages). **App UI text in Spanish.**
- Never commit secrets. Local secrets live in `dotnet user-secrets`; in Azure, in Key Vault.
- Package versions are managed centrally in `Directory.Packages.props`; project files reference packages without versions.
- `Directory.Build.props` sets net10.0, nullable, latest analyzers and **warnings as errors**. The SDK is pinned to .NET 10 (LTS) by `global.json`, which also opts `dotnet test` into Microsoft.Testing.Platform.
- Do not add MediatR, AutoMapper or FluentAssertions (commercial licenses). Map explicitly (`Application/Features/Mappings.cs`).
- Culture: the dev machine runs es-CO. Parse/format numbers with the invariant culture in code and attributes (e.g. `RangeAttribute.ParseLimitsInInvariantCulture`).

## Architecture

```
src/
  ShopNet.Domain           Entities (Product, Order) and their invariants, role names. No dependencies.
  ShopNet.Application      Use cases: one *Handler class per use case, grouped by feature; FluentValidation validators;
                           ports (IProductRepository, IUserAccounts, IUnitOfWork, ITokenService, ICurrentUser...).
  ShopNet.Infrastructure   EF Core (ShopNetDbContext, configurations, migrations, repositories), ASP.NET Core Identity
                           (users, roles, password hashing, lockout) behind IUserAccounts, JWT issuing, admin seed.
  ShopNet.Api              Minimal API endpoints per feature, global exception handler -> ProblemDetails,
                           validation endpoint filter, JWT auth, CORS, OpenAPI + Scalar.
  ShopNet.Contracts        Request/response DTOs shared by Api and Web. No dependencies.
  ShopNet.Web              Blazor WebAssembly client (UI in Spanish). ApiClient + typed services; no HttpClient in components.
  ShopNet.ServiceDefaults  OpenTelemetry, health checks (/health/live, /health/ready), resilience.
  ShopNet.AppHost          .NET Aspire, local development only (SQL Server 2025 container + Api + Web).
tests/
  ShopNet.UnitTests                 Domain + Application (xUnit v3, NSubstitute, Shouldly).
  ShopNet.Api.IntegrationTests      WebApplicationFactory + Testcontainers (SQL Server 2025). Requires Docker.
```

Dependencies point inwards: Api → Application/Infrastructure → Domain. Endpoints never touch the DbContext.
Expected errors are exceptions (`NotFoundException` 404, `ConflictException` 409, `DomainException` 400) translated by
`GlobalExceptionHandler`; anything else is a generic 500 that is logged, never exposed.

Order amounts are always calculated by the server (`Order.Create/Change`) from the stored product price and the
`Orders:TaxRate` setting. Deleting a product or user with orders returns 409 (FKs are `Restrict`).

## Commands

```bash
# Restore local tools (dotnet-ef)
dotnet tool restore

# One-time: JWT signing key (min 32 bytes) and the seeded admin's password (min 8 characters) for local development
dotnet user-secrets set "Jwt:SigningKey" "<random-64-byte-base64>" --project src/ShopNet.Api
dotnet user-secrets set "Seed:Admin:Password" "<password>" --project src/ShopNet.Api

# Run everything with Aspire (requires Docker): SQL Server container, API, Blazor client and the Aspire dashboard.
# The admin password is the AppHost parameter "admin-password" (asked in the dashboard, or set it once):
dotnet user-secrets set "Parameters:admin-password" "<password>" --project src/ShopNet.AppHost
dotnet run --project src/ShopNet.AppHost

# Or run the API alone against local SQL Server Express (database ShopNetDev, migrated on startup)
dotnet run --project src/ShopNet.Api
dotnet run --project src/ShopNet.Web

# Build and test (integration tests require Docker)
dotnet build ShopNet.slnx
dotnet test --solution ShopNet.slnx

# Add an EF Core migration
dotnet ef migrations add <Name> --project src/ShopNet.Infrastructure --output-dir Persistence/Migrations
```

URLs: API `https://localhost:7165` (Scalar at `/scalar/v1`, OpenAPI at `/openapi/v1.json`), Web `https://localhost:7043`.
The API redirects HTTP to HTTPS. Main routes: `/api/auth/*`, `/api/products`, `/api/users` (admin), `/api/orders` (admin),
`/api/me/orders` (signed-in user).

Auth: ASP.NET Core Identity stores users and roles (`Admin`, `Customer`); passwords are PBKDF2 hashes and 5 failed
logins lock the account for 15 minutes. The API issues its own JWT (HS256, 60 minutes, `iss`/`aud` validated).
On startup the API creates the Admin from `Seed:Admin` (`UserName` is `admin` in Development) if no user has that name.
Users migrated from the pre-Identity `Users` table keep their id and role but have no password: the admin sets one.
