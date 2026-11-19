using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ShopNet.Application.Abstractions;
using ShopNet.Domain.Categories;
using ShopNet.Domain.Orders;
using ShopNet.Domain.Products;
using ShopNet.Infrastructure.Identity;

namespace ShopNet.Infrastructure.Persistence;

/// <summary>Catalog and orders plus the ASP.NET Core Identity tables (AspNetUsers, AspNetRoles...).</summary>
public sealed class ShopNetDbContext(DbContextOptions<ShopNetDbContext> options)
    : IdentityDbContext<AppUser, IdentityRole<int>, int>(options), IUnitOfWork
{
    public DbSet<Category> Categories => Set<Category>();

    public DbSet<Product> Products => Set<Product>();

    public DbSet<Order> Orders => Set<Order>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(ShopNetDbContext).Assembly);
    }
}
