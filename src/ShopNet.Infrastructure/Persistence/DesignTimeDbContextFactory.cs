using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ShopNet.Infrastructure.Persistence;

/// <summary>Lets <c>dotnet ef</c> create migrations without starting the API. No database connection is needed to add a migration.</summary>
internal sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ShopNetDbContext>
{
    public ShopNetDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("SHOPNET_DESIGN_CONNECTION")
            ?? "Server=.\\SQLEXPRESS;Database=ShopNetDev;Trusted_Connection=True;TrustServerCertificate=True";

        var options = new DbContextOptionsBuilder<ShopNetDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new ShopNetDbContext(options);
    }
}
