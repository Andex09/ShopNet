using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ShopNet.Application.Abstractions;
using ShopNet.Infrastructure.Persistence;
using ShopNet.Infrastructure.Security;

namespace ShopNet.Infrastructure;

public static class DependencyInjection
{
    /// <summary>Connection string name. Aspire injects it as ConnectionStrings__shopnet; outside Aspire it comes from configuration.</summary>
    public const string ConnectionName = "shopnet";

    public static IHostApplicationBuilder AddInfrastructure(this IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        // Aspire integration: registers the DbContext with retries, health check and OpenTelemetry tracing.
        builder.AddSqlServerDbContext<ShopNetDbContext>(ConnectionName);
        builder.Services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ShopNetDbContext>());

        builder.Services.AddScoped<IProductRepository, ProductRepository>();
        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddScoped<IOrderRepository, OrderRepository>();

        builder.Services.AddOptions<JwtOptions>()
            .Bind(builder.Configuration.GetSection(JwtOptions.SectionName))
            .ValidateDataAnnotations()
            .Validate(JwtOptions.HasValidKeyLength, "Jwt:SigningKey must be at least 32 bytes (256 bits).")
            .ValidateOnStart();

        builder.Services.AddSingleton(TimeProvider.System);
        builder.Services.AddSingleton<IPasswordHasher, HmacPasswordHasher>();
        builder.Services.AddSingleton<ITokenService, JwtTokenService>();

        return builder;
    }

    /// <summary>Applies pending migrations. Used for local development and tests; Azure uses a migration bundle in the pipeline.</summary>
    public static async Task MigrateDatabaseAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ShopNetDbContext>();
        await db.Database.MigrateAsync(cancellationToken);
    }
}
