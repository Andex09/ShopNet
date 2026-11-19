using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShopNet.Domain.Orders;
using ShopNet.Domain.Products;
using ShopNet.Domain.Users;
using ShopNet.Infrastructure.Identity;

namespace ShopNet.Infrastructure.Persistence;

internal sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Name).HasMaxLength(Product.NameMaxLength).IsRequired();
        builder.Property(p => p.Price).HasPrecision(18, 2);

        builder.HasData(
            new { Id = 1, Name = "Platzi", Price = 400000m },
            new { Id = 2, Name = "Mx 3 Mouse", Price = 350000m },
            new { Id = 3, Name = "Audifonos Logitech", Price = 200000m });
    }
}

internal sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");
        builder.HasKey(o => o.Id);
        builder.Property(o => o.UnitPrice).HasPrecision(18, 2);
        builder.Property(o => o.Subtotal).HasPrecision(18, 2);
        builder.Property(o => o.TaxRate).HasPrecision(5, 4);
        builder.Property(o => o.Tax).HasPrecision(18, 2);
        builder.Property(o => o.Total).HasPrecision(18, 2);

        // Restrict (not cascade): deleting a user or product must never erase order history.
        builder.HasOne<AppUser>().WithMany().HasForeignKey(o => o.UserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Product>().WithMany().HasForeignKey(o => o.ProductId).OnDelete(DeleteBehavior.Restrict);
    }
}

/// <summary>The two fixed roles. Seeded with constant values so migrations stay deterministic.</summary>
internal sealed class RoleConfiguration : IEntityTypeConfiguration<IdentityRole<int>>
{
    public void Configure(EntityTypeBuilder<IdentityRole<int>> builder) =>
        builder.HasData(
            new IdentityRole<int> { Id = 1, Name = Roles.Admin, NormalizedName = "ADMIN", ConcurrencyStamp = "8f9c2a51-4a3e-4b5f-9d6e-1c0b7a2e3f41" },
            new IdentityRole<int> { Id = 2, Name = Roles.Customer, NormalizedName = "CUSTOMER", ConcurrencyStamp = "3d7e4b62-9c1f-4e8a-b5d0-6f2a8c9e1b73" });
}
