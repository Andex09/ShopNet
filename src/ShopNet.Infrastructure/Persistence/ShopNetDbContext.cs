using Microsoft.EntityFrameworkCore;
using ShopNet.Application.Abstractions;
using ShopNet.Domain.Orders;
using ShopNet.Domain.Products;
using ShopNet.Domain.Users;

namespace ShopNet.Infrastructure.Persistence;

public sealed class ShopNetDbContext(DbContextOptions<ShopNetDbContext> options) : DbContext(options), IUnitOfWork
{
    public DbSet<Product> Products => Set<Product>();

    public DbSet<User> Users => Set<User>();

    public DbSet<Order> Orders => Set<Order>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ShopNetDbContext).Assembly);
}
