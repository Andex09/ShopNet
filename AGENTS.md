# AGENTS.md

Guidance for AI coding agents (and humans) working in this repository.

## Project status

ShopNet is a 2022 technical test (store: products, users, orders) being modernized as a portfolio piece:
"2022 technical test → how I build it in 2026". The original state is preserved in git tag `v2022-original`.
Work proceeds in phases, one branch + PR per phase (`phase-N-<topic>`), and every phase must leave the
solution building and running.

| Phase | Topic | Status |
|---|---|---|
| 0 | Git baseline (`v2022-original`) | Done |
| 1 | .NET 10, central package management, OpenAPI + Scalar, secrets out of config | Done |
| 2 | Clean Architecture (Domain/Application/Infrastructure/Api), Minimal APIs, ProblemDetails, FluentValidation, OpenTelemetry, health checks, Aspire | Pending |
| 3 | ASP.NET Core Identity, hardened JWT (iss/aud/sub), CORS, rate limiting | Pending |
| 4 | Orders with multiple lines, server-side pricing, realistic catalog seed | Pending |
| 5 | Unit + integration tests (Testcontainers), CI | Pending |
| 6 | AI: semantic search, similar products, recommendations (Azure OpenAI embeddings + Azure SQL `VECTOR`) | Pending |
| 7 | Terraform (azurerm): Container Apps, Azure SQL, Static Web Apps, Key Vault, AI Foundry | Pending |
| 8 | GitHub Actions CI/CD with OIDC | Pending |
| 9 | Blazor UI polish | Pending |
| 10 | README, ADRs, "2022 vs 2026" | Pending |

## Conventions

- **Code in English** (identifiers, comments, commits). **App UI text in Spanish.**
- Never commit secrets. Local secrets live in `dotnet user-secrets`; in Azure, in Key Vault.
- Package versions are managed centrally in `Directory.Packages.props`; project files reference packages without versions.
- Target framework and shared build settings live in `Directory.Build.props`. The SDK is pinned to .NET 10 (LTS) by `global.json`, even if newer SDKs are installed.
- Do not add MediatR, AutoMapper or FluentAssertions (commercial licenses since 2025). AutoMapper is still referenced by the legacy API and is removed in phase 2.

## Commands

```bash
# Restore local tools (dotnet-ef)
dotnet tool restore

# Build the whole solution
dotnet build ShopNet.slnx

# One-time: JWT signing key (min 32 bytes) for local development
dotnet user-secrets set Token "<random-64-byte-base64>" --project ShopNet.WebApi

# Apply EF Core migrations (local SQL Server Express, database "ShopNet")
dotnet ef database update --project ShopNet.WebApi

# Add a migration
dotnet ef migrations add <MigrationName> --project ShopNet.WebApi

# Run the API (Scalar UI at https://localhost:7165/scalar/v1, OpenAPI at /openapi/v1.json)
dotnet run --project ShopNet.WebApi

# Run the Blazor frontend (https://localhost:7043, calls the API at https://localhost:7165)
dotnet run --project ShopNet.Front
```

The API redirects HTTP to HTTPS; use the HTTPS URLs.

## Current architecture (legacy, until phase 2)

Three projects in `ShopNet.slnx`, all on .NET 10:

- **ShopNet.WebApi**: ASP.NET Core controllers, EF Core models (`ShopNetContext`), repositories, `TokenService` (JWT), AutoMapper profiles, migrations.
- **ShopNet.Front**: Blazor WebAssembly SPA. Typed HTTP services per area; `AuthStateProvider` keeps the session (token + user) in Blazored session storage.
- **ShopNet.Models**: shared DTOs, no package dependencies.

Entities: `Producto`, `Usuario` (unique `UsuNombre`, role `Basic`/`Admin`, HMAC password hash), `Pedido` (one product per order).
Connection string `cnShopNet` in `ShopNet.WebApi/appsettings.json` targets `.\SQLEXPRESS`.

Known issues are tracked in the modernization plan and fixed in the phases above. Do not fix them ad hoc
outside their phase: for example, client-side order pricing (phase 4), password hashing (phase 3) and nullable warnings in legacy code (phase 2).
There is no way to create the first Admin from the app yet (fixed in phase 3); locally, promote a user with
`UPDATE Usuarios SET UsuRol = 'Admin' WHERE UsuNombre = '<name>'`.
