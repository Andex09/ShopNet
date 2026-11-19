using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Testcontainers.MsSql;

namespace ShopNet.Api.IntegrationTests;

/// <summary>
/// Runs the real API in memory against a real SQL Server 2025 container (same engine as production),
/// with migrations applied and the admin seeded on startup. Requires Docker.
/// </summary>
public sealed class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public const string SigningKey = "integration-tests-only-signing-key-0123456789abcdef";
    public const string Issuer = "shopnet-api";
    public const string Audience = "shopnet-web";
    public const string AdminUserName = "admin";
    public const string AdminPassword = "Admin-test-pass";

    private readonly MsSqlContainer _sql = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2025-latest").Build();

    public async ValueTask InitializeAsync() => await _sql.StartAsync();

    public override async ValueTask DisposeAsync()
    {
        await _sql.DisposeAsync();
        await base.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:shopnet", _sql.GetConnectionString());
        builder.UseSetting("Jwt:SigningKey", SigningKey);
        builder.UseSetting("Database:MigrateOnStartup", "true");
        builder.UseSetting("Seed:Admin:UserName", AdminUserName);
        builder.UseSetting("Seed:Admin:Password", AdminPassword);

        // TestServer has no remote IP, so every test client shares one rate-limit partition: raise the limit.
        builder.UseSetting("RateLimiting:Auth:PermitLimit", "10000");
    }
}
