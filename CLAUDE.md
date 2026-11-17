# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

```bash
# Build entire solution
dotnet build ShopNet.sln

# Run the API (from solution root or WebApi directory)
dotnet run --project ShopNet.WebApi

# Run the Blazor frontend (from solution root or Front directory)
dotnet run --project ShopNet.Front

# Apply EF Core database migrations
dotnet ef database update --project ShopNet.WebApi

# Add a new EF Core migration
dotnet ef migrations add <MigrationName> --project ShopNet.WebApi
```

**Running URLs:**
- WebApi: `https://localhost:7165` / `http://localhost:5165`
- Swagger UI: `https://localhost:7165/swagger`
- Frontend: configured to call API at `https://localhost:7165`

## Architecture

Three-project .NET 6.0 solution:

- **ShopNet.WebApi** — ASP.NET Core REST API. Contains controllers, EF Core models, repositories, services, AutoMapper profiles, and migrations.
- **ShopNet.Front** — Blazor WebAssembly SPA. Calls the API via typed HTTP services; manages auth state via a custom `AuthStateProvider` backed by Blazored session storage.
- **ShopNet.Models** — Shared class library holding all DTOs used by both projects.

### Domain

Three core entities managed by EF Core (`ShopNetContext`):
- **Producto** (product) — `ProdID`, `ProDesc`, `ProValor`
- **Usuario** (user) — `UsuID`, `UsuNombre` (unique), `UsuRol`, hashed password + salt, collection of orders
- **Pedido** (order) — join between `Usuario` and `Producto`

The database is a local SQL Server Express instance. Connection string is in `ShopNet.WebApi/appsettings.json` and targets the local `.\SQLEXPRESS` instance.

### WebApi patterns

- **Repository pattern**: each entity has an interface (`IProductoRepository`, `IUsuarioRepository`, `IPedidoRepository`) and a concrete implementation wired in `Program.cs`.
- **Service layer**: `ITokenService` handles JWT generation; auth logic lives in `IAuthRepository`.
- **AutoMapper**: profiles map between EF entities and DTOs from `ShopNet.Models`.
- **JWT auth**: tokens are validated using the `Token` key from configuration (not committed; set it locally). The JWT issuer/audience validation is configured in `Program.cs`.
- **CORS**: enabled globally to allow the Blazor frontend to call the API.

### Frontend patterns

- Pages are Blazor `@page` components under `ShopNet.Front/Pages/`.
- Each domain area has a typed service class (e.g., `ProductoService`, `PedidoService`) that wraps `HttpClient` calls to the API.
- `AuthStateProvider` extends `AuthenticationStateProvider` and reads/writes a session token via `Blazored.SessionStorage` to persist login state across page navigations.
