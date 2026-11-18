using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ShopNet.Application.Abstractions;
using ShopNet.Infrastructure.Identity;
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
        builder.Services.AddScoped<IOrderRepository, OrderRepository>();

        AddIdentity(builder);
        AddJwt(builder);

        builder.Services.AddSingleton(TimeProvider.System);

        return builder;
    }

    /// <summary>Applies pending migrations. Used for local development and tests; Azure uses a migration bundle in the pipeline.</summary>
    public static async Task MigrateDatabaseAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ShopNetDbContext>();
        await db.Database.MigrateAsync(cancellationToken);
    }

    /// <summary>Creates the Admin from <c>Seed:Admin</c> when it is configured and does not exist yet.</summary>
    public static async Task SeedAdminAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<AdminSeeder>().SeedAsync(cancellationToken);
    }

    private static void AddIdentity(IHostApplicationBuilder builder)
    {
        // Identity core only (no cookies, no UI): password hashing (PBKDF2), lockout and roles. The API issues its own JWT.
        builder.Services.AddIdentityCore<AppUser>(options =>
            {
                // Length over composition rules (NIST SP 800-63B). Same limits as the request validators.
                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;

                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);

                options.User.RequireUniqueEmail = false;
            })
            .AddRoles<IdentityRole<int>>()
            .AddEntityFrameworkStores<ShopNetDbContext>();

        builder.Services.AddScoped<IUserAccounts, UserAccounts>();

        builder.Services.AddOptions<AdminSeedOptions>()
            .Bind(builder.Configuration.GetSection(AdminSeedOptions.SectionName))
            .Validate(AdminSeedOptions.HasPasswordWhenConfigured, "Seed:Admin:Password is required when Seed:Admin:UserName is set.")
            .ValidateOnStart();
        builder.Services.AddScoped<AdminSeeder>();
    }

    private static void AddJwt(IHostApplicationBuilder builder)
    {
        builder.Services.AddOptions<JwtOptions>()
            .Bind(builder.Configuration.GetSection(JwtOptions.SectionName))
            .ValidateDataAnnotations()
            .Validate(JwtOptions.HasValidKeyLength, "Jwt:SigningKey must be at least 32 bytes (256 bits).")
            .ValidateOnStart();

        builder.Services.AddSingleton<ITokenService, JwtTokenService>();
    }
}
